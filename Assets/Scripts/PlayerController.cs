using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private GameObject hero;
    [SerializeField] private List<GameObject> unitPrefabs = new();
    [SerializeField] private GameObject toolbarPrefab;
    public static PlayerController Instance;
    public PlayerLevelSo playerLevelSo;
    public PlayerData playerData;

    public event Action<int, int, int, int> OnPlayerLevelChange;
    public static event Action<Unit, List<Unit>> OnUnitChange;
    public static event Action<Building, List<Building>> OnBuildingChange;

    private void SpawnHero()
    {
        var heroInstance = Instantiate(hero, playerData.spawnPosition, Quaternion.identity);
        var damagableScript = heroInstance.GetComponent<Damagable>();
        var unitScript = heroInstance.GetComponent<Unit>();
        var unitMovement = heroInstance.GetComponent<UnitMovement>();

        if (unitMovement != null) unitMovement.isReachedDestinationAfterSpawn = true;

        damagableScript.OwnerClientId = OwnerClientId;
        unitScript.OwnerClientId = OwnerClientId;
        unitScript.ChangeMaterial(playerData.playerMaterial, true);
        playerData.units.Add(unitScript);

        playerData.spawnPosition += new Vector3(2, 0, 0);
    }

    public void AddUnit(Unit unit)
    {
        var damagableScript = unit.GetComponent<Damagable>();
        playerData.units.Add(unit);
        damagableScript.OnDead += () =>
        {
            RemoveUnit(unit);
        };

        OnUnitChange?.Invoke(unit, playerData.units);
    }

    public void AddExpiernce(int amount)
    {
        if (playerData.playerLevel == playerLevelSo.levelsData.Count) return;

        playerData.playerExpierence += amount;
        var nextLevelData = playerLevelSo.levelsData[playerData.playerLevel];
        var diffrence = playerData.playerExpierence - nextLevelData.expToNextLevel;

        if (playerData.playerLevel < playerLevelSo.levelsData.Count && playerData.playerExpierence >= nextLevelData.expToNextLevel)
        {
            playerData.playerLevel++;
            playerData.playerExpierence = diffrence;
            SkillTreeManager.Instance.AddSkillPoints(1);
        }

        OnPlayerLevelChange?.Invoke(nextLevelData.expToNextLevel, playerData.playerExpierence, playerData.playerLevel, playerLevelSo.levelsData.Count);
    }

    public void RemoveUnit(Unit unit)
    {
        playerData.units.Remove(unit);
        OnUnitChange?.Invoke(unit, playerData.units);
    }

    public void AddBuilding(Building building)
    {
        var damagableScript = building.GetComponent<Damagable>();
        playerData.buildings.Add(building);

        damagableScript.OnDead += () =>
        {
            RemoveBuilding(building);
        };

        OnBuildingChange?.Invoke(building, playerData.buildings);
    }

    public void RemoveBuilding(Building building)
    {
        playerData.buildings.Remove(building);
        OnBuildingChange?.Invoke(building, playerData.buildings);
    }

    public bool IsMaxBuildingOfType(BuildingSo buildingSo)
    {
        int count = GetBuildingCountOfType(buildingSo);
        return count >= buildingSo.maxBuildingCount;
    }

    public int GetBuildingCountOfType(BuildingSo buildingSo)
    {
        int count = 0;

        foreach (var building in playerData.buildings)
        {
            if (building.buildingSo.buildingName == buildingSo.buildingName) count++;
        }

        return count;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnUnitsServerRpc()
    {

        // SpawnHero();
        foreach (var unitPrefab in unitPrefabs)
        {
            for (int i = 0; i < 2; i++)
            {
                var unit = Instantiate(unitPrefab, playerData.spawnPosition, Quaternion.identity);
                var damagableScript = unit.GetComponent<Damagable>();
                var unitScript = unit.GetComponent<Unit>();
                var unitMovement = unit.GetComponent<UnitMovement>();
                var no = unit.GetComponent<NetworkObject>();
                unitMovement.agent.enabled = true;

                if (unitMovement != null) unitMovement.isReachedDestinationAfterSpawn = true;

                damagableScript.OwnerClientId = OwnerClientId;
                unitScript.OwnerClientId = OwnerClientId;
                unitScript.ChangeMaterial(playerData.playerMaterial, true);
                AddUnit(unitScript);

                playerData.spawnPosition += new Vector3(2, 0, 0);
                no.SpawnWithOwnership(OwnerClientId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnToolbarServerRpc()
    {
        var toolbar = Instantiate(toolbarPrefab, playerData.spawnPosition, Quaternion.identity);
        var no = toolbar.GetComponent<NetworkObject>();
        no.SpawnWithOwnership(OwnerClientId);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerData = MultiplayerController.Instance.Get(OwnerClientId);
        SpawnToolbarServerRpc();
        SpawnUnitsServerRpc();
        AddExpiernce(0);
    }
}
