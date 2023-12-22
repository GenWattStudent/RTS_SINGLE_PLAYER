using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    private Unit unit;
    private Animator animator;

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
        agent.SetDestination(destination);
        agent.isStopped = false;
        agent.avoidancePriority = Random.Range(1, 100);
    }

    public void Stop()
    {
        agent.isStopped = true;
    }

    private void Update()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            SetIsWalking(false);
        } else {
            SetIsWalking(true);
        }
    }
}
