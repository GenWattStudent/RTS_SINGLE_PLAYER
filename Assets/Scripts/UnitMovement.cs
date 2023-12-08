using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Unit unit;

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
        SetNavMeshValues();
    }

    public void MoveTo(Vector3 destination)
    {
        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    public void Stop()
    {
        agent.isStopped = true;
    }
}
