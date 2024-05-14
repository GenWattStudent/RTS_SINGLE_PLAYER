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

    private float heartbeatTimer = 0.0f;
    private RoomUi RoomUi;

    private async void Start()
    {
        RoomUi = GetComponent<RoomUi>();
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += OnSignInCompleted;
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
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

    public async Task JoinLobby(string lobbyId)
    {
        CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
        RoomUi.JoinRoom(CurrentLobby);
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
        RoomUi.JoinRoom(CurrentLobby);
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
}

