using Unity.Netcode;
using UnityEngine;

public class Worker : NetworkBehaviour
{
    public Construction construction;
    public Unit unit;
    public Laser laser;
    public Stats stats;

    private bool isBuilding = false;
    private UnitMovement unitMovement;

    [ServerRpc(RequireOwnership = false)]
    private void ActivateLaserServerRpc()
    {
        var targetNo = construction.GetComponent<NetworkObject>();
        ActivateLaserClientRpc(targetNo);
    }

    [ClientRpc]
    private void ActivateLaserClientRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var construction = no.GetComponent<Construction>();
            laser.isAttacking = false;
            laser.SetTarget(construction.GetComponent<Damagable>());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeactivateLaserServerRpc()
    {
        DeactivateLaserClientRpc();
    }

    [ClientRpc]
    private void DeactivateLaserClientRpc()
    {
        laser.SetTarget(null);
    }

    private float DistanceToConstruction()
    {
        return Vector3.Distance(transform.position, construction.transform.position);
    }

    private void StartConstruction()
    {
        construction.AddWorker(unit);
        isBuilding = true;
        ActivateLaserServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopConstructionServerRpc(bool removeFromList = true)
    {
        if (construction == null) return;
        if (removeFromList) construction.RemoveWorker(unit);
        isBuilding = false;
        construction = null;
        DeactivateLaserServerRpc();
    }

    private void MoveToConstruction(Construction construction)
    {
        this.construction = construction;

        if (unitMovement != null && DistanceToConstruction() > unit.unitSo.buildingDistance)
        {
            unitMovement.MoveToServerRpc(construction.transform.position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveToConstructionServerRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var construction = no.GetComponent<Construction>();
            // if worker is building something else
            if (this.construction != null)
            {
                StopConstructionServerRpc();
            }
            // if is clicked on building that worker currently building
            if (this.construction == construction)
            {
                return;
            }

            MoveToConstruction(construction);
        }
    }

    private void Awake()
    {
        unit = GetComponent<Unit>();
        unitMovement = GetComponent<UnitMovement>();
        stats = GetComponent<Stats>();
        laser = GetComponent<Laser>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;

        stats.AddStat(StatType.Damage, laser.laserSo.Damage);
        stats.AddStat(StatType.AttackSpeed, laser.laserSo.AttackSpeed);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        StopConstructionServerRpc();
    }

    private void Update()
    {
        if (!IsServer || construction == null) return;

        var distance = DistanceToConstruction();
        unitMovement.RotateToTarget(construction.transform.position);

        if (distance <= unit.unitSo.buildingDistance && !isBuilding)
        {
            if (unitMovement != null)
            {
                unitMovement.Stop();
            }

            StartConstruction();
        }

        if (isBuilding && distance > unit.unitSo.buildingDistance)
        {
            MoveToConstruction(construction);
        }
    }
}
