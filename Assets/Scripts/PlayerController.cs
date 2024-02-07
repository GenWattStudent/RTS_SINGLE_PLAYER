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
    public static PlayerController Instance;
    public PlayerLevelSo playerLevelSo;
    public NetworkVariable<int> playerExpierence = new(0);

    public event Action<int, int, int, int> OnPlayerLevelChange;
    public static event Action<Unit, List<Unit>> OnUnitChange;
    public static event Action<Building, List<Building>> OnBuildingChange;

    private void SpawnHero(ulong clientId, PlayerData playerData)
    {
        var heroInstance = Instantiate(hero, playerData.spawnPosition, Quaternion.identity);
        var unitMovement = heroInstance.GetComponent<UnitMovement>();
        var no = heroInstance.GetComponent<NetworkObject>();

        if (unitMovement != null) unitMovement.isReachedDestinationAfterSpawn = true;

        playerData.spawnPosition += new Vector3(2, 0, 0);

        no.SpawnWithOwnership(clientId);
    }

    public void AddUnit(Unit unit, ulong clientId)
    {
        PlayerData playerData = MultiplayerController.Instance.Get(OwnerClientId);
        var damagableScript = unit.GetComponent<Damagable>();
        playerData.units.Add(unit);
        damagableScript.OnDead += () =>
        {
            RemoveUnit(unit, clientId);
        };

        OnUnitChange?.Invoke(unit, playerData.units);
    }

    public void AddExpiernce(int amount)
    {
        PlayerData playerData = MultiplayerController.Instance.Get(OwnerClientId);
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

    public void RemoveUnit(Unit unit, ulong clientId)
    {
        PlayerData playerData = MultiplayerController.Instance.Get(clientId);
        playerData.units.Remove(unit);
        OnUnitChange?.Invoke(unit, playerData.units);
    }

    public void AddBuilding(Building building, ulong clientId)
    {
        PlayerData playerData = MultiplayerController.Instance.Get(clientId);
        var damagableScript = building.GetComponent<Damagable>();
        playerData.buildings.Add(building);

        damagableScript.OnDead += () =>
        {
            RemoveBuilding(building, clientId);
        };

        OnBuildingChange?.Invoke(building, playerData.buildings);
    }

    public void RemoveBuilding(Building building, ulong clientId)
    {
        PlayerData playerData = MultiplayerController.Instance.Get(clientId);
        playerData.buildings.Remove(building);
        OnBuildingChange?.Invoke(building, playerData.buildings);
    }

    public bool IsMaxBuildingOfType(BuildingSo buildingSo, ulong clientId)
    {
        int count = GetBuildingCountOfType(buildingSo, clientId);
        return count >= buildingSo.maxBuildingCount;
    }

    public int GetBuildingCountOfType(BuildingSo buildingSo, ulong clientId)
    {
        PlayerData playerData = MultiplayerController.Instance.Get(clientId);
        int count = 0;

        foreach (var building in playerData.buildings)
        {
            if (building.buildingSo.buildingName == buildingSo.buildingName) count++;
        }

        return count;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnUnitServerRpc(ulong clientId)
    {
        PlayerData playerData = MultiplayerController.Instance.Get(clientId);
        SpawnHero(clientId, playerData);
        Debug.Log("SpawnUnitServerRpc server " + clientId);
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
                no.SpawnWithOwnership(clientId);
            }
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

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerUiServerRpc(ulong clientId)
    {
        var managers = Instantiate(managersPrefab, Vector3.zero, Quaternion.identity);
        var noManagers = managers.GetComponent<NetworkObject>();
        noManagers.SpawnWithOwnership(clientId);

        var playerUi = Instantiate(toolbarPrefab, Vector3.zero, Quaternion.identity);
        var no = playerUi.GetComponent<NetworkObject>();
        no.SpawnWithOwnership(clientId);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log("SpawnUnitServerRpc server");
            SpawnUnitServerRpc(clientId);
            SpawnPlayerUiServerRpc(clientId);
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log("OnClientDisconnect server");
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {

    }
}
