using UnityEngine;
using UnityEngine.UIElements;

public class RoomUi : ToolkitHelper
{
    [SerializeField] private VisualTreeAsset roomItemTemplate;
    private Button readyButton;
    private VisualElement playerList;
    private Button startGameButton;
    private Button exitButton;

    void Start()
    {
        readyButton = GetButton("ReadyButton");
        playerList = GetVisualElement("PlayerList");
        startGameButton = GetButton("StartGame");
        exitButton = GetButton("Exit");
        // HideStartGameButton();
        readyButton.clicked += Ready;
        // StartCoroutine(RefreshPlayers());
    }

    private void Ready()
    {
        Debug.Log("Ready");
    }

    public void CreatePlayerItem(string playerName)
    {
        var playerItem = roomItemTemplate.CloneTree();
        playerItem.Q<Label>("PlayerName").text = playerName;
        playerList.Add(playerItem);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
