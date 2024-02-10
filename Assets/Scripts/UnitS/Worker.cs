using Unity.Netcode;
using UnityEngine;

public class Worker : NetworkBehaviour
{
    public Construction construction;
    public Unit unit;
    private bool isBuilding = false;
    private UnitMovement unitMovement;
    private Laser laser;

    [ServerRpc(RequireOwnership = false)]
    private void ActivateLaserServerRpc()
    {
        ActivateLaserClientRpc();
    }

    [ClientRpc]
    private void ActivateLaserClientRpc()
    {
        laser.isAttacking = false;
        laser.SetTarget(construction.GetComponent<Damagable>());
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

    public void StopConstruction(bool removeFromList = true)
    {
        if (construction == null) return;
        if (removeFromList) construction.RemoveWorker(unit);
        isBuilding = false;
        construction = null;
        DeactivateLaserServerRpc();
    }

    public void MoveToConstruction(Construction construction)
    {
        this.construction = construction;

        if (unitMovement != null && DistanceToConstruction() > unit.unitSo.buildingDistance)
        {
            unitMovement.MoveToServerRpc(construction.transform.position);
        }
    }

    void Awake()
    {
        unit = GetComponent<Unit>();
        unitMovement = GetComponent<UnitMovement>();
        laser = GetComponent<Laser>();
    }

    public override void OnDestroy()
    {
        StopConstruction();
    }

    void Update()
    {
        if (!IsServer) return;

        if (construction == null) return;
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
