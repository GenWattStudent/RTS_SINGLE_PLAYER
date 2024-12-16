using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : Singleton<LobbyManager>
{
    [SerializeField] private float hearbeatInterval = 25.0f;
    public bool isSingInCompleted = false;
    public string playerId;
    public string playerName;
    public bool isHost = false;
    public int maxPlayers = 4;
    public LobbyData lobbyData;
    public Lobby CurrentLobby => lobbyData?.CurrentLobby;
    public PlayerLobbyData playerLobbyData;
    public List<PlayerLobbyData> lobbyPlayers = new();

    private float heartbeatTimer = 0.0f;
    private RoomUi RoomUi;

    public override void Awake()
    {
        base.Awake();
        lobbyData = new LobbyData();
    }

    private async void Start()
    {
        RoomUi = FindAnyObjectByType<RoomUi>();

        var initOptions = new InitializationOptions();
        playerName = $"{PlayerPrefs.GetString("username")}";
        initOptions.SetProfile(playerName);
        await UnityServices.InitializeAsync(initOptions);

        AuthenticationService.Instance.SignedIn += OnSignInCompleted;

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        else
        {
            OnSignInCompleted();
        }

        DontDestroyOnLoad(gameObject);
    }

    private async void Update()
    {
        if (!isSingInCompleted) return;
        await Heartbeat();
    }

    public bool HasLobbyDataValue(string key) => CurrentLobby != null && CurrentLobby.Data != null && CurrentLobby.Data.ContainsKey(key) &&
            CurrentLobby.Data[key].Value != default;
    public bool IsHost() => CurrentLobby != null && CurrentLobby.HostId == playerId;
    public bool IsHostByPlayerId(string playerId) => CurrentLobby != null && playerId == CurrentLobby.HostId;
    public bool HasPlayerDataValue(string key, Player player) => CurrentLobby != null && player.Data != null
        && player.Data.ContainsKey(key) && player.Data[key] != null;

    public Dictionary<TeamType, int> GetTeamPlayersCount(Lobby lobby)
    {
        Dictionary<TeamType, int> teamPlayersCount = new Dictionary<TeamType, int>
        {
            { TeamType.Blue, 0 },
            { TeamType.Red, 0 }
        };

        foreach (var player in lobby.Players)
        {
            if (HasPlayerDataValue("Team", player))
            {
                teamPlayersCount[(TeamType)Enum.Parse(typeof(TeamType), player.Data["Team"].Value)]++;
            }
        }

        return teamPlayersCount;
    }

    public async Task JoinLobby(string lobbyId)
    {
        lobbyData.CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
        playerLobbyData = new PlayerLobbyData(playerId, playerName);

        var teamPlayersCount = GetTeamPlayersCount(CurrentLobby);
        var teamWithLowestPlayers = teamPlayersCount.FirstOrDefault(x => x.Value == teamPlayersCount.Values.Min()).Key;

        await lobbyData.GetLobbyData(lobbyId);
        await playerLobbyData.SetTeam(teamWithLowestPlayers, CurrentLobby.Id);

        await JoinRelayServer(lobbyData.RelayCode);
        await RoomUi.JoinRoom(this);
    }

    public async Task LeaveLobby(string playerId)
    {
        if (CurrentLobby == null) return;
        await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);
        lobbyData.CurrentLobby = null;
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
    }

    public async Task CreateLobby(string lobbyName, int maxPlayers)
    {
        Debug.Log("Creating lobby");
        playerLobbyData = new PlayerLobbyData(playerId, playerName, TeamType.Blue);

        var lobbyOptions = new CreateLobbyOptions
        {
            Player = new Player { Data = playerLobbyData.Get() },
            IsPrivate = false,
        };

        lobbyData.CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);
        Debug.Log("Lobby created");
        await StartRelay();
        Debug.Log("Relay started");
        await RoomUi.JoinRoom(this);
        Debug.Log("Room joined");
    }

    public async Task<List<Lobby>> GetAll()
    {
        // Get all lobbies but not full ones and not started ones
        QueryLobbiesOptions options = new QueryLobbiesOptions();
        options.Filters = new List<QueryFilter>()
        {
            new QueryFilter(
                field: QueryFilter.FieldOptions.AvailableSlots,
                op: QueryFilter.OpOptions.GT,
                value: "0"),
            new QueryFilter(
                field: QueryFilter.FieldOptions.S1,
                op: QueryFilter.OpOptions.EQ,
                value: "false"),
        };

        var lobbies = await Lobbies.Instance.QueryLobbiesAsync();
        return lobbies.Results;
    }

    private async Task Heartbeat()
    {
        if (CurrentLobby == null || !IsHost()) return;

        heartbeatTimer += Time.deltaTime;

        if (heartbeatTimer < hearbeatInterval) return;

        heartbeatTimer = 0.0f;
        Debug.Log("Sending heartbeat ping");
        await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
    }

    public async Task StartRelay()
    {
        if (CurrentLobby == null) return;
        string code = await RelayManager.Instance.CreateRelay(CurrentLobby.MaxPlayers);

        await lobbyData.SetRelayCode(code, CurrentLobby.Id);
        await playerLobbyData.SetConnectionData(
            RelayManager.Instance.AllocationId.ToString(),
            Convert.ToBase64String(RelayManager.Instance.ConnectionData),
            CurrentLobby.Id);
    }

    public async Task JoinRelayServer(string code)
    {
        await RelayManager.Instance.JoinRelay(code);
        await playerLobbyData.SetConnectionData(RelayManager.Instance.AllocationId.ToString(), Convert.ToBase64String(RelayManager.Instance.ConnectionData), CurrentLobby.Id);
    }
}

