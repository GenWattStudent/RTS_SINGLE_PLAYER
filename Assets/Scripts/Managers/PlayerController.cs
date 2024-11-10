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
    public LatencyManager LatencyManager;

    [HideInInspector] public NetworkVariable<int> playerExperience = new(0);
    [HideInInspector] public NetworkVariable<int> playerLevel = new(1);
    [HideInInspector] public NetworkVariable<TeamType> teamType = new(TeamType.None);

    private RTSObjectsManager RTSObjectsManager;

    public event Action<int, int, int, int> OnPlayerLevelChange;

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

        var playerExp = playerExperience.Value;
        playerExp += amount;

        while (playerLevel.Value < playerLevelSo.levelsData.Count)
        {
            var nextLevelData = playerLevelSo.levelsData[playerLevel.Value];
            if (playerExp >= nextLevelData.expToNextLevel)
            {
                playerExp -= nextLevelData.expToNextLevel;
                playerLevel.Value++;
                var playerSkillTree = NetworkManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.GetComponentInChildren<SkillTreeManager>();
                playerSkillTree.AddSkillPointsServerRpc(1);
            }
            else
            {
                break;
            }
        }

        playerExperience.Value = playerExp;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnUnitServerRpc(Vector3 spawnPosition, ulong clientId)
    {
        SpawnHero(clientId, spawnPosition);
        spawnPosition += new Vector3(2, 0, 0);

        foreach (var unitPrefab in unitPrefabs)
        {
            for (int i = 0; i < 6; i++)
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

        OnPlayerLevelChange?.Invoke(expToNextLevel, playerExperience.Value, current, playerLevelSo.levelsData.Count);
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

        if (IsOwner)
        {
            playerLevel.OnValueChanged += OnPlayerLevelChangeHandler;
            playerExperience.OnValueChanged += OnPlayerExpierenceChangeHandler;

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

            var spawnPositions = GameObject.Find("PlayerSpawnPoints");
            playerData.playerColor = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId].playerColor;
            playerData.playerMaterial = MultiplayerController.Instance.playerMaterials[(int)OwnerClientId].playerMaterial;
            playerData.spawnPosition = spawnPositions.transform.GetChild((int)OwnerClientId).position;

            var cameraSystem = FindAnyObjectByType<CameraSystem>();
            cameraSystem.SetCameraPosition(new Vector3(playerData.spawnPosition.x, cameraSystem.transform.position.y, playerData.spawnPosition.z));
        }
    }

    private void Awake()
    {
        playerData = new PlayerData();
        LatencyManager = GetComponent<LatencyManager>();
        RTSObjectsManager = GetComponent<RTSObjectsManager>();
    }

    private void Start()
    {
        if (IsOwner)
        {
            SpawnUnitServerRpc(playerData.spawnPosition, OwnerClientId);
        }

        if (IsServer)
        {
            AddExpiernceServerRpc(1);
        }
    }

    [ServerRpc()]
    private void SetTeamServerRpc(TeamType teamType)
    {
        this.teamType.Value = teamType;
    }
}
