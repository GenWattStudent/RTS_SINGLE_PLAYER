using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private GameObject hero;
    [SerializeField] private List<GameObject> unitPrefabs = new();
    public PlayerLevelSo playerLevelSo;
    public PlayerData playerData;

    public NetworkVariable<int> playerExpierence = new(0);
    public NetworkVariable<int> playerLevel = new(1);
    public NetworkVariable<TeamType> teamType = new(TeamType.None);
    public float currentPing;

    private RTSObjectsManager RTSObjectsManager;
    private double lastPingTime;
    private float checkPingTime = 1f;
    private float checkPingTimer;

    public event Action<int, int, int, int> OnPlayerLevelChange;

    public void GetPing()
    {
        if (IsClient)
        {
            lastPingTime = NetworkManager.Singleton.LocalTime.Time;
            RequestPingServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPingServerRpc(ServerRpcParams serverRpcParams = default)
    {
        RespondPingClientRpc(serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void RespondPingClientRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Calculate the round-trip time (ping)
            double currentTime = NetworkManager.Singleton.LocalTime.Time;
            currentPing = (float)((currentTime - lastPingTime) * 1000); // Convert to milliseconds
        }
    }

    private void SpawnHero(ulong clientId, Vector3 spawnPosition)
    {
        var heroInstance = Instantiate(hero, spawnPosition, Quaternion.identity);
        var unitMovement = heroInstance.GetComponent<UnitMovement>();
        var no = heroInstance.GetComponent<NetworkObject>();
        var damagable = heroInstance.GetComponent<Damagable>();

        if (unitMovement != null) unitMovement.isReachedDestinationAfterSpawn = true;

        no.SpawnWithOwnership(clientId);
        damagable.teamType.Value = teamType.Value;
        RTSObjectsManager.AddUnitServerRpc(no);
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
        SpawnHero(OwnerClientId, spawnPosition);
        spawnPosition += new Vector3(2, 0, 0);

        foreach (var unitPrefab in unitPrefabs)
        {
            for (int i = 0; i < 2; i++)
            {
                var unit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
                var unitMovement = unit.GetComponent<UnitMovement>();
                var no = unit.GetComponent<NetworkObject>();
                var damagable = unit.GetComponent<Damagable>();

                unitMovement.agent.enabled = true;

                if (unitMovement != null) unitMovement.isReachedDestinationAfterSpawn = true;

                spawnPosition += new Vector3(2, 0, 0);

                no.SpawnWithOwnership(OwnerClientId);
                damagable.teamType.Value = teamType.Value;
                RTSObjectsManager.AddUnitServerRpc(no);
            }
        }
    }

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
        RTSObjectsManager = GetComponent<RTSObjectsManager>();
    }

    [ServerRpc()]
    private void SetTeamServerRpc(TeamType teamType)
    {
        this.teamType.Value = teamType;
    }

    private void Start()
    {
        playerData.playerColor = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId].playerColor;
        playerData.playerMaterial = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId].playerMaterial;
        playerData.spawnPosition = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId].spawnPosition.position;

        if (IsOwner)
        {
            if (GameManager.Instance.IsDebug)
            {
                var team = OwnerClientId % 2 == 0 ? TeamType.Blue : TeamType.Red;
                SetTeamServerRpc(team);
            }
            else
            {
                var team = LobbyManager.Instance.playerLobbyData.Team;
                SetTeamServerRpc(team);
            }

            SpawnUnitServerRpc(playerData.spawnPosition);
        }

        if (IsServer)
        {
            AddExpiernceServerRpc(1);
            playerData.teamId = NetworkManager.Singleton.ConnectedClients.Count;
        }
    }

    private void Update()
    {
        if (IsClient)
        {
            checkPingTimer += Time.deltaTime;

            if (checkPingTimer >= checkPingTime)
            {
                GetPing();
                checkPingTimer = 0;
            }
        }
    }
}
