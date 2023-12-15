using System.Collections.Generic;
using UnityEngine;

public class Construction : MonoBehaviour
{
    public BuildingSo buildingSo;
    private float constructionTimer = 0f;
    private bool isCurrentlyConstructing = false;
    private float buildingSpeed  = 0f;
    public List<Unit> buildingUnits = new List<Unit>();
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private GameObject buildingInProgressPrefab;
    [SerializeField] private GameObject constructionPrefab;
    private ProgresBar progresBar;

    public void AddWorker(Unit unit)
    {
        if (unit.unitSo.type != UnitSo.UnitType.Worker) return;
        buildingUnits.Add(unit);
        buildingSpeed += unit.unitSo.attackDamage;
        StartConstruction();
    }

    public void RemoveWorker(Unit unit)
    {
        if (unit.unitSo.type != UnitSo.UnitType.Worker) return;
        buildingUnits.Remove(unit);
        buildingSpeed -= unit.unitSo.attackDamage;
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
        foreach (var unit in buildingUnits)
        {
            var worker = unit.GetComponent<Worker>();
            if (worker != null)
            {
                worker.StopConstruction();
            }
        }
    }

    private void InstantiateBuilding()
    {
        // Finished building
        var building = Instantiate(buildingSo.prefab, transform.position, Quaternion.identity);
        building.GetComponent<Unit>().playerId = GetComponent<Unit>().playerId;
        building.GetComponent<Damagable>().playerId = GetComponent<Unit>().playerId;
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

    void Start()
    {
        progresBar = healthBar.GetComponent<ProgresBar>();
        progresBar.UpdateProgresBar(0, buildingSo.health);
    }

    void Update()
    {
        if (isCurrentlyConstructing)
        {
            constructionTimer += buildingSpeed * Time.deltaTime;
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
