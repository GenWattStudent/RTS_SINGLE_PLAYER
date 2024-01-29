using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TankBuilding : MonoBehaviour, ISpawnerBuilding
{
    [SerializeField] private UnitSo unitToSpawn;
    [SerializeField] private Transform unitSpawnPoint;
    public Transform unitMovePoint;
    private List<UnitSo> unitsQueue = new();
    private float spawnTimer;
    private bool isUnitSpawning = false;
    private UnitSo currentSpawningUnit;
    private Building buildingScript;
    public float totalSpawnTime { get; set; } = 0;

    private ResourceUsage resourceUsage;
    public event Action<UnitSo, Unit> OnSpawnUnit;

    private void Awake()
    {
        buildingScript = GetComponent<Building>();
        resourceUsage = GetComponent<ResourceUsage>();
    }

    private Unit InstantiateUnit()
    {
        if (!UnitCountManager.Instance.CanSpawnUnit())
        {
            InfoBox.Instance.AddError("You have reached unit limit");
            return null;
        };

        UIStorage.Instance.DecreaseResource(unitToSpawn.costResource, unitToSpawn.cost);

        var agent = unitToSpawn.prefab.GetComponent<NavMeshAgent>();
        agent.enabled = false;

        GameObject unitInstance = Instantiate(unitToSpawn.prefab, unitSpawnPoint.position, unitSpawnPoint.rotation);
        var unitScript = unitInstance.GetComponent<Unit>();
        var damagableScript = unitInstance.GetComponent<Damagable>();

        if (damagableScript != null) unitInstance.GetComponent<Damagable>().playerId = PlayerController.playerId;
        unitScript.playerId = PlayerController.playerId;
        unitScript.ChangeMaterial(PlayerController.playerMaterial, true);

        if (unitMovePoint != null)
        {
            var unitMovement = unitInstance.GetComponent<UnitMovement>();
            if (unitMovement != null) unitMovement.SetDestinationAfterSpawn(unitMovePoint.position);
        }

        PlayerController.AddUnit(unitScript);
        return unitScript;
    }

    public void AddUnitToQueue(UnitSo unit)
    {
        unitsQueue.Add(unit);
        StartQueue();
    }

    private void StartQueue()
    {
        if (unitsQueue.Count > 0 && !isUnitSpawning)
        {
            spawnTimer = unitsQueue[0].spawnTime - buildingScript.buildingLevelable.reduceSpawnTime;
            totalSpawnTime = spawnTimer;
            currentSpawningUnit = unitsQueue[0];
            isUnitSpawning = true;
        }
    }

    private void SpawnUnit()
    {
        if (unitsQueue.Count > 0 && spawnTimer < 0)
        {
            unitToSpawn = unitsQueue[0];
            var unit = InstantiateUnit();
            OnSpawnUnit?.Invoke(unitToSpawn, unit);
            unitsQueue.RemoveAt(0);
            isUnitSpawning = false;
            currentSpawningUnit = null;
            StartQueue();
        }
    }

    private void UpdateScreen()
    {
        var screenController = GetComponentInChildren<ScreenController>();

        if (screenController == null) return;

        if (currentSpawningUnit != null)
        {
            screenController.SetProgresBar(spawnTimer, totalSpawnTime);
        }
        else
        {
            screenController.SetProgresBar(0, 0);
        }
    }

    private void Update()
    {
        if (currentSpawningUnit == null
            || !UIStorage.Instance.HasEnoughResource(currentSpawningUnit.costResource, currentSpawningUnit.cost)
            || resourceUsage.isInDebt) return;

        spawnTimer -= Time.deltaTime;
        UpdateScreen();
        SpawnUnit();
    }

    public float GetSpawnTimer()
    {
        return spawnTimer < 0 ? 0 : spawnTimer;
    }

    public UnitSo GetCurrentSpawningUnit()
    {
        return currentSpawningUnit;
    }

    public int GetUnitQueueCountByName(string unitName)
    {
        return unitsQueue.FindAll(unit => unit.unitName == unitName).Count;
    }

    private void OnDestroy()
    {
        if (UIUnitManager.Instance.currentBuilding != this) return;
        UIUnitManager.Instance.ClearTabs();
    }
}
