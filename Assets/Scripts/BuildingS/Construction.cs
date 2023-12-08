using System.Collections.Generic;
using UnityEngine;

public class Construction : MonoBehaviour
{
    public BuildingSo buildingSo;
    private float constructionTimer = 0f;
    private bool isCurrentlyConstructing = false;
    private float buildingSpeed  = 0f;
    public List<UnitSo> buildingUnits = new List<UnitSo>();
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private GameObject buildingInProgressPrefab;
    [SerializeField] private GameObject constructionPrefab;
    private ProgresBar progresBar;

    public void AddWorker(UnitSo unitSo)
    {
        if (unitSo.type != UnitSo.UnitType.Worker) return;
        buildingUnits.Add(unitSo);
        buildingSpeed += unitSo.attackDamage;
        StartConstruction();
    }

    public void RemoveWorker(UnitSo unitSo)
    {
        if (unitSo.type != UnitSo.UnitType.Worker) return;
        buildingUnits.Remove(unitSo);
        buildingSpeed -= unitSo.attackDamage;
        if (buildingUnits.Count == 0)
        {
            StopConstruction();
        }
    }

    public void StartConstruction()
    {
        isCurrentlyConstructing = true;
        ActivateBuildInProgress();
    }

    public void StopConstruction()
    {
        isCurrentlyConstructing = false;
        ActivateConstructionBuilding();
    }

    private void InstantiateBuilding()
    {
        // Finished building
        var building = Instantiate(buildingSo.prefab, transform.position, Quaternion.identity);
        building.GetComponent<Unit>().playerId = GetComponent<Unit>().playerId;
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
