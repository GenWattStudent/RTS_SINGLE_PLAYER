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
    public List<Selectable> selectableObjects = new();
    public List<Building> buildings = new();
    public Vector3 spawnPosition = new Vector3(1.5f, 0, 2f);
    public int playerLevel = 1;
    public int playerExpierence = 0;
    public BuildingSo selectedBuilding;
}

[Serializable]
public class PlayerVisualData
{
    public Color playerColor;
    public Material playerMaterial;
}

public class MultiplayerController : NetworkBehaviour
{
    public List<PlayerVisualData> playerMaterials = new();
    public static MultiplayerController Instance;
    public Dictionary<ulong, PlayerData> playerData = new();

    public event Action<ulong, PlayerData> OnPlayerJoined;

    public void Add(ulong clientId)
    {
        var data = new PlayerData
        {
            playerColor = playerMaterials[playerData.Count].playerColor,
            playerMaterial = playerMaterials[playerData.Count].playerMaterial,
            teamId = playerData.Count,
            spawnPosition = new Vector3(1.5f + (5 * playerData.Count), 0, 2f * playerData.Count)
        };

        playerData.Add(clientId, data);
        OnPlayerJoined?.Invoke(clientId, data);
    }

    public void Remove(ulong clientId)
    {
        playerData.Remove(clientId);
    }

    public PlayerData Get(ulong clientId)
    {
        return playerData[clientId];
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        Debug.Log($"Client {clientId} connected");
        Add(clientId);
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (!IsServer) return;
        Debug.Log($"Client {clientId} disconnected");
        Remove(clientId);
    }

    private void Start()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        Instance = this;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    public override void OnDestroy()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
    }
}
