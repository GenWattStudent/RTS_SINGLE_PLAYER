using System;
using System.Collections.Generic;
using FOVMapping;
using Unity.Netcode;
using UnityEngine;

// i want this script to execute first
[DefaultExecutionOrder(-100)]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private GameObject hero;
    [SerializeField] private List<GameObject> unitPrefabs = new();
    public PlayerLevelSo playerLevelSo;
    public PlayerData playerData;
    public NetworkVariable<int> playerExpierence = new(0);
    public NetworkVariable<int> playerLevel = new(1);
    private FOVManager fovManager;
    private RTSObjectsManager RTSObjectsManager;

    public event Action<int, int, int, int> OnPlayerLevelChange;
    public static event Action<Unit, List<Unit>> OnUnitChange;
    public static event Action<Building, List<Building>> OnBuildingChange;

    private void SpawnHero(ulong clientId, Vector3 spawnPosition)
    {
        var heroInstance = Instantiate(hero, spawnPosition, Quaternion.identity);
        var unitMovement = heroInstance.GetComponent<UnitMovement>();
        var no = heroInstance.GetComponent<NetworkObject>();

        if (unitMovement != null) unitMovement.isReachedDestinationAfterSpawn = true;

        no.SpawnWithOwnership(clientId);
        RTSObjectsManager.AddUnitServerRpc(no);
    }

    // [ClientRpc]
    // private void SpawnHeroClientRpc(NetworkObjectReference no, ulong clientId)
    // {
    //     if (no.TryGet(out NetworkObject unit))
    //     {
    //         var unitScript = unit.GetComponent<Unit>();
    //         if (clientId == OwnerClientId) RTSObjectsManager.AddUnitServerRpc(unitScript);
    //     }
    // }

    public void AddUnit(Unit unit)
    {
        var damagableScript = unit.GetComponent<Damagable>();
        playerData.units.Add(unit);

        if (unit.TryGetComponent(out FOVAgent fovAgent)) fovManager.AddFOVAgent(fovAgent);

        damagableScript.OnDead += () =>
        {
            RemoveUnit(unit);
        };

        OnUnitChange?.Invoke(unit, playerData.units);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddExpiernceServerRpc(int amount)
    {
        if (playerLevel.Value == playerLevelSo.levelsData.Count) return;

        var playerExp = playerExpierence.Value;
        playerExp += amount;
        Debug.Log("AddExpiernceServerRpc " + playerExp + " " + playerLevel.Value + " " + amount);
        var nextLevelData = playerLevelSo.levelsData[playerLevel.Value];
        var diffrence = playerExp - nextLevelData.expToNextLevel;

        if (playerLevel.Value < playerLevelSo.levelsData.Count && playerExp >= nextLevelData.expToNextLevel)
        {
            playerLevel.Value++;
            playerExp = diffrence;
            var playerSkillTree = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponentInChildren<SkillTreeManager>();
            Debug.Log("AddExpiernceServerRpc " + playerSkillTree.OwnerClientId);
            playerSkillTree.AddSkillPointsServerRpc(1);
        }
        Debug.Log("AddExpiernce " + playerExp);
        playerExpierence.Value = playerExp;
    }

    public void RemoveUnit(Unit unit)
    {
        if (unit.TryGetComponent(out FOVAgent fovAgent)) fovManager.RemoveFOVAgent(fovAgent);
        playerData.units.Remove(unit);
        OnUnitChange?.Invoke(unit, playerData.units);
    }

    public void AddBuilding(Building building)
    {
        var damagableScript = building.GetComponent<Damagable>();
        playerData.buildings.Add(building);

        if (building.TryGetComponent(out FOVAgent fovAgent)) fovManager.AddFOVAgent(fovAgent);

        damagableScript.OnDead += () =>
        {
            RemoveBuilding(building);
        };

        OnBuildingChange?.Invoke(building, playerData.buildings);
    }

    public void RemoveBuilding(Building building)
    {
        if (building.TryGetComponent(out FOVAgent fovAgent)) fovManager.RemoveFOVAgent(fovAgent);
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
    private void SpawnUnitServerRpc(Vector3 spawnPosition, ServerRpcParams rpcParams = default)
    {
        SpawnHero(rpcParams.Receive.SenderClientId, spawnPosition);
        spawnPosition += new Vector3(2, 0, 0);

        foreach (var unitPrefab in unitPrefabs)
        {
            for (int i = 0; i < 2; i++)
            {
                var unit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
                var unitMovement = unit.GetComponent<UnitMovement>();
                var no = unit.GetComponent<NetworkObject>();
                unitMovement.agent.enabled = true;

                if (unitMovement != null) unitMovement.isReachedDestinationAfterSpawn = true;

                spawnPosition += new Vector3(2, 0, 0);
                no.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
                RTSObjectsManager.AddUnitServerRpc(no);
            }
        }
    }

    // [ClientRpc]
    // private void SpawnUnitClientRpc(NetworkObjectReference no, ulong clientId)
    // {
    //     if (no.TryGet(out NetworkObject unit))
    //     {
    //         var unitScript = unit.GetComponent<Unit>();

    //         if (clientId == OwnerClientId) RTSObjectsManager.AddUnitServerRpc(unitScript);
    //     }
    // }

    private void OnPlayerLevelChangeHandler(int prev, int current)
    {
        if (!IsOwner) return;

        var expToNextLevel = -1;

        if (current < playerLevelSo.levelsData.Count)
        {
            expToNextLevel = playerLevelSo.levelsData[current].expToNextLevel;
        }

        OnPlayerLevelChange?.Invoke(expToNextLevel, playerExpierence.Value, current, playerLevelSo.levelsData.Count);
    }

    private void OnPlayerExpierenceChangeHandler(int prev, int current)
    {
        if (!IsOwner) return;

        var expToNextLevel = -1;

        if (playerLevel.Value < playerLevelSo.levelsData.Count)
        {
            expToNextLevel = playerLevelSo.levelsData[playerLevel.Value].expToNextLevel;
        }

        OnPlayerLevelChange?.Invoke(playerLevelSo.levelsData[playerLevel.Value].expToNextLevel, current, playerLevel.Value, playerLevelSo.levelsData.Count);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerLevel.OnValueChanged += OnPlayerLevelChangeHandler;
        playerExpierence.OnValueChanged += OnPlayerExpierenceChangeHandler;
    }

    private void Awake()
    {
        playerData = new PlayerData();
        fovManager = FindFirstObjectByType<FOVManager>();
        RTSObjectsManager = GetComponent<RTSObjectsManager>();
    }

    private void Start()
    {
        playerData.playerColor = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId].playerColor;
        playerData.playerMaterial = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId].playerMaterial;
        playerData.spawnPosition = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId].spawnPosition.position;

        if (IsOwner)
        {
            SpawnUnitServerRpc(playerData.spawnPosition);
        }

        if (IsServer)
        {
            AddExpiernceServerRpc(1);
            playerData.teamId = NetworkManager.Singleton.ConnectedClients.Count;
        }
    }
}
