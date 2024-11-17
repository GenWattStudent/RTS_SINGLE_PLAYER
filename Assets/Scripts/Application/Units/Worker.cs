using Unity.Netcode;
using UnityEngine;

public class Worker : NetworkBehaviour
{
    public IWorkerConstruction construction;
    public Unit unit;
    public Laser laser;
    public Stats stats;

    private bool isBuilding = false;
    private UnitMovement unitMovement;

    [ServerRpc(RequireOwnership = false)]
    private void ActivateLaserServerRpc()
    {
        var targetNo = (construction as NetworkBehaviour).GetComponent<NetworkObject>();
        ActivateLaserClientRpc(targetNo);
    }

    [ClientRpc]
    private void ActivateLaserClientRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var construction = no.GetComponent<IWorkerConstruction>();
            laser.isAttacking = false;
            laser.SetTarget(construction.transform);
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
        return Vector3.Distance(transform.position, (construction as NetworkBehaviour).transform.position);
    }

    private void StartConstruction()
    {
        construction.AddWorker(this);
        isBuilding = true;
        ActivateLaserServerRpc();
    }

    private void HandleFinish()
    {
        StopConstructionServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopConstructionServerRpc(bool removeFromList = true)
    {
        if (construction == null) return;
        if (removeFromList) construction.RemoveWorker(this);
        isBuilding = false;

        construction.OnFinshed -= HandleFinish;
        construction = null;
        DeactivateLaserServerRpc();
    }

    private void MoveToConstruction()
    {
        if (unitMovement != null && construction != null && DistanceToConstruction() > stats.GetStat(StatType.BuildingDistance))
        {
            unitMovement.MoveToServerRpc(construction.transform.position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveToConstructionServerRpc(NetworkObjectReference nor)
    {
        if (nor.TryGet(out NetworkObject no))
        {
            var construction = no.GetComponent<IWorkerConstruction>();

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

            this.construction = construction;
            this.construction.OnFinshed += HandleFinish;

            MoveToConstruction();
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

        NetworkManager.Singleton.NetworkTickSystem.Tick += UpdateWorker;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        StopConstructionServerRpc();

        NetworkManager.Singleton.NetworkTickSystem.Tick -= UpdateWorker;
    }

    private void UpdateWorker()
    {
        if (!IsServer) return;
        if (construction == null) return;

        var distance = DistanceToConstruction();
        unitMovement.RotateToTarget(construction.transform.position);

        var buildingDistance = stats.GetStat(StatType.BuildingDistance);

        if (distance <= buildingDistance && !isBuilding)
        {
            unitMovement.Stop();

            StartConstruction();
        }

        if (isBuilding)
        {
            MoveToConstruction();
        }
    }
}
