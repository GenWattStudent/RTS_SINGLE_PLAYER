using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    public Guid playerId;
    public Color playerColor;
    public Material playerMaterial;
    public List<Unit> units = new ();
    public List<Building> buildings = new ();
    [SerializeField] private GameObject hero;
    [SerializeField] private List<GameObject> unitPrefabs = new ();
    public Vector3 spawnPosition = new Vector3(1.5f, 0, 2f);

    // add unit event
    public event Action<Unit, List<Unit>> OnUnitChange;
    public event Action<Building, List<Building>> OnBuildingChange;

    private void SpawnHero() {
        var heroInstance = Instantiate(hero, spawnPosition, Quaternion.identity);
        var damagableScript = heroInstance.GetComponent<Damagable>();
        var unitScript = heroInstance.GetComponent<Unit>();
        var unitMovement = heroInstance.GetComponent<UnitMovement>();

        if (unitMovement != null) unitMovement.isReachedDestinationAfterSpawn = true;

        damagableScript.playerId = playerId;
        unitScript.playerId = playerId;
        unitScript.ChangeMaterial(playerMaterial);
        units.Add(unitScript);

        spawnPosition += new Vector3(2, 0 ,0);
    }

    public void AddUnit(Unit unit) {
        var damagableScript = unit.GetComponent<Damagable>();
        units.Add(unit);
        damagableScript.OnDead += () => {
            RemoveUnit(unit);
        };

        OnUnitChange?.Invoke(unit, units);
    }

    public void RemoveUnit(Unit unit) {
        units.Remove(unit);
        OnUnitChange?.Invoke(unit, units);
    }

    public void AddBuilding(Building building) {
        var damagableScript = building.GetComponent<Damagable>();
        buildings.Add(building);

        damagableScript.OnDead += () => {
            RemoveBuilding(building);
        };

        OnBuildingChange?.Invoke(building, buildings);
    }

    public void RemoveBuilding(Building building) {
        buildings.Remove(building);
        OnBuildingChange?.Invoke(building, buildings);
    }

    public bool IsMaxBuildingOfType(BuildingSo buildingSo) {
        int count = GetBuildingCountOfType(buildingSo);
        return count >= buildingSo.maxBuildingCount;
    }

    public int GetBuildingCountOfType(BuildingSo buildingSo) {
        int count = 0;

        foreach (var building in buildings) {
            if (building.buildingSo.buildingName == buildingSo.buildingName) count++;
        }

        return count;
    }

    private void SpawnUnits() {
        SpawnHero();
        foreach (var unitPrefab in unitPrefabs) {
            for (int i = 0; i < 2; i++) {
                var unit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
                var damagableScript = unit.GetComponent<Damagable>();
                var unitScript = unit.GetComponent<Unit>();
                var unitMovement = unit.GetComponent<UnitMovement>();
                unitMovement.agent.enabled = true;

                if (unitMovement != null) unitMovement.isReachedDestinationAfterSpawn = true;

                damagableScript.playerId = playerId;
                unitScript.playerId = playerId;
                unitScript.ChangeMaterial(playerMaterial);
                AddUnit(unitScript);

                spawnPosition += new Vector3(2, 0 ,0);
            }
        }
    }

    void Start()
    {
        playerId = Guid.NewGuid();;
        SpawnUnits();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
