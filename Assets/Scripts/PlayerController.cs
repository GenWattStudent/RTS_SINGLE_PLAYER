using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private GameObject hero;
    [SerializeField] private List<GameObject> unitPrefabs = new();
    [SerializeField] private GameObject toolbarPrefab;
    [SerializeField] private GameObject managersPrefab;
    public PlayerLevelSo playerLevelSo;
    public PlayerData playerData;
    public NetworkVariable<int> playerExpierence = new(0);
    public GameObject toolbar;

    public event Action<int, int, int, int> OnPlayerLevelChange;
    public static event Action<Unit, List<Unit>> OnUnitChange;
    public static event Action<Building, List<Building>> OnBuildingChange;

    private void SpawnHero(ulong clientId)
    {
        var heroInstance = Instantiate(hero, playerData.spawnPosition, Quaternion.identity);
        var unitMovement = heroInstance.GetComponent<UnitMovement>();
        var no = heroInstance.GetComponent<NetworkObject>();

        if (unitMovement != null) unitMovement.isReachedDestinationAfterSpawn = true;

        playerData.spawnPosition += new Vector3(2, 0, 0);

        no.SpawnWithOwnership(clientId);
        SpawnHeroClientRpc(no, clientId);
    }

    [ClientRpc]
    private void SpawnHeroClientRpc(NetworkObjectReference no, ulong clientId)
    {
        if (no.TryGet(out NetworkObject unit))
        {
            var unitScript = unit.GetComponent<Unit>();
            if (clientId == OwnerClientId) AddUnit(unitScript);
        }
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

        playerExpierence.Value += amount;
        var nextLevelData = playerLevelSo.levelsData[playerData.playerLevel];
        var diffrence = playerExpierence.Value - nextLevelData.expToNextLevel;

        if (playerData.playerLevel < playerLevelSo.levelsData.Count && playerData.playerExpierence >= nextLevelData.expToNextLevel)
        {
            playerData.playerLevel++;
            playerExpierence.Value = diffrence;
            SkillTreeManager.Instance.AddSkillPoints(1);
        }

        OnPlayerLevelChange?.Invoke(nextLevelData.expToNextLevel, playerExpierence.Value, playerData.playerLevel, playerLevelSo.levelsData.Count);
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
    private void SpawnUnitServerRpc(ServerRpcParams rpcParams = default)
    {
        SpawnHero(rpcParams.Receive.SenderClientId);
        Debug.Log("SpawnUnitServerRpc server " + rpcParams.Receive.SenderClientId);
        foreach (var unitPrefab in unitPrefabs)
        {
            for (int i = 0; i < 2; i++)
            {
                var unit = Instantiate(unitPrefab, playerData.spawnPosition, Quaternion.identity);
                var unitMovement = unit.GetComponent<UnitMovement>();
                var no = unit.GetComponent<NetworkObject>();
                unitMovement.agent.enabled = true;

                if (unitMovement != null) unitMovement.isReachedDestinationAfterSpawn = true;

                playerData.spawnPosition += new Vector3(2, 0, 0);
                no.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
                SpawnUnitClientRpc(no, rpcParams.Receive.SenderClientId);
            }
        }

        playerData.spawnPosition = new Vector3(1.5f, 0, 7.5f);
    }

    [ClientRpc]
    private void SpawnUnitClientRpc(NetworkObjectReference no, ulong clientId)
    {
        if (no.TryGet(out NetworkObject unit))
        {
            var unitScript = unit.GetComponent<Unit>();

            if (clientId == OwnerClientId) AddUnit(unitScript);
        }
    }

    // public PlayerController GetPlayerControllerWithClientId(ulong clientId)
    // {
    //     var playerControllers = FindObjectsOfType<PlayerController>();

    //     foreach (var playerController in playerControllers)
    //     {
    //         if (playerController.OwnerClientId == clientId) return playerController;
    //     }

    //     return null;
    // }

    private void Awake()
    {
        playerData = new PlayerData
        {
            playerColor = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId].playerColor,
            playerMaterial = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId].playerMaterial
        };

        var playerLevelUI = GetComponent<PlayerLevelUI>();
        toolbar = GetComponentInChildren<UIBuildingManager>().gameObject;
        playerLevelUI.gameResult = FindObjectOfType<GameResult>();
    }

    private void Start()
    {
        if (IsOwner)
        {
            Debug.Log("SpawnUnitServerRpc client");
            SpawnUnitServerRpc();
        }

        if (IsServer)
        {
            AddExpiernce(0);
        }
    }
}
