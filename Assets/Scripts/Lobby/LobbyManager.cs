using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : ToolkitHelper
{
    [SerializeField] private float hearbeatInterval = 25.0f;
    public bool isSingInCompleted = false;
    public string playerId;
    public int maxPlayers = 2;
    public Lobby CurrentLobby;
    public LobbyData lobbyData;

    private float heartbeatTimer = 0.0f;
    private RoomUi RoomUi;

    private async void Start()
    {
        RoomUi = FindAnyObjectByType<RoomUi>();
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += OnSignInCompleted;
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        DontDestroyOnLoad(gameObject);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        lobbyData = new LobbyData();
    }

    private void Update()
    {
        if (!isSingInCompleted) return;
        Heartbeat();
    }

    private Dictionary<string, PlayerDataObject> GetPlayerData(bool isReady)
    {
        Dictionary<string, PlayerDataObject> playerData = new()
        {
            { "IsReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, isReady.ToString()) }
        };

        return playerData;
    }

    public async Task Ready(bool isReady, string playerId)
    {
        if (CurrentLobby == null) return;
        UpdatePlayerOptions options = new UpdatePlayerOptions
        {
            Data = GetPlayerData(isReady)
        };

        await LobbyService.Instance.UpdatePlayerAsync(CurrentLobby.Id, playerId, options);
    }

    private async Task UpdatePlayerData(string playerId, Dictionary<string, PlayerDataObject> playerData, string allocationId = default, string connectionData = default)
    {
        UpdatePlayerOptions options = new UpdatePlayerOptions
        {
            Data = playerData,
            AllocationId = allocationId,
            ConnectionInfo = connectionData
        };

        await LobbyService.Instance.UpdatePlayerAsync(CurrentLobby.Id, playerId, options);
    }

    public async Task JoinLobby(string lobbyId)
    {
        CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
        RoomUi.JoinRoom(CurrentLobby, false);
    }

    public async Task LeaveLobby(string playerId)
    {
        if (CurrentLobby == null) return;
        await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);
        CurrentLobby = null;
    }

    public async Task KickPlayer(string playerId)
    {
        if (CurrentLobby == null) return;
        await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);
    }

    private void OnSignInCompleted()
    {
        isSingInCompleted = true;
        playerId = AuthenticationService.Instance.PlayerId;
        Debug.Log("Sign in completed " + playerId);
    }

    public async Task CreateLobby(string lobbyName, int maxPlayers)
    {
        var lobbyOptions = new CreateLobbyOptions
        {
            Player = new Player { Data = GetPlayerData(false) },
            IsPrivate = false,
        };
        CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);
        RoomUi.JoinRoom(CurrentLobby, true);
    }

    public async Task<List<Lobby>> GetAll()
    {
        var lobbies = await Lobbies.Instance.QueryLobbiesAsync();
        return lobbies.Results;
    }

    public async Task GetLobbyData()
    {
        if (CurrentLobby == null) return;
        CurrentLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
    }

    private async void Heartbeat()
    {
        if (CurrentLobby == null) return;

        heartbeatTimer += Time.deltaTime;

        if (heartbeatTimer < hearbeatInterval) return;

        heartbeatTimer = 0.0f;
        await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
    }

    public async Task StartGame()
    {
        if (CurrentLobby == null) return;
        string code = await RelayManager.Instance.CreateRelay(CurrentLobby.MaxPlayers);

        Debug.Log("Relay code: " + code + " " + CurrentLobby.Id + " " + lobbyData);
        await lobbyData.SetRelayCode(code, CurrentLobby.Id);
        await UpdatePlayerData(playerId, new Dictionary<string, PlayerDataObject>(), RelayManager.Instance.AllocationId.ToString(), System.Convert.ToBase64String(RelayManager.Instance.ConnectionData));
    }

    public async Task JoinRelayServer(string code)
    {
        await RelayManager.Instance.JoinRelay(code);
        await UpdatePlayerData(playerId, new Dictionary<string, PlayerDataObject>(), RelayManager.Instance.AllocationId.ToString(), System.Convert.ToBase64String(RelayManager.Instance.ConnectionData));
    }
}

