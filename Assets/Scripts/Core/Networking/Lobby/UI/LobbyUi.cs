using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LobbyUi : ToolkitHelper
{
    [SerializeField] private VisualTreeAsset lobbyItemTemplate;

    private Button createLobbyButton;
    private VisualElement lobbiesContainer;
    private TextField lobbyName;
    private Label errorLabel;
    private LobbyManager lobbyManager;
    private Button closeLobbyButton;
    private RoomUi roomUi;
    private VisualElement _lobbyContainer;

    protected override void OnEnable()
    {
        base.OnEnable();
        lobbyManager = FindAnyObjectByType<LobbyManager>();
        roomUi = FindAnyObjectByType<RoomUi>();

        _lobbyContainer = GetVisualElement("LobbyBox");
        createLobbyButton = GetButton("CreateLobby");
        lobbiesContainer = GetVisualElement("LobbiesContainer");
        lobbyName = root.Q<TextField>("LobbyName");
        errorLabel = root.Q<Label>("ErrorLabel");
        closeLobbyButton = GetButton("CloseLobby");

        HideError();
        createLobbyButton.clicked += CreateLobby;
        closeLobbyButton.clicked += async () => await CloseLobby();

        // currentPlayerName.text = LobbyManager.Instance.playerName;

        Debug.Log("LobbyUi started");
        _lobbyContainer.style.display = DisplayStyle.Flex;
        lobbiesContainer.Clear();
        Debug.Log("LobbyUi started 2" + _lobbyContainer.style.display);
        // CreateLobbiesUI();
        StartCoroutine(RefreshLobbies());
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        createLobbyButton.clicked -= CreateLobby;
        // closeLobbyButton.clicked -= CloseLobby;
    }

    private IEnumerator RefreshLobbies()
    {
        while (!roomUi.isInRoom)
        {
            Debug.Log("Refesh" + _lobbyContainer.style.display);
            _lobbyContainer.style.display = DisplayStyle.Flex;
            if (!lobbyManager.isSingInCompleted) yield return new WaitForSeconds(1);
            CreateLobbiesUI();
            yield return new WaitForSeconds(3);
        }
    }

    private async Task CloseLobby()
    {
        await roomUi.HideLobbyAndRoom();

        // Destroy(gameObject);

        SceneManager.LoadScene("MainMenu");
    }

    private async void CreateLobby()
    {
        var lobbyName = this.lobbyName.text;

        // try
        // {
        await lobbyManager.CreateLobby(lobbyName, LobbyManager.Instance.maxPlayers);
        // }
        // catch (System.Exception)
        // {
        //     ShowError("Failed to create lobby");
        // }
    }

    private void CreateLobbyItem(Lobby lobby)
    {
        var lobbyItem = lobbyItemTemplate.CloneTree();

        lobbyItem.Q<Label>("LobbyName").text = lobby.Name;
        lobbyItem.Q<Label>("LobbyId").text = lobby.Id;
        lobbyItem.Q<Label>("LobbyMaxPlayers").text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        lobbyItem.Q<Button>("LobbyItem").clicked += async () => await JoinLobby(lobby.Id);
        Debug.Log("Lobby item created " + lobby.MaxPlayers);
        lobbiesContainer.Add(lobbyItem);
    }

    private async Task JoinLobby(string lobbyId)
    {
        // try
        // {
        await lobbyManager.JoinLobby(lobbyId);
        // }
        // catch (System.Exception e)
        // {
        //     Debug.LogError("Failed to join lobby " + e.Message);
        //     ShowError($"Failed to join lobby {e.Message}");
        // }
    }

    private void ShowError(string message)
    {
        errorLabel.style.display = DisplayStyle.Flex;
        errorLabel.text = message;
    }

    private void HideError()
    {
        errorLabel.style.display = DisplayStyle.None;
    }

    private async void CreateLobbiesUI()
    {
        try
        {
            var lobbies = await lobbyManager.GetAll();
            var avaliableLobbies = lobbies.FindAll(lobby => lobby.Players.Count < lobby.MaxPlayers);

            lobbiesContainer.Clear();
            foreach (var lobby in avaliableLobbies)
            {
                CreateLobbyItem(lobby);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            ShowError("Failed to get lobbies");
        }
    }
}
