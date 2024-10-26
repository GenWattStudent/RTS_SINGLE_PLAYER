using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : NetworkBehaviour
{
    public NavMeshAgent agent;
    public Vector3 destinationAfterSpawn = Vector3.zero;
    public bool isMoving = false;
    public bool isReachedDestinationAfterSpawn = false;

    private Unit unit;
    private ResourceUsage resourceUsage;
    private Stats stats;

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

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        unit = GetComponent<Unit>();
        resourceUsage = GetComponent<ResourceUsage>();
        stats = GetComponent<Stats>();

        SetNavMeshValues();
    }

    private void Start()
    {
        stats.stats.OnListChanged += StatsChanged;
    }

    private void StatsChanged(NetworkListEvent<Stat> changeEvent)
    {
        SetNavMeshValues();
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveToServerRpc(Vector3 destination)
    {
        if (!unit.IsUpgrading.Value && (resourceUsage == null || !resourceUsage.isInDebt) && NavMesh.SamplePosition(destination, out NavMeshHit hit, 30f, NavMesh.AllAreas))
        {
            Debug.Log("Move to " + destination);
            agent.SetDestination(hit.position);
            agent.isStopped = false;
        }
    }

    public void SetDestinationAfterSpawn(Vector3 destination)
    {
        destinationAfterSpawn = destination;
        isReachedDestinationAfterSpawn = false;
    }

    public void MoveToWithoutNavMesh(Vector3 destination)
    {
        // move to destination without nav mesh
        var direction = (destination - transform.position).normalized;
        transform.position += direction * stats.GetStat(StatType.Speed) * Time.deltaTime;

        if (Vector3.Distance(transform.position, destination) < 0.5f)
        {
            isReachedDestinationAfterSpawn = true;
            agent.enabled = true;
            isMoving = false;
        }
    }

    public void Stop()
    {
        agent.isStopped = true;
    }

    private void Update()
    {
        if (!IsServer) return;

        if (!isReachedDestinationAfterSpawn)
        {
            MoveToWithoutNavMesh(destinationAfterSpawn);
            isMoving = true;
        }

        if (!agent.enabled) return;

        if (resourceUsage != null && resourceUsage.isInDebt)
        {
            Stop();
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            isMoving = false;
        }
        else
        {
            isMoving = true;
        }
    }
}
