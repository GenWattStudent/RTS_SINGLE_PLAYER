using UnityEngine;

public class Worker : MonoBehaviour
{
    public Construction construction;
    public UnitSo unitSo;
    private bool isBuilding = false;
    private UnitMovement unitMovement;

    private float DistanceToConstruction() {
        return Vector3.Distance(transform.position, construction.transform.position);
    }

    private void StartConstruction() {
        construction.AddWorker(unitSo);
        isBuilding = true;
    }

    public void StopConstruction() {
        construction.RemoveWorker(unitSo);
        isBuilding = false;
        construction = null;
    }

    public void MoveToConstruction(Construction construction) {
        if (unitMovement != null) {
            this.construction = construction;
            unitMovement.MoveTo(construction.transform.position);
        }
    }

    void Awake()
    {
        unitSo = GetComponent<Unit>().unitSo;
        unitMovement = GetComponent<UnitMovement>();
    }

    void Update()
    {
        if (construction == null) return;

        if (DistanceToConstruction() <= unitSo.buildingDistance && !isBuilding) {
            if (unitMovement != null) {
                unitMovement.Stop();
            }
            StartConstruction();
        }   

        if (isBuilding && DistanceToConstruction() > unitSo.buildingDistance) {
            StopConstruction();
        }
    }
}
