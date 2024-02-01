using Unity.Netcode;
using UnityEngine.UIElements;

public class MultiplayerTools : NetworkBehaviour
{
    private Button startHostButton;
    private Button startClientButton;
    private VisualElement multiplayerModal;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        startHostButton = root.Q<Button>("StartHost");
        startClientButton = root.Q<Button>("StartClient");
        multiplayerModal = root.Q<VisualElement>("MultiplayerTools");

        startHostButton.clicked += StartHost;
        startClientButton.clicked += StartClient;
    }

    private void OnDisable()
    {
        startHostButton.clicked -= StartHost;
        startClientButton.clicked -= StartClient;
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        multiplayerModal.style.display = DisplayStyle.None;
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        multiplayerModal.style.display = DisplayStyle.None;
    }
}
