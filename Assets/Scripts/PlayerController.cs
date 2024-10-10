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

    [HideInInspector] public NetworkVariable<int> playerExpierence = new(0);
    [HideInInspector] public NetworkVariable<int> playerLevel = new(1);
    [HideInInspector] public NetworkVariable<TeamType> teamType = new(TeamType.None);
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
        damagable.teamType.Value = teamType.Value;
        no.SpawnWithOwnership(clientId);

        RTSObjectsManager.AddUnitServerRpc(no);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddExpiernceServerRpc(int amount)
    {
        if (playerLevel.Value == playerLevelSo.levelsData.Count) return;

        var playerExp = playerExpierence.Value;
        playerExp += amount;
        var nextLevelData = playerLevelSo.levelsData[playerLevel.Value];
        var diffrence = playerExp - nextLevelData.expToNextLevel;

        if (playerLevel.Value < playerLevelSo.levelsData.Count && playerExp >= nextLevelData.expToNextLevel)
        {
            playerLevel.Value++;
            playerExp = diffrence;
            var playerSkillTree = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponentInChildren<SkillTreeManager>();

            playerSkillTree.AddSkillPointsServerRpc(1);
        }

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
    private void SpawnUnitServerRpc(Vector3 spawnPosition, ulong clientId)
    {
        SpawnHero(clientId, spawnPosition);
        spawnPosition += new Vector3(2, 0, 0);

        foreach (var unitPrefab in unitPrefabs)
        {
            for (int i = 0; i < 1; i++)
            {
                var unit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
                var unitMovement = unit.GetComponent<UnitMovement>();
                var no = unit.GetComponent<NetworkObject>();
                var damagable = unit.GetComponent<Damagable>();

                unitMovement.agent.enabled = true;

                if (unitMovement != null) unitMovement.isReachedDestinationAfterSpawn = true;

                spawnPosition += new Vector3(2, 0, 0);
                damagable.teamType.Value = teamType.Value;
                no.SpawnWithOwnership(clientId);
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
        if (!IsOwner) return;
        playerLevel.OnValueChanged += OnPlayerLevelChangeHandler;
        playerExpierence.OnValueChanged += OnPlayerExpierenceChangeHandler;

        if (GameManager.Instance.IsDebug)
        {
            var team = OwnerClientId % 2 == 0 ? TeamType.Blue : TeamType.Red;
            SetTeamServerRpc(team);
        }
        else
        {
            var team = LobbyManager.Instance.playerLobbyData.Team;
            Debug.Log("OnNetworkSpawn " + team);
            SetTeamServerRpc(team);
        }
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
        Debug.Log($"PlayerController Start {OwnerClientId} {MultiplayerController.Instance}");

        if (IsOwner)
        {
            playerData.playerColor = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId].playerColor;
            playerData.playerMaterial = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId].playerMaterial;
            playerData.spawnPosition = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId].spawnPosition.position;

            var cameraSystem = FindAnyObjectByType<CameraSystem>();
            cameraSystem.SetCameraPosition(new Vector3(playerData.spawnPosition.x, cameraSystem.transform.position.y, playerData.spawnPosition.z));
            SpawnUnitServerRpc(playerData.spawnPosition, OwnerClientId);
        }

        if (IsServer)
        {
            AddExpiernceServerRpc(1);
            playerData.teamId = NetworkManager.Singleton.ConnectedClients.Count;
        }
    }

    private void Update()
    {
        if (IsClient && IsOwner)
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
