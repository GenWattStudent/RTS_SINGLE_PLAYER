using Unity.Netcode;
using UnityEngine;

public class Healer : NetworkBehaviour
{
    public Damagable target;
    public DamagableSo damagableSo;
    private float healRate = 1f;
    private UnitMovement unitMovement;
    private Laser laser;
    private float healPoints = 0f;

    public void Heal(Damagable target)
    {
        if (target.stats.GetStat(StatType.Health) >= target.stats.GetStat(StatType.MaxHealth))
        {
            SetTarget(null);
            return;
        }

        target.TakeDamage(healPoints * Time.deltaTime * -1);
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

    [ServerRpc(RequireOwnership = false)]
    public void SetTargetToNullServerRpc()
    {
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

        target.OnDead += () => SetTarget(null);

        SetLaserTargetClientRpc(target.GetComponent<NetworkObject>());
    }

    [ClientRpc]
    private void SetLaserTargetClientRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject networkObject))
        {
            laser.SetTarget(networkObject.GetComponent<Damagable>());
        }
    }

    [ClientRpc]
    private void SetLaserTargetToNullClientRpc()
    {
        laser.SetTarget(null);
    }

    private bool IsInRange()
    {
        if (target == null) return false;
        var colliders = Physics.OverlapSphere(transform.position, damagableSo.attackRange);
        foreach (var collider in colliders)
        {
            if (collider.gameObject == target.gameObject)
            {
                return true;
            }
        }

        return false;
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

    void Start()
    {
        unitMovement = GetComponent<UnitMovement>();
        laser = GetComponent<Laser>();
        healRate = damagableSo.attackSpeed;
        healPoints = damagableSo.attackDamage / healRate;
    }

    void Update()
    {
        if (!IsServer) return;
        if (target == null) return;

        if (!IsInRange())
        {
            MoveToTarget();
        }
        else
        {
            Heal(target);
        }

        if (unitMovement != null) unitMovement.RotateToTarget(target.transform.position);
    }
}
