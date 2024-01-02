using UnityEngine;
using UnityEngine.AI;

public class EnemyUnitMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Damagable damagable;
    private Damagable targetDamagable;
    private float updatePositionTimer = 0f;
    [SerializeField] private float updatePositionTime = 0.4f;
    [SerializeField] private float offset = 1f;

    private void SetTarget(Damagable target) {
        targetDamagable = target;

        if (target == null) return;
        target.OnDead += OnDead;
    }

    private void OnDead() {
        SetTarget(null);
        Destroy(gameObject);
    }

    private Vector3 GetPositionInRange(Vector3 targetPosition, float offset) {
        var distance = Vector3.Distance(transform.position, targetPosition);
        var distanceToMove = distance - damagable.damagableSo.attackRange + offset;
        var direction = (targetPosition - transform.position).normalized;
        var destination = transform.position + direction * distanceToMove;

        if (NavMesh.SamplePosition(destination, out var hit, 8.0f, NavMesh.AllAreas)) {
            destination = hit.position;
        } else {
            destination = transform.position;
        }

        return destination;
    }

    private void FindClosestPlayerTarget() {
        var players = PlayerController.Instance.units;
        if (players.Count == 0) return;

        var closestPlayer = players[0];
        if (closestPlayer == null) return;
        
        var closestDistance = Vector3.Distance(transform.position, closestPlayer.transform.position);

        foreach (var player in players) {
            var distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestPlayer = player;
            }
        }
        if (closestPlayer == null) return;

        targetDamagable = closestPlayer.GetComponent<Damagable>();
        MoveTo(GetPositionInRange(closestPlayer.transform.position, offset));
    }

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = true;
        damagable = GetComponent<Damagable>();
    }

    public void MoveTo(Vector3 destination) {
        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    public void Stop() {
        agent.isStopped = true;
    }

    private void Update() {
        if (damagable.isDead || agent == null) return; 

        if (targetDamagable == null) {
            FindClosestPlayerTarget();
            return;
        }

        if (targetDamagable == null) return;

        updatePositionTimer += Time.deltaTime;

        if (updatePositionTimer >= updatePositionTime) {
            updatePositionTimer = 0f;
            MoveTo(GetPositionInRange(targetDamagable.transform.position, offset));
        }

        if (agent.remainingDistance < agent.stoppingDistance) {
            Stop();
        }

        if (agent.remainingDistance <= agent.stoppingDistance) {
            agent.isStopped = true;
        }
    }
}
