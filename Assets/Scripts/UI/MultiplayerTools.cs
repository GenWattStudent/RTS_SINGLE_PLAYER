using Unity.Netcode;
using UnityEngine.UIElements;

public class MultiplayerTools : NetworkBehaviour
{
    private Button startHostButton;
    private Button startClientButton;
    private Label playerCountLabel;
    private VisualElement multiplayerModal;
    private bool isHostRunning = false;
    private bool isClientRunning = false;
    public int playerCount = 0;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        startHostButton = root.Q<Button>("StartHost");
        startClientButton = root.Q<Button>("StartClient");
        multiplayerModal = root.Q<VisualElement>("MultiplayerTools");
        playerCountLabel = root.Q<Label>("PlayerCount");

        startHostButton.clicked += ToogleHost;
        startClientButton.clicked += ToogleClient;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            playerCount++;
            playerCountLabel.text = $"Player count: {playerCount}";
        }
    }

    private void OnDisable()
    {
        startHostButton.clicked -= ToogleHost;
        startClientButton.clicked -= ToogleClient;
    }

    private void OnDestroy()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        isHostRunning = true;
        startHostButton.text = "Stop Host";
    }

    private void StopHost()
    {
        NetworkManager.Singleton.Shutdown();
        isHostRunning = false;
        startHostButton.text = "Start Host";
    }

    private void ToogleHost()
    {
        if (isHostRunning)
        {
            StopHost();
            return;
        }

        StartHost();
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        isClientRunning = true;
        startClientButton.text = "Stop Client";
    }

    private void StopClient()
    {
        NetworkManager.Singleton.Shutdown();
        isClientRunning = false;
        startClientButton.text = "Start Client";
    }

    private void ToogleClient()
    {
        if (isClientRunning)
        {
            StopClient();
            return;
        }

        StartClient();
    }
}
