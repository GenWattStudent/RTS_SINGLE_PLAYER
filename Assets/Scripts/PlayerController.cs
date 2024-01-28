using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static Color playerColor;
    public static Material playerMaterial;
    public static List<Unit> units = new ();
    public static List<Building> buildings = new ();
    [SerializeField] private GameObject hero;
    [SerializeField] private List<GameObject> unitPrefabs = new ();
    public Vector3 spawnPosition = new Vector3(1.5f, 0, 2f);
    public static Guid playerId;

    // add unit event
    public static event Action<Unit, List<Unit>> OnUnitChange;
    public static event Action<Building, List<Building>> OnBuildingChange;
    public int playerLevel = 1;
    public int playerExpierence = 0;
    public static PlayerController Instance;
    public PlayerLevelSo playerLevelSo;
    public event Action<int, int, int, int> OnPlayerLevelChange;

    private void SpawnHero() {
        var heroInstance = Instantiate(hero, spawnPosition, Quaternion.identity);
        var damagableScript = heroInstance.GetComponent<Damagable>();
        var unitScript = heroInstance.GetComponent<Unit>();
        var unitMovement = heroInstance.GetComponent<UnitMovement>();

        if (unitMovement != null) unitMovement.isReachedDestinationAfterSpawn = true;

        damagableScript.playerId = playerId;
        unitScript.playerId = playerId;
        unitScript.ChangeMaterial(playerMaterial, true);
        units.Add(unitScript);

        spawnPosition += new Vector3(2, 0 ,0);
    }

    public static void AddUnit(Unit unit) {
        var damagableScript = unit.GetComponent<Damagable>();
        units.Add(unit);
        damagableScript.OnDead += () => {
            RemoveUnit(unit);
        };

        OnUnitChange?.Invoke(unit, units);
    }

    public void AddExpiernce(int amount) {
        if (playerLevel == playerLevelSo.levelsData.Count) return;

        playerExpierence += amount;
        var nextLevelData = playerLevelSo.levelsData[playerLevel]; 
        var diffrence = playerExpierence - nextLevelData.expToNextLevel;

        if (playerLevel < playerLevelSo.levelsData.Count && playerExpierence >= nextLevelData.expToNextLevel) {
            playerLevel++;
            playerExpierence = diffrence;
            SkillTreeManager.Instance.AddSkillPoints(1);
        }

        OnPlayerLevelChange?.Invoke(nextLevelData.expToNextLevel, playerExpierence, playerLevel, playerLevelSo.levelsData.Count);
    }

    public static void RemoveUnit(Unit unit) {
        units.Remove(unit);
        OnUnitChange?.Invoke(unit, units);
    }

    public static void AddBuilding(Building building) {
        var damagableScript = building.GetComponent<Damagable>();
        buildings.Add(building);

        damagableScript.OnDead += () => {
            RemoveBuilding(building);
        };

        OnBuildingChange?.Invoke(building, buildings);
    }

    public static void RemoveBuilding(Building building) {
        buildings.Remove(building);
        OnBuildingChange?.Invoke(building, buildings);
    }

    public static bool IsMaxBuildingOfType(BuildingSo buildingSo) {
        int count = GetBuildingCountOfType(buildingSo);
        return count >= buildingSo.maxBuildingCount;
    }

    public static int GetBuildingCountOfType(BuildingSo buildingSo) {
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
                unitScript.ChangeMaterial(playerMaterial, true);
                AddUnit(unitScript);

                spawnPosition += new Vector3(2, 0 ,0);
            }
        }
    }

    void Awake() {
        Instance = this;
    }

    void Start()
    {
        SpawnUnits();
        AddExpiernce(0);
    }

    void Update()
    {

    }
}
