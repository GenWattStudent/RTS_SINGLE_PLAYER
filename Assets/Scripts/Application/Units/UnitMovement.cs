using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : NetworkBehaviour
{
    public NavMeshAgent agent;
    public Vector3 destinationAfterSpawn = Vector3.zero;
    public bool isMoving = false;
    public Vector3 Destination;

    private Unit unit;
    private Stats stats;
    private RTSObjectsManager rtsObjectManager;

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

    public override void OnNetworkDespawn()
    {
        if (stats != null)
        {
            stats.BaseStats.OnListChanged -= StatsChanged;
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        unit = GetComponent<Unit>();
        stats = GetComponent<Stats>();
        agent.stoppingDistance = 0.1f;
    }

    private void Start()
    {
        SetNavMeshValues();
        stats.BaseStats.OnListChanged += StatsChanged;
        rtsObjectManager = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<RTSObjectsManager>();
    }

    private void StatsChanged(NetworkListEvent<Stat> changeEvent)
    {
        Debug.Log("Stats changed: " + transform + " " + stats.GetStat(StatType.Speed));
        SetNavMeshValues();
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveToServerRpc(Vector3 destination)
    {
        if (!unit.IsUpgrading.Value && NavMesh.SamplePosition(destination, out NavMeshHit hit, 30f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.acceleration = stats.GetStat(StatType.Acceleration);
            Destination = hit.position;
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

        if (!agent.isStopped && agent.hasPath && Vector3.Distance(transform.position, Destination) <= agent.stoppingDistance)
        {
            Stop();
        }
        // is moving
        if (agent.velocity.magnitude > 0.1f)
        {
            isMoving = true;
            rtsObjectManager.UpdateTree();
        }
        else
        {
            isMoving = false;
        }
    }
}
