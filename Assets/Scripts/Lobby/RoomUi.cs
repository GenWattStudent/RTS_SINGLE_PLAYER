using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class RoomUi : ToolkitHelper
{
    [SerializeField] private VisualTreeAsset roomItemTemplate;
    [SerializeField] private GameObject playerPrefab;
    public float updateInterval = 3.0f;
    public bool isReady = false;
    public bool isGameStarted = false;
    public bool isInRoom = false;

    private float updateTimer = 0;
    private Button readyButton;
    private VisualElement playerList;
    private Button startGameButton;
    private Button exitButton;
    private VisualElement lobby;
    private VisualElement room;
    private Dictionary<string, VisualElement> playerItems = new();
    private LobbyManager lobbyManager;
    private LobbyGameSetup lobbyGameSetup;

    private void Start()
    {
        lobbyManager = FindAnyObjectByType<LobbyManager>();

        readyButton = GetButton("ReadyButton");
        playerList = GetVisualElement("PlayerList");
        startGameButton = GetButton("StartGame");
        exitButton = GetButton("Exit");
        lobby = GetVisualElement("Lobby");
        room = GetVisualElement("Room");

        readyButton.clicked += Ready;
        exitButton.clicked += Exit;
        startGameButton.clicked += StartGame;

        startGameButton.SetEnabled(false);
    }

    private async void Ready()
    {
        isReady = !isReady;
        await lobbyManager.playerLobbyData.SetReady(isReady, lobbyManager.CurrentLobby.Id);
    }

    private void ChangePlayerItemStatus(VisualElement playerItem, bool isReady)
    {
        playerItem.Q<Label>("Status").AddToClassList(isReady ? "bg-success" : "bg-danger");
        playerItem.Q<Label>("Status").RemoveFromClassList(isReady ? "bg-danger" : "bg-success");
        playerItem.Q<Label>("Status").text = isReady ? "Ready" : "Not Ready";
    }

    private void ReadyUi()
    {
        if (isReady) readyButton.AddToClassList("active");
        else readyButton.RemoveFromClassList("active");

        foreach (var playerItem in playerItems)
        {
            foreach (var player in lobbyManager.CurrentLobby.Players)
            {
                if (playerItem.Key == player.Id && lobbyManager.HasPlayerDataValue("IsReady", player))
                {
                    ChangePlayerItemStatus(playerItem.Value, player.Data["IsReady"].Value == "True");
                }
            }
        }
    }

    private async void Exit()
    {
        try
        {
            await lobbyManager.LeaveLobby(AuthenticationService.Instance.PlayerId);
            isInRoom = false;
            ShowLobbyAndHideRoom();
        }
        catch (System.Exception)
        {
            Debug.Log("Failed to leave lobby");
        }
    }

    public void CreatePlayerItem(Player player)
    {
        var playerItem = roomItemTemplate.CloneTree();
        var teamName = LobbyManager.Instance.HasPlayerDataValue("Team", player) ? player.Data["Team"].Value : "None";

        playerItem.Q<Label>("PlayerName").text = player.Id;
        playerItem.Q<Label>("PlayerType").text = lobbyManager.IsHostByPlayerId(player.Id) ? "Host" : "Member";
        playerItem.Q<Label>("PlayerTeam").text = $"Team: {teamName}";
        playerItem.Q<Label>("PlayerTeam").style.color = teamName == "Blue" ? Color.blue : Color.red;

        if (lobbyManager.IsHost() && !lobbyManager.IsHostByPlayerId(player.Id))
        {
            playerItem.Q<Button>("Kick").style.display = DisplayStyle.Flex;
            playerItem.Q<Button>("Kick").clicked += async () => await KickPlayer(player.Id);
        }
        else
        {
            playerItem.Q<Button>("Kick").style.display = DisplayStyle.None;
        }

        playerList.Add(playerItem);
        playerItems.Add(player.Id, playerItem);
    }

    private async Task KickPlayer(string playerId)
    {
        try
        {
            await lobbyManager.KickPlayer(playerId);
        }
        catch (System.Exception)
        {
            Debug.Log("Failed to kick player");
        }
    }

    private void HideLobbyAndShowRoom()
    {
        lobby.style.display = DisplayStyle.None;
        room.style.display = DisplayStyle.Flex;

        lobbyGameSetup = FindAnyObjectByType<LobbyGameSetup>();
        lobbyGameSetup.OnMapSelected += OnMapSelected;
        lobbyGameSetup.Initialize();
    }

    private async void OnMapSelected(MapSo map)
    {
        if (lobbyManager.CurrentLobby == null || !lobbyManager.IsHost()) return;
        await lobbyManager.lobbyData.SetMapName(map.MapName, lobbyManager.CurrentLobby.Id);
    }

    private void ShowLobbyAndHideRoom()
    {
        lobby.style.display = DisplayStyle.Flex;
        room.style.display = DisplayStyle.None;

        lobbyGameSetup.OnMapSelected -= OnMapSelected;
    }

    private void CreatePlayerItems(List<Player> players)
    {
        playerList.Clear();
        playerItems.Clear();
        foreach (var player in players)
        {
            CreatePlayerItem(player);
        }
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
        CreatePlayerItems(lobbyManager.CurrentLobby.Players);
        ReadyUi();
        CheckIfAllPlayersReady();
        lobbyGameSetup.Update();
        UpdatePlayersLobbyData();

        await JoinGameScene();
    }

    private void UpdatePlayersLobbyData()
    {
        foreach (var player in lobbyManager.CurrentLobby.Players)
        {
            LobbyManager.Instance.lobbyPlayers.Add(new PlayerLobbyData(player));
        }
    }

    private async Task JoinGameScene()
    {
        if (lobbyManager.HasLobbyDataValue("RelayCode") && !isGameStarted)
        {
            var map = lobbyManager.CurrentLobby.Data["MapName"].Value;

            if (lobbyManager.CurrentLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                await SceneManager.LoadSceneAsync(map);
            }
            else
            {
                await lobbyManager.JoinRelayServer(lobbyManager.CurrentLobby.Data["RelayCode"].Value);
                await SceneManager.LoadSceneAsync(map);
            }

            lobbyGameSetup.OnMapSelected -= OnMapSelected;
            lobby.style.display = DisplayStyle.None;
            room.style.display = DisplayStyle.None;
            isGameStarted = true;
        }
    }

    public void JoinRoom(Lobby lobby)
    {
        HideLobbyAndShowRoom();
        isInRoom = true;
        CreatePlayerItems(lobby.Players);
    }

    private async void StartGame()
    {
        // try
        // {
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
