using UnityEngine;

public class Worker : MonoBehaviour
{
    public Construction construction;
    public Unit unit;
    private bool isBuilding = false;
    private UnitMovement unitMovement;
    private Laser laser;

    private void ActivateLaser() {
        laser.isAttacking = false;
        laser.SetTarget(construction.GetComponent<Damagable>());
    }

    private void DeactivateLaser() {
        laser.SetTarget(null);
    }

    private float DistanceToConstruction() {
        return Vector3.Distance(transform.position, construction.transform.position);
    }

    private void StartConstruction() {
        construction.AddWorker(unit);
        isBuilding = true;
        ActivateLaser();
    }

    public void StopConstruction(bool removeFromList = true) {
        if (construction == null) return;
        if (removeFromList) construction.RemoveWorker(unit);
        isBuilding = false;
        construction = null;
        DeactivateLaser();
    }

    public void MoveToConstruction(Construction construction) {
        this.construction = construction;

        if (unitMovement != null && DistanceToConstruction() > unit.unitSo.buildingDistance) {
            unitMovement.MoveTo(construction.transform.position);
        }
    }

    void Awake()
    {
        unit = GetComponent<Unit>();
        unitMovement = GetComponent<UnitMovement>();
        laser = GetComponent<Laser>();
    }

    void Update()
    {
        if (construction == null) return;
        var distance = DistanceToConstruction();
        unitMovement.RotateToTarget(construction.transform.position);
        if (distance <= unit.unitSo.buildingDistance && !isBuilding) {
            if (unitMovement != null) {
                unitMovement.Stop();
            }

            StartConstruction();
        }   

        if (isBuilding && distance > unit.unitSo.buildingDistance) {
            MoveToConstruction(construction);
        }
    }
}
