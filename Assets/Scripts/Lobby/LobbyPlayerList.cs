using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyPlayerList : NetworkToolkitHelper
{
    [SerializeField] private VisualTreeAsset roomItemTemplate;

    private LobbyManager lobbyManager;
    private VisualElement playerList;
    private Dictionary<string, VisualElement> playerItems = new();

    protected override void OnEnable()
    {
        base.OnEnable();

        playerList = GetVisualElement("PlayerList");
    }

    private void Awake()
    {
        lobbyManager = FindAnyObjectByType<LobbyManager>();
    }

    public void CreatePlayerItems(List<Player> players)
    {
        playerList.Clear();
        playerItems.Clear();

        foreach (var player in players)
        {
            CreatePlayerItem(player);
        }
    }

    public void CreatePlayerItem(Player player)
    {
        var playerItem = roomItemTemplate.CloneTree();
        var teamName = LobbyManager.Instance.HasPlayerDataValue("Team", player) ? player.Data["Team"].Value : "None";
        var playerName = LobbyManager.Instance.HasPlayerDataValue("PlayerName", player) ? player.Data["PlayerName"].Value : player.Id;

        playerItem.Q<Label>("PlayerName").text = playerName;
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

    public void CheckPlayerReadyStatus()
    {
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

    private void ChangePlayerItemStatus(VisualElement playerItem, bool isReady)
    {
        playerItem.Q<Label>("Status").AddToClassList(isReady ? "bg-success" : "bg-danger");
        playerItem.Q<Label>("Status").RemoveFromClassList(isReady ? "bg-danger" : "bg-success");
        playerItem.Q<Label>("Status").text = isReady ? "Ready" : "Not Ready";
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
}
