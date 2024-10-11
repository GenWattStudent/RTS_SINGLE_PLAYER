using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomUi : NetworkToolkitHelper
{
    [SerializeField] private GameObject playerPrefab;
    public float updateInterval = 3.0f;
    public bool isInRoom = false;
    [HideInInspector] NetworkVariable<bool> IsGameStarted = new(false);

    private float updateTimer = 0;
    private Button readyButton;
    private Button startGameButton;
    private Button exitButton;
    private VisualElement lobby;
    private VisualElement room;

    private LobbyManager lobbyManager;
    private LobbyGameSetup lobbyGameSetup;
    private LobbyPlayerList lobbyPlayerList;

    protected override void OnEnable()
    {
        base.OnEnable();
        lobbyManager = FindAnyObjectByType<LobbyManager>();
        lobbyPlayerList = GetComponent<LobbyPlayerList>();
        lobbyGameSetup = FindAnyObjectByType<LobbyGameSetup>();

        startGameButton = GetButton("StartGame");
        exitButton = GetButton("Exit");
        lobby = GetVisualElement("Lobby");
        room = GetVisualElement("Room");
        readyButton = GetButton("ReadyButton");

        readyButton.clicked += Ready;
        exitButton.clicked += async () => await Exit();
        startGameButton.clicked += StartGame;

        startGameButton.SetEnabled(false);
    }

    private void Start()
    {
        LobbyRoomService.Instance.playerNetcodeLobbyData.OnListChanged += PlayerListChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        IsGameStarted.OnValueChanged += HandleGameStarted;
    }

    private void HandleGameStarted(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            CloseRoom();
        }
    }

    private void OnDisable()
    {
        readyButton.clicked -= Ready;
        exitButton.clicked -= async () => await Exit();
        startGameButton.clicked -= StartGame;

        startGameButton.SetEnabled(false);
    }

    private void Ready()
    {
        LobbyRoomService.Instance.SetReadyServerRpc();
    }

    private async Task Exit()
    {
        // try
        // {
        LobbyRoomService.Instance.Exit();
        await lobbyManager.LeaveLobby(AuthenticationService.Instance.PlayerId);
        isInRoom = false;
        ShowLobbyAndHideRoom();
        // }
        // catch (Exception)
        // {
        //     Debug.Log("Failed to leave lobby");
        // }
    }

    private void HideLobbyAndShowRoom()
    {
        lobby.style.display = DisplayStyle.None;
        room.style.display = DisplayStyle.Flex;

        lobbyGameSetup.OnMapSelected += OnMapSelected;
        lobbyGameSetup.Initialize();
    }

    private void ShowLobbyAndHideRoom()
    {
        lobby.style.display = DisplayStyle.Flex;
        room.style.display = DisplayStyle.None;

        lobbyGameSetup.OnMapSelected -= OnMapSelected;
    }

    public async Task HideLobbyAndRoom()
    {
        if (lobbyManager.CurrentLobby != null)
        {
            await Exit();
        }

        lobby.style.display = DisplayStyle.None;
        room.style.display = DisplayStyle.None;

        lobbyGameSetup.OnMapSelected -= OnMapSelected;
    }

    private async void OnMapSelected(MapSo map)
    {
        if (lobbyManager.CurrentLobby == null || !lobbyManager.IsHost()) return;
        await lobbyManager.lobbyData.SetMapName(map.MapName, lobbyManager.CurrentLobby.Id);
    }

    private void CheckPlayerInLobby()
    {
        if (lobbyManager.CurrentLobby == null) return;
        if (lobbyManager.CurrentLobby.Players.Find(player => player.Id == AuthenticationService.Instance.PlayerId) == null)
        {
            isInRoom = false;
            ShowLobbyAndHideRoom();
        }
    }

    private void CheckIfAllPlayersReady(NetworkList<PlayerNetcodeLobbyData> players)
    {

        PlayerNetcodeLobbyData? me = null;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].NetcodePlayerId == NetworkManager.Singleton.LocalClientId)
            {
                me = players[i];
                break;
            }
        }

        if (me.HasValue && me.Value.IsReady) readyButton.AddToClassList("active");
        else readyButton.RemoveFromClassList("active");

        if (players.Count < 2) return;

        foreach (var player in players)
        {
            if (!player.IsReady)
            {
                startGameButton.SetEnabled(false);
                return;
            }
        }

        startGameButton.SetEnabled(true);
    }

    private void HideStartButtonIfNotHost()
    {
        if (lobbyManager.CurrentLobby.HostId != AuthenticationService.Instance.PlayerId)
        {
            startGameButton.style.display = DisplayStyle.None;
            return;
        }

        startGameButton.style.display = DisplayStyle.Flex;
    }

    private async Task UpdateRoomData()
    {
        await lobbyManager.lobbyData.GetLobbyData(lobbyManager.CurrentLobby.Id);
        HideStartButtonIfNotHost();
        CheckPlayerInLobby();
        lobbyGameSetup.Update();
        UpdatePlayersLobbyData();
    }

    private void UpdatePlayersLobbyData()
    {
        foreach (var player in lobbyManager.CurrentLobby.Players)
        {
            LobbyManager.Instance.lobbyPlayers.Add(new PlayerLobbyData(player));
        }
    }

    private void CloseRoom()
    {
        lobbyGameSetup.OnMapSelected -= OnMapSelected;
        room.style.display = DisplayStyle.None;
    }

    private void JoinGameScene()
    {
        if (!IsGameStarted.Value && RelayManager.Instance.IsHost)
        {
            var map = lobbyManager.CurrentLobby.Data["MapName"].Value;

            LobbyRoomService.Instance.ChangeScene(map);
            IsGameStarted.Value = true;
        }
    }

    public async Task JoinRoom(LobbyManager lobby)
    {
        await UpdateRoomData();
        HideLobbyAndShowRoom();
        isInRoom = true;

        LobbyRoomService.Instance.StartNetcode(lobby.playerLobbyData.PlayerName);

        if (RelayManager.Instance.IsHost)
        {
            Ready();
        }
    }

    private void PlayerListChanged(NetworkListEvent<PlayerNetcodeLobbyData> changeEvent)
    {
        lobbyPlayerList.CreatePlayerItems(LobbyRoomService.Instance.playerNetcodeLobbyData);
        lobbyPlayerList.CheckPlayerReadyStatus(LobbyRoomService.Instance.playerNetcodeLobbyData);
        CheckIfAllPlayersReady(LobbyRoomService.Instance.playerNetcodeLobbyData);
    }

    private async void StartGame()
    {
        // try
        // {
        await lobbyManager.lobbyData.SetStarted(true, lobbyManager.CurrentLobby.Id);
        JoinGameScene();

        // }
        // catch (System.Exception e)
        // {
        //     Debug.Log("Failed to start game");
        //     Debug.LogError(e.Message);
        // }
    }

    private async void Update()
    {
        if (!isInRoom) return;

        updateTimer += Time.deltaTime;
        if (updateTimer < updateInterval) return;
        updateTimer = 0.0f;
        await UpdateRoomData();
    }
}
