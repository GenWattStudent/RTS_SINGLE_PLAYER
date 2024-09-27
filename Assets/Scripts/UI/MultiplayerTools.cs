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
    public NetworkVariable<int> playerCount = new(0);

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        startHostButton = root.Q<Button>("StartHost");
        startClientButton = root.Q<Button>("StartClient");
        multiplayerModal = root.Q<VisualElement>("MultiplayerTools");
        playerCountLabel = root.Q<Label>("PlayerCount");

        if (GameManager.Instance.IsDebug)
        {
            multiplayerModal.style.display = DisplayStyle.Flex;
        }
        else
        {
            multiplayerModal.style.display = DisplayStyle.None;
            return;
        }

        startHostButton.clicked += ToogleHost;
        startClientButton.clicked += ToogleClient;
        playerCount.OnValueChanged += (oldValue, newValue) => playerCountLabel.text = $"Player count: {newValue}";
    }

    private void Start()
    {
        if (!GameManager.Instance.IsDebug) return;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        playerCount.Value += 1;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (!IsServer) return;
        playerCount.Value -= 1;
    }

    private void OnDisable()
    {
        if (!GameManager.Instance.IsDebug) return;
        startHostButton.clicked -= ToogleHost;
        startClientButton.clicked -= ToogleClient;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!GameManager.Instance.IsDebug) return;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
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
