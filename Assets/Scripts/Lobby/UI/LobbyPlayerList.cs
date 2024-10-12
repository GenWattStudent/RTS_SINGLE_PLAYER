using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
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

    public void CreatePlayerItems(NetworkList<PlayerNetcodeLobbyData> players)
    {
        playerList.Clear();
        playerItems.Clear();

        foreach (var player in players)
        {
            CreatePlayerItem(player);
        }
    }

    public void CreatePlayerItem(PlayerNetcodeLobbyData player)
    {
        var playerItem = roomItemTemplate.CloneTree();
        var teamName = player.Team.ToString();
        var playerName = player.PlayerName.ToString();

        playerItem.Q<Label>("PlayerName").text = playerName;
        playerItem.Q<Label>("PlayerType").text = lobbyManager.IsHostByPlayerId(player.LobbyPlayerId.ToString()) ? "Host" : "Member";
        playerItem.Q<Label>("PlayerTeam").text = $"Team: {teamName}";
        playerItem.Q<Label>("PlayerTeam").style.color = teamName == "Blue" ? Color.blue : Color.red;
        playerItem.Q<VisualElement>("PlayerColor").style.backgroundColor = player.playerColor;

        if (lobbyManager.IsHost() && !lobbyManager.IsHostByPlayerId(player.LobbyPlayerId.ToString()))
        {
            playerItem.Q<Button>("Kick").style.display = DisplayStyle.Flex;
            playerItem.Q<Button>("Kick").clicked += async () => await KickPlayer(player.LobbyPlayerId.ToString(), player.NetcodePlayerId);
        }
        else
        {
            playerItem.Q<Button>("Kick").style.display = DisplayStyle.None;
        }

        playerList.Add(playerItem);
        playerItems.Add(player.LobbyPlayerId.ToString(), playerItem);
    }

    public void CheckPlayerReadyStatus(NetworkList<PlayerNetcodeLobbyData> players)
    {
        foreach (var playerItem in playerItems)
        {
            foreach (var player in players)
            {
                if (playerItem.Key == player.LobbyPlayerId.ToString())
                {
                    ChangePlayerItemStatus(playerItem.Value, player.IsReady);
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

    private async Task KickPlayer(string playerId, ulong netcodePlayerId)
    {
        try
        {
            Debug.Log($"Kicking player {playerId}");
            LobbyRoomService.Instance.KickPlayer(netcodePlayerId);
            await lobbyManager.KickPlayer(playerId);
        }
        catch (System.Exception)
        {
            Debug.Log("Failed to kick player");
        }
    }
}
