using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class SceneLoader : ToolkitHelper
{
    [SerializeField] private VisualTreeAsset loaderPlayerTemplate;
    private VisualElement sceneLoading;
    private VisualElement playerList;
    private Dictionary<ulong, VisualElement> playerItems = new();

    protected override void OnEnable()
    {
        base.OnEnable();

        sceneLoading = GetVisualElement("SceneLoading");
        playerList = GetVisualElement("LoaderPlayerBox");
    }

    private void OnDisable()
    {
        sceneLoading.style.display = DisplayStyle.None;
    }

    public void ShowSceneLoading(NetworkList<PlayerNetcodeLobbyData> players)
    {
        sceneLoading.style.display = DisplayStyle.Flex;
        CreatePlayerList(players);
    }

    public void HideSceneLoading()
    {
        sceneLoading.style.display = DisplayStyle.None;
    }

    public void CreatePlayerList(NetworkList<PlayerNetcodeLobbyData> players)
    {
        playerList.Clear();
        playerItems.Clear();

        foreach (var player in players)
        {
            CreatePlayerItem(player);
        }
    }

    private void CreatePlayerItem(PlayerNetcodeLobbyData player)
    {
        var playerItem = loaderPlayerTemplate.CloneTree();

        playerItem.Q<Label>("PlayerName").text = player.PlayerName.ToString();
        playerItem.Q<ProgressBar>("LoadingProgressbar").title = $"Loading({player.Progress}%)";
        playerItem.Q<ProgressBar>("LoadingProgressbar").value = player.Progress;

        playerList.Add(playerItem);
        playerItems.Add(player.NetcodePlayerId, playerItem);
    }
}
