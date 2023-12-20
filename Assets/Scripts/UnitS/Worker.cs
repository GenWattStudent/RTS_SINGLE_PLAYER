using UnityEngine;

public class Worker : MonoBehaviour
{
    public Construction construction;
    public Unit unit;
    private bool isBuilding = false;
    private UnitMovement unitMovement;

    private float DistanceToConstruction() {
        return Vector3.Distance(transform.position, construction.transform.position);
    }

    private void StartConstruction() {
        construction.AddWorker(unit);
        isBuilding = true;
    }

    public void StopConstruction() {
        if (construction == null) return;
        construction.RemoveWorker(unit);
        isBuilding = false;
        construction = null;
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
    }

    void Update()
    {
        if (construction == null) return;
        var distance = DistanceToConstruction();

        if (distance <= unit.unitSo.buildingDistance && !isBuilding) {
            if (unitMovement != null) {
                unitMovement.Stop();
            }
            StartConstruction();
        }   

        if (isBuilding && distance > unit.unitSo.buildingDistance) {
            StopConstruction();
        }
    }
}
