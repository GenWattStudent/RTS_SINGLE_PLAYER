using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyPlayersHandler : NetworkBehaviour
{
    public Dictionary<ulong, PlayerNetcodeLobbyData> playerNetcodeLobbyDataDict;
    public NetworkList<PlayerNetcodeLobbyData> playerNetcodeLobbyData;

    public static LobbyPlayersHandler Instance;

    private void Awake()
    {
        Instance = this;
        playerNetcodeLobbyDataDict = new();
        playerNetcodeLobbyData = new();
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
    }

    private int GetFreeColorIndex()
    {
        int index = 0;

        for (int i = 0; i < playerNetcodeLobbyData.Count; i++)
        {
            for (int j = 0; j < MultiplayerController.Instance.playerMaterials.Count; j++)
            {
                if (playerNetcodeLobbyData[i].playerColor == MultiplayerController.Instance.playerMaterials[j].playerColor)
                {
                    index++;
                    break;
                }
            }
        }

        return index;
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var savedPlayerData = playerNetcodeLobbyDataDict[clientId];
            var playerIndex = GetFreeColorIndex();
            Debug.Log($"Player {clientId} connected with index {playerIndex}");
            Color? notSelectedColor = MultiplayerController.Instance.playerMaterials[playerIndex].playerColor;

            savedPlayerData.Team = playerIndex % 2 == 0 ? TeamType.Blue : TeamType.Red;
            savedPlayerData.playerColor = (Color)notSelectedColor;
            savedPlayerData.IsReady = false;
            savedPlayerData.PlayerIndex = playerIndex;

            playerNetcodeLobbyData.Add(savedPlayerData);
        }
    }

    public void RemovePlayer(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (playerNetcodeLobbyDataDict.ContainsKey(clientId))
            {
                playerNetcodeLobbyDataDict.Remove(clientId);
            }

            for (int i = 0; i < playerNetcodeLobbyData.Count; i++)
            {
                if (playerNetcodeLobbyData[i].NetcodePlayerId == clientId)
                {
                    Debug.Log($"Player {clientId} disconnected");
                    playerNetcodeLobbyData.RemoveAt(i);
                    break;
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        for (int i = 0; i < playerNetcodeLobbyData.Count; i++)
        {
            if (playerNetcodeLobbyData[i].NetcodePlayerId == rpcParams.Receive.SenderClientId)
            {
                var playerData = playerNetcodeLobbyData[i];
                playerData.IsReady = !playerData.IsReady;
                playerNetcodeLobbyData[i] = playerData;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTeamServerRpc(TeamType team, ServerRpcParams rpcParams = default)
    {

    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerProgressServerRpc(int progress, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"Player {rpcParams.Receive.SenderClientId} progress: {progress}");
        for (int i = 0; i < playerNetcodeLobbyData.Count; i++)
        {
            if (playerNetcodeLobbyData[i].NetcodePlayerId == rpcParams.Receive.SenderClientId)
            {
                var playerData = playerNetcodeLobbyData[i];
                playerData.Progress = progress;
                playerNetcodeLobbyData[i] = playerData;
            }
        }
    }

    private void SetPlayerProgress(PlayerNetcodeLobbyData? playerData, int progress)
    {
        if (playerData.HasValue)
        {
            var nonNullablePlayerData = playerData.Value;
            nonNullablePlayerData.Progress = progress;
            playerNetcodeLobbyData[playerNetcodeLobbyData.IndexOf(nonNullablePlayerData)] = nonNullablePlayerData;
        }
    }

    public PlayerNetcodeLobbyData? GetPlayerData(ulong clientId)
    {
        PlayerNetcodeLobbyData? playerData = null;

        for (int i = 0; i < playerNetcodeLobbyData.Count; i++)
        {
            if (playerNetcodeLobbyData[i].NetcodePlayerId == clientId)
            {
                playerData = playerNetcodeLobbyData[i];
                break;
            }
        }

        return playerData;
    }

    public void Clear()
    {
        playerNetcodeLobbyDataDict.Clear();
        if (NetworkManager.Singleton.IsServer) playerNetcodeLobbyData.Clear();
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
    }

    public void AddPlayerToDict(PlayerNetcodeLobbyData playerData)
    {
        playerNetcodeLobbyDataDict.Add(playerData.NetcodePlayerId, playerData);
    }
}
