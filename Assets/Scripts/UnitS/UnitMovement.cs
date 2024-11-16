using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : NetworkBehaviour
{
    public NavMeshAgent agent;
    public Vector3 destinationAfterSpawn = Vector3.zero;
    public bool isReachedDestinationAfterSpawn = false;
    public bool isMoving = false;

    private Unit unit;
    private ResourceUsage resourceUsage;
    private Stats stats;
    private Vector3 oldPosition;

    private void SetNavMeshValues()
    {
        agent.speed = stats.GetStat(StatType.Speed);
        agent.acceleration = stats.GetStat(StatType.Acceleration);
    }

    public void RotateToTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, unit.unitSo.rotateSpeed * Time.deltaTime);
    }

    private void HandleDebt(bool isInDebt)
    {
        if (isInDebt)
        {
            agent.speed = stats.GetStat(StatType.Speed) / 2;
        }
        else
        {
            agent.speed = stats.GetStat(StatType.Speed);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (resourceUsage != null)
        {
            resourceUsage.OnDebtChanged -= HandleDebt;
        }

        if (stats != null)
        {
            stats.BaseStats.OnListChanged -= StatsChanged;
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        unit = GetComponent<Unit>();
        resourceUsage = GetComponent<ResourceUsage>();
        stats = GetComponent<Stats>();

        if (resourceUsage != null)
        {
            resourceUsage.OnDebtChanged += HandleDebt;
        }

        SetNavMeshValues();
    }

    private void Start()
    {
        SetNavMeshValues();
        stats.BaseStats.OnListChanged += StatsChanged;
    }

    private void StatsChanged(NetworkListEvent<Stat> changeEvent)
    {
        if (transform.name == "Worker(Clone)")
        {
            Debug.Log("Speed: " + stats.GetStat(StatType.Speed) + transform.name);
        }

        SetNavMeshValues();
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveToServerRpc(Vector3 destination)
    {
        Debug.Log("Move to server rpc: " + transform + " Upgrading: " + unit.IsUpgrading.Value + " ResourceUsage: " + (resourceUsage == null || !resourceUsage.isInDebt).ToString());
        if (!unit.IsUpgrading.Value && (resourceUsage == null || !resourceUsage.isInDebt) && NavMesh.SamplePosition(destination, out NavMeshHit hit, 30f, NavMesh.AllAreas))
        {
            Debug.Log("Move to: " + destination);
            agent.isStopped = false;
            agent.acceleration = stats.GetStat(StatType.Acceleration);
            agent.SetDestination(hit.position);
        }
    }

    public void Stop()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();
    }

    private void Update()
    {
        if (!IsServer) return;

        oldPosition = transform.position;

        if (!agent.isStopped && agent.hasPath && agent.remainingDistance <= 0.08f)
        {
            Debug.Log("Stop: " + agent.remainingDistance);
            Stop();
        }
        // is moving
        if (agent.velocity.magnitude > 0.1f)
        {
            isMoving = true;
            RTSObjectsManager.quadtree.UpdateUnit(RTSObjectsManager.Units);
        }
        else
        {
            isMoving = false;
        }
    }
}
