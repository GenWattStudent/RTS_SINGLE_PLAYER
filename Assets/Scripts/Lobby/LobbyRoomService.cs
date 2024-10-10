using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerIdentifiers
{
    public string LobbyPlayerId;
    public ulong NetcodePlayerId;
}

public class LobbyRoomService : NetworkBehaviour
{
    private int completedPlayers = 0;
    private SceneLoader sceneLoader;

    [HideInInspector] public NetworkVariable<float> progress = new(0.0f);
    [HideInInspector] public NetworkVariable<bool> isLoading = new(false);
    public static LobbyRoomService Instance;
    // public Dictionary<PlayerIdentifiers, >

    public event Action OnAllPlayersLoaded;

    public override void OnNetworkSpawn()
    {
        progress.OnValueChanged += HandleProgressChange;
        isLoading.OnValueChanged += HandleLoadingChange;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        progress.OnValueChanged -= HandleProgressChange;
        isLoading.OnValueChanged -= HandleLoadingChange;
    }

    private void Awake()
    {
        sceneLoader = FindAnyObjectByType<SceneLoader>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
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

    public void StartNetcode()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.UTF8.GetBytes(LobbyManager.Instance.playerId);

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
        NetworkManager.Singleton.Shutdown();
    }

    private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string playerId = System.Text.Encoding.UTF8.GetString(request.Payload);

        Debug.Log($"ConnectionApproval: {playerId}");

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
