using Unity.Netcode;
using UnityEngine;

public class Healer : NetworkBehaviour
{
    public Damagable target;
    public DamagableSo damagableSo;

    private UnitMovement unitMovement;
    private Laser laser;
    private Stats stats;
    private float currentHealTimer;

    public void Heal(Damagable target)
    {
        if (target.stats.GetStat(StatType.Health) >= target.stats.GetStat(StatType.MaxHealth))
        {
            SetTarget(null);
            return;
        }

        var healPoints = stats.GetStat(StatType.Damage);
        var totalHealPoints = Mathf.RoundToInt(healPoints * Time.deltaTime);

        target.TakeDamage(Mathf.RoundToInt(totalHealPoints * -1));
        currentHealTimer = stats.GetStat(StatType.AttackSpeed);

        if (!laser.IsLaserOn)
        {
            SetLaserTargetClientRpc(target.GetComponent<NetworkObject>());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTargetServerRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject networkObject))
        {
            var damagable = networkObject.GetComponent<Damagable>();
            SetTarget(damagable);
        }
    }

    private void HandleDeath(Damagable damagable)
    {
        SetTarget(null);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTargetToNullServerRpc()
    {
        if (target != null)
        {
            target.OnDead -= HandleDeath;
        }

        SetTarget(null);
    }

    private void SetTarget(Damagable target)
    {
        this.target = target;

        if (target == null)
        {
            SetLaserTargetToNullClientRpc();
            return;
        }

        target.OnDead += HandleDeath;
    }

    [ClientRpc]
    private void SetLaserTargetClientRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject networkObject))
        {
            laser.SetTarget(networkObject.transform);
        }
    }

    [ClientRpc]
    private void SetLaserTargetToNullClientRpc()
    {
        laser.SetTarget(null);
    }

    private bool IsInRange()
    {
        Vector3 closestPointOnTargetUnit = GetClosestPointOnCollider(target, transform.position);
        float distance = Vector3.Distance(transform.position, closestPointOnTargetUnit);

        return distance <= stats.GetStat(StatType.BuildingDistance);
    }

    private Vector3 GetClosestPointOnCollider(Damagable damagable, Vector3 position)
    {
        Collider collider = damagable.GetComponent<Collider>();
        if (collider != null)
        {
            return collider.ClosestPoint(position);
        }
        return damagable.transform.position;
    }

    private void MoveToTarget()
    {
        if (target == null) return;
        // move to target to be in distance to heal take offset in thye account
        var offset = 1f;
        var distance = Vector3.Distance(transform.position, target.transform.position);
        var distanceToMove = distance - damagableSo.attackRange + offset;
        var direction = (target.transform.position - transform.position).normalized;
        var destination = transform.position + direction * distanceToMove;

        unitMovement.MoveToServerRpc(destination);
    }

    private void Start()
    {
        unitMovement = GetComponent<UnitMovement>();
        laser = GetComponent<Laser>();
        stats = GetComponent<Stats>();

        currentHealTimer = stats.GetStat(StatType.AttackSpeed);
    }

    private void Update()
    {
        if (!IsServer) return;

        currentHealTimer -= Time.deltaTime;

        if (target == null) return;

        if (unitMovement != null) unitMovement.RotateToTarget(target.transform.position);

        if (!IsInRange())
        {
            MoveToTarget();
        }
        else if (currentHealTimer <= 0)
        {
            Heal(target);
        }
    }
}
