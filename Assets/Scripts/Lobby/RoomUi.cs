using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomUi : NetworkToolkitHelper
{
    [SerializeField] private GameObject playerPrefab;
    public float updateInterval = 3.0f;
    public bool isReady = false;
    public bool isGameStarted = false;
    public bool isInRoom = false;

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

        readyButton.clicked += async () => await Ready();
        exitButton.clicked += async () => await Exit();
        startGameButton.clicked += StartGame;

        startGameButton.SetEnabled(false);
    }

    private void OnDisable()
    {
        readyButton.clicked -= async () => await Ready();
        exitButton.clicked -= async () => await Exit();
        startGameButton.clicked -= StartGame;

        startGameButton.SetEnabled(false);
    }

    private async Task Ready()
    {
        isReady = !isReady;
        await lobbyManager.playerLobbyData.SetReady(isReady, lobbyManager.CurrentLobby.Id);
    }

    private async Task Exit()
    {
        try
        {
            await lobbyManager.LeaveLobby(AuthenticationService.Instance.PlayerId);
            isInRoom = false;
            ShowLobbyAndHideRoom();
            LobbyRoomService.Instance.Exit();
        }
        catch (System.Exception)
        {
            Debug.Log("Failed to leave lobby");
        }
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

    private void CheckIfAllPlayersReady()
    {
        // if (lobbyManager.CurrentLobby.Players.Count < 2) return;
        if (isReady) readyButton.AddToClassList("active");
        else readyButton.RemoveFromClassList("active");

        foreach (var player in lobbyManager.CurrentLobby.Players)
        {
            if (lobbyManager.HasPlayerDataValue("IsReady", player) && player.Data["IsReady"].Value == "False")
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
        lobbyPlayerList.CreatePlayerItems(lobbyManager.CurrentLobby.Players);
        lobbyPlayerList.CheckPlayerReadyStatus();
        CheckIfAllPlayersReady();
        lobbyGameSetup.Update();
        UpdatePlayersLobbyData();

        JoinGameScene();
    }

    private void UpdatePlayersLobbyData()
    {
        foreach (var player in lobbyManager.CurrentLobby.Players)
        {
            LobbyManager.Instance.lobbyPlayers.Add(new PlayerLobbyData(player));
        }
    }

    private void JoinGameScene()
    {
        if (!isGameStarted && lobbyManager.HasLobbyDataValue("Started") && lobbyManager.lobbyData.Started)
        {
            var map = lobbyManager.CurrentLobby.Data["MapName"].Value;

            LobbyRoomService.Instance.ChangeScene(map);

            lobbyGameSetup.OnMapSelected -= OnMapSelected;
            lobby.style.display = DisplayStyle.None;
            room.style.display = DisplayStyle.None;
            isGameStarted = true;
        }
    }

    public async Task JoinRoom(Lobby lobby)
    {
        if (RelayManager.Instance.IsHost)
        {
            await Ready();
        }

        await UpdateRoomData();
        HideLobbyAndShowRoom();
        isInRoom = true;

        LobbyRoomService.Instance.StartNetcode();
    }

    private async void StartGame()
    {
        // try
        // {
        await lobbyManager.lobbyData.SetStarted(true, lobbyManager.CurrentLobby.Id);
        await lobbyManager.StartGame();
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
