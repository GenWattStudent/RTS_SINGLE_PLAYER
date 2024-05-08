using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyUi : ToolkitHelper
{
    [SerializeField] private VisualTreeAsset lobbyItemTemplate;

    private Button createLobbyButton;
    private VisualElement lobbiesContainer;
    private TextField lobbyName;
    private Label errorLabel;

    private void Start()
    {
        createLobbyButton = GetButton("CreateLobby");
        lobbiesContainer = GetVisualElement("LobbiesContainer");
        lobbyName = root.Q<TextField>("LobbyName");
        errorLabel = root.Q<Label>("ErrorLabel");

        HideError();
        createLobbyButton.clicked += CreateLobby;
        CreateLobbiesUI();
        StartCoroutine(RefreshLobbies());
    }

    private IEnumerator RefreshLobbies()
    {
        while (true)
        {
            if (!LobbyManager.Instance.isSingInCompleted) yield return new WaitForSeconds(1);
            CreateLobbiesUI();
            yield return new WaitForSeconds(3);
        }
    }

    private async void CreateLobby()
    {
        var lobbyName = this.lobbyName.text;

        try
        {
            await LobbyManager.Instance.CreateLobby(lobbyName, LobbyManager.Instance.maxPlayers);
        }
        catch (System.Exception)
        {
            ShowError("Failed to create lobby");
        }
    }

    private void CreateLobbyItem(Lobby lobby)
    {
        var lobbyItem = lobbyItemTemplate.CloneTree();

        lobbyItem.Q<Label>("LobbyName").text = lobby.Name;
        lobbyItem.Q<Label>("LobbyId").text = lobby.Id;
        lobbyItem.Q<Label>("LobbyMaxPlayers").text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        lobbyItem.AddManipulator(new Clickable(() => Debug.Log("Clicked on lobby: " + lobby.Id)));

        lobbiesContainer.Add(lobbyItem);
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
            var lobbies = await LobbyManager.Instance.GetAll();
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
