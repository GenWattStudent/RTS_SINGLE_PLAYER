using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyRoomService : NetworkBehaviour
{
    [HideInInspector] public NetworkVariable<bool> isLoading = new(false);
    public static LobbyRoomService Instance;
    public LobbyPlayersHandler lobbyPlayersHandler;
    public LobbyNetcodeDataHandler lobbyNetcodeDataHandler;
    public NetworkList<PlayerNetcodeLobbyData> PlayerNetcodeLobbyData => lobbyPlayersHandler.playerNetcodeLobbyData;

    private int completedPlayers = 0;
    private SceneLoader sceneLoader;

    public event Action OnAllPlayersLoaded;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        isLoading.OnValueChanged += HandleLoadingChange;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandlePlayerDisconnect;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        isLoading.OnValueChanged -= HandleLoadingChange;
    }

    private void RemoveNetworkManagerCallbacks()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandlePlayerDisconnect;

            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApproval;
            }
        }
    }

    private void Awake()
    {
        sceneLoader = GetComponent<SceneLoader>();
        lobbyPlayersHandler = GetComponent<LobbyPlayersHandler>();
        lobbyNetcodeDataHandler = GetComponent<LobbyNetcodeDataHandler>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void HandlePlayerDisconnect(ulong clientId)
    {
        lobbyPlayersHandler.RemovePlayer(clientId);

        if (NetworkManager.LocalClientId == clientId)
        {
            Debug.Log("Disconnected from server");
            Exit();
        }
    }

    private void HandleLoadingChange(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            sceneLoader.ShowSceneLoading(PlayerNetcodeLobbyData);
        }
        else
        {
            sceneLoader.HideSceneLoading();
        }
    }

    private void Start()
    {
        PlayerNetcodeLobbyData.OnListChanged += PlayerListChanged;
    }

    private void PlayerListChanged(NetworkListEvent<PlayerNetcodeLobbyData> changeEvent)
    {
        sceneLoader.CreatePlayerList(PlayerNetcodeLobbyData);
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
        lobbyPlayersHandler.Clear();
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

        var playerData = new PlayerNetcodeLobbyData(playerConnectionData, request.ClientNetworkId);
        lobbyPlayersHandler.AddPlayerToDict(playerData);

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
            lobbyPlayersHandler.SetPlayerProgressServerRpc((int)(asyncOperation.progress * 100));

            yield return new WaitForSeconds(0.01f);
        }
    }
}
