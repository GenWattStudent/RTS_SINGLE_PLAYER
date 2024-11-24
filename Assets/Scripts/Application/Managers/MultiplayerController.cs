using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerData
{
    public int teamId;
    public Color playerColor;
    public Material playerMaterial;
    public List<Unit> units = new();
    public List<Building> buildings = new();
    public Vector3 spawnPosition = new Vector3(1.5f, 0, 2f);
}

[Serializable]
public class PlayerVisualData
{
    public Color playerColor;
    public Material playerMaterial;
    public Transform spawnPosition;
}

[DefaultExecutionOrder(-100)]
public class MultiplayerController : NetworkBehaviour
{
    public List<PlayerVisualData> playerMaterials = new();
    public static MultiplayerController Instance;
    public GameObject playerPrefab;

    public event Action OnAllPlayersLoad;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (GameManager.Instance.IsDebug)
        {
            OnAllPlayersLoad?.Invoke();
        }
    }

    private void Start()
    {
        if (!GameManager.Instance.IsDebug)
        {
            LobbyRoomService.Instance.OnAllPlayersLoaded += OnAllPlayersLoaded;
        }
    }

    private void OnDisable()
    {
        if (!GameManager.Instance.IsDebug)
        {
            LobbyRoomService.Instance.OnAllPlayersLoaded -= OnAllPlayersLoaded;
        }
    }

    private void OnAllPlayersLoaded()
    {
        if (!IsServer) return;

        foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            var no = playerObject.GetComponent<NetworkObject>();

            no.SpawnAsPlayerObject(player.ClientId);
        }

        OnAllPlayersLoad?.Invoke();
    }
}
