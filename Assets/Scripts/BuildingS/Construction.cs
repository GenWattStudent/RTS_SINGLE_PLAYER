using System.Collections.Generic;
using UnityEngine;

public class Construction : MonoBehaviour
{
    public BuildingSo buildingSo;
    private float constructionTimer = 0f;
    private bool isCurrentlyConstructing = false;
    private float buildingSpeed  = 0f;
    public List<Unit> buildingUnits = new ();
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private GameObject buildingInProgressPrefab;
    [SerializeField] private GameObject constructionPrefab;
    private ProgresBar progresBar;
    private Damagable damagable;

    public void AddWorker(Unit unit)
    {
        if (unit.unitSo.type != UnitSo.UnitType.Worker) return;
        buildingUnits.Add(unit);
        buildingSpeed += unit.unitSo.attackDamage;
        Debug.Log("Add worker - building " + buildingUnits.Count + " - " + buildingSpeed);
        StartConstruction();
    }

    public void RemoveWorker(Unit unit)
    {
        if (unit.unitSo.type != UnitSo.UnitType.Worker) return;
        buildingUnits.Remove(unit);
        if (buildingSpeed > 0) buildingSpeed -= unit.unitSo.attackDamage;
        if (buildingUnits.Count == 0)
        {
            StopConstruction();
        }
    }

    public void StartConstruction()
    {
        isCurrentlyConstructing = true;
        Debug.Log("Start construction - building");
        ActivateBuildInProgress();
    }

    public void StopConstruction()
    {
        isCurrentlyConstructing = false;
        ActivateConstructionBuilding();
        StopWorkersConstruction();
    }

    private void StopWorkersConstruction()
    {
        foreach (var unit in buildingUnits)
        {
            var worker = unit.GetComponent<Worker>();
            worker.StopConstruction(false);
        }
    }

    private bool RemoveConstructionIfSelected()
    {
        if (SelectionManager.selectedObjects.Count == 1)
        {
            var selectedConstruction = SelectionManager.selectedObjects[0].GetComponent<Construction>();

            if (selectedConstruction == this) {
                var selectable = SelectionManager.selectedObjects[0].GetComponent<Selectable>();
                SelectionManager.Deselect(selectable);
                return true;
            }

            return false;
        }

        return false;
    }

    private void InstantiateBuilding()
    {
        // Finished building
        StopWorkersConstruction();
        var building = Instantiate(buildingSo.prefab, transform.position, Quaternion.identity);
        var isRemoved = RemoveConstructionIfSelected();
        var selectable = building.GetComponent<Selectable>();

        building.GetComponent<Unit>().playerId = GetComponent<Unit>().playerId;
        building.GetComponent<Damagable>().playerId = GetComponent<Unit>().playerId;
        if (isRemoved) SelectionManager.SelectBuilding(selectable);
        Destroy(gameObject);
    }

    private void ActivateBuildInProgress()
    {
        // Building with shader effect while someone is building it
        buildingInProgressPrefab.SetActive(true);
        constructionPrefab.SetActive(false);
    }

    private void ActivateConstructionBuilding()
    {
        // Building with shader effect while someone is building it
        buildingInProgressPrefab.SetActive(false);
        constructionPrefab.SetActive(true);
    }

    private void OnDestroy()
    {
        StopWorkersConstruction();
    }

    void Start()
    {
        progresBar = healthBar.GetComponent<ProgresBar>();
        damagable = GetComponent<Damagable>();
        progresBar.UpdateProgresBar(damagable.health, buildingSo.health);
    }

    void Update()
    {
        if (isCurrentlyConstructing)
        {
            constructionTimer += buildingSpeed * Time.deltaTime;
            damagable.health = Mathf.Floor(constructionTimer);
            progresBar.UpdateProgresBar(constructionTimer, buildingSo.health);

            if (constructionTimer >= buildingSo.health)
            {
                isCurrentlyConstructing = false;
                constructionTimer = 0;
                InstantiateBuilding();
            }
        }
    }
}
