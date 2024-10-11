using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct PlayerNetcodeLobbyData : INetworkSerializable, IEquatable<PlayerNetcodeLobbyData>
{
    public FixedString32Bytes LobbyPlayerId;
    public ulong NetcodePlayerId;
    public FixedString32Bytes PlayerName;
    public bool IsReady;
    public TeamType Team;
    public Color playerColor;

    public bool Equals(PlayerNetcodeLobbyData other)
    {
        return LobbyPlayerId == other.LobbyPlayerId &&
            NetcodePlayerId == other.NetcodePlayerId &&
            IsReady == other.IsReady &&
            Team == other.Team &&
            playerColor == other.playerColor &&
            PlayerName == other.PlayerName;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref LobbyPlayerId);
        serializer.SerializeValue(ref NetcodePlayerId);
        serializer.SerializeValue(ref IsReady);
        serializer.SerializeValue(ref Team);
        serializer.SerializeValue(ref playerColor);
    }
}

[Serializable]
public class PlayerConnectionData
{
    public string PlayerId;
    public string PlayerName;

    public PlayerConnectionData(string playerId, string playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
    }

    public byte[] ToByteArray()
    {
        string json = JsonUtility.ToJson(this);
        return Encoding.UTF8.GetBytes(json);
    }

    public static PlayerConnectionData FromByteArray(byte[] data)
    {
        string json = Encoding.UTF8.GetString(data);
        return JsonUtility.FromJson<PlayerConnectionData>(json);
    }
}

public class LobbyRoomService : NetworkBehaviour
{
    private int completedPlayers = 0;
    private SceneLoader sceneLoader;

    [HideInInspector] public NetworkVariable<float> progress = new(0.0f);
    [HideInInspector] public NetworkVariable<bool> isLoading = new(false);
    public static LobbyRoomService Instance;
    public Dictionary<ulong, PlayerNetcodeLobbyData> playerNetcodeLobbyDataDict;
    public NetworkList<PlayerNetcodeLobbyData> playerNetcodeLobbyData;

    public event Action OnAllPlayersLoaded;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        progress.OnValueChanged += HandleProgressChange;
        isLoading.OnValueChanged += HandleLoadingChange;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandlePlayerDisconnect;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        progress.OnValueChanged -= HandleProgressChange;
        isLoading.OnValueChanged -= HandleLoadingChange;
    }

    private void RemoveNetworkManagerCallbacks()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandlePlayerDisconnect;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;

            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApproval;
            }
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var savedPlayerData = playerNetcodeLobbyDataDict[clientId];
            Color? notSelectedColor = MultiplayerController.Instance.playerMaterials[(int)clientId].playerColor;

            savedPlayerData.Team = clientId % 2 == 0 ? TeamType.Blue : TeamType.Red;
            savedPlayerData.playerColor = (Color)notSelectedColor;
            savedPlayerData.IsReady = false;

            playerNetcodeLobbyData.Add(savedPlayerData);
        }
    }

    private void Awake()
    {
        sceneLoader = FindAnyObjectByType<SceneLoader>();
        playerNetcodeLobbyDataDict = new();
        playerNetcodeLobbyData = new();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
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

    private void HandlePlayerDisconnect(ulong clientId)
    {
        Debug.Log($"Player {clientId} disconnected");

        RemovePlayer(clientId);

        if (NetworkManager.LocalClientId == clientId)
        {
            Debug.Log("Disconnected from server");
            Exit();
        }
    }

    private void HandleProgressChange(float previousValue, float newValue)
    {
        sceneLoader.SetProgress(Mathf.RoundToInt(newValue));
    }

    private void HandleLoadingChange(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            sceneLoader.SetProgress(0);
            sceneLoader.ShowSceneLoading();
        }
        else
        {
            sceneLoader.HideSceneLoading();
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

    public void StartNetcode(string playerName)
    {
        var playerConnectionData = new PlayerConnectionData(AuthenticationService.Instance.PlayerId, playerName);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = playerConnectionData.ToByteArray();

        if (RelayManager.Instance.IsHost)
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApproval;
            RelayServerData relayServerData = new RelayServerData(RelayManager.Instance.Allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            RelayServerData relayServerData = new RelayServerData(RelayManager.Instance.JoinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }
    }

    public void ChangeScene(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplate;
        NetworkManager.Singleton.SceneManager.OnLoad += OnLoad;

        if (NetworkManager.Singleton.IsServer)
        {
            isLoading.Value = true;
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    public void Exit()
    {
        playerNetcodeLobbyDataDict.Clear();
        if (NetworkManager.Singleton.IsServer) playerNetcodeLobbyData.Clear();
        RemoveNetworkManagerCallbacks();
        NetworkManager.Singleton.Shutdown();
    }

    public void KickPlayer(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.DisconnectClient(clientId);
        }
    }

    private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        PlayerConnectionData playerConnectionData = PlayerConnectionData.FromByteArray(request.Payload);

        if (string.IsNullOrEmpty(playerConnectionData.PlayerId))
        {
            response.Approved = false;
            return;
        }

        var playerData = new PlayerNetcodeLobbyData
        {
            LobbyPlayerId = playerConnectionData.PlayerId,
            NetcodePlayerId = request.ClientNetworkId,
            PlayerName = playerConnectionData.PlayerName ?? playerConnectionData.PlayerId,
        };

        playerNetcodeLobbyDataDict.Add(request.ClientNetworkId, playerData);

        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = false;
    }

    private void OnLoadComplate(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        completedPlayers++;

        if (NetworkManager.Singleton.IsServer && completedPlayers == NetworkManager.Singleton.ConnectedClients.Count)
        {
            OnAllPlayersLoaded?.Invoke();
            isLoading.Value = false;
        }
    }

    private void OnLoad(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
    {
        StartCoroutine(TrackSceneLoadProgress(asyncOperation));
    }

    private IEnumerator TrackSceneLoadProgress(AsyncOperation asyncOperation)
    {
        while (!asyncOperation.isDone)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                progress.Value = asyncOperation.progress;
            }

            yield return new WaitForSeconds(0.01f);
        }
    }
}
