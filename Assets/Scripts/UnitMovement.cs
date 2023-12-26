using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    private Unit unit;
    private Animator animator;
    public bool isReachedDestinationAfterSpawn = false;
    public Vector3 destinationAfterSpawn = Vector3.zero;

    private void SetNavMeshValues()
    {
        agent.speed = unit.unitSo.speed;
        agent.acceleration = unit.unitSo.acceleration;
    }

    public void RotateToTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, unit.unitSo.rotateSpeed * Time.deltaTime);
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        unit = GetComponent<Unit>();
        animator = GetComponent<Animator>();
        SetNavMeshValues();
    }

    private void SetIsWalking(bool isWalking)
    {
        if (animator == null) return;
        animator.SetBool("isWalking", isWalking);
    }

    public void MoveTo(Vector3 destination)
    {
        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 30f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            agent.isStopped = false;
            agent.avoidancePriority = Random.Range(1, 100);
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
        transform.position += direction * unit.unitSo.speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, destination) < 0.5f)
        {
            isReachedDestinationAfterSpawn = true;
            agent.enabled = true;
            SetIsWalking(false);
        }
    }

    public void Stop()
    {
        agent.isStopped = true;
    }

    private void Update()
    {
        if (!isReachedDestinationAfterSpawn) {
            MoveToWithoutNavMesh(destinationAfterSpawn);
            SetIsWalking(true);
        }

        if (!agent.enabled) return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            SetIsWalking(false);
        } else {
            SetIsWalking(true);
        }
    }
}
