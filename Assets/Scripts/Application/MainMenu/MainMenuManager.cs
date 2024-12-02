using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private int minUsernameLength = 3;
    [SerializeField] private int maxUsernameLength = 10;
    [SerializeField] private float fpsInterval = 0.5f;

    private UIDocument uIDocument;
    private VisualElement root;
    private VisualElement userModal;
    private Button submitUsernameButton;
    private Button multiplayerButton;
    private Button settingsButton;
    private TextField usernameInput;
    private Label usernameLabel;
    private Label errorLabel;
    private VisualElement settingsBox;
    private Button closeSettingsButton;
    private Label fps;
    private Button quitButton;
    private float fpsTimer;

    private void OnEnable()
    {
        uIDocument = GetComponent<UIDocument>();
        root = uIDocument.rootVisualElement;
        userModal = root.Q<VisualElement>("UserModalBox");
        submitUsernameButton = root.Q<Button>("AcceptUsername");
        multiplayerButton = root.Q<Button>("Multiplayer");
        settingsButton = root.Q<Button>("SettingsButton");
        usernameInput = root.Q<TextField>("UserInput");
        usernameLabel = root.Q<Label>("Username");
        errorLabel = root.Q<Label>("ErrorLabel");
        settingsBox = root.Q<VisualElement>("SettingsBox");
        fps = root.Q<Label>("FPS");
        closeSettingsButton = root.Q<Button>("CloseSettings");
        quitButton = root.Q<Button>("QuitButton");

        PlayerPrefs.DeleteKey("username");

        if (!PlayerPrefs.HasKey("username"))
        {
            PlayerPrefs.SetString("username", $"Player{Mathf.Round(Random.Range(0, 1000))}");
        }

        usernameInput.value = PlayerPrefs.GetString("username");
        Debug.Log(PlayerPrefs.GetString("username"));
        userModal.style.display = DisplayStyle.None;

        settingsButton.clicked += ShowSettings;
        usernameLabel.RegisterCallback<ClickEvent>(HandleShowUserModal);
        multiplayerButton.clicked += StartMultiplayerGame;
        submitUsernameButton.clicked += SubmitUsername;
        closeSettingsButton.clicked += CloseSettings;
        quitButton.clicked += Application.Quit;
    }

    private void HandleShowUserModal(ClickEvent evt)
    {
        userModal.style.display = DisplayStyle.Flex;
    }

    private void StartMultiplayerGame()
    {
        SceneManager.LoadScene("Lobby");
    }

    private void CloseSettings()
    {
        settingsBox.style.display = DisplayStyle.None;
    }

    private void ShowSettings()
    {
        settingsBox.style.display = DisplayStyle.Flex;
    }

    private void OnDisable()
    {
        submitUsernameButton.clicked -= SubmitUsername;
        settingsButton.clicked -= ShowSettings;
        closeSettingsButton.clicked -= CloseSettings;
        usernameLabel.UnregisterCallback<ClickEvent>(HandleShowUserModal);
        quitButton.clicked -= Application.Quit;
    }

    private void SubmitUsername()
    {
        if (usernameInput.text.Length < minUsernameLength)
        {
            errorLabel.style.display = DisplayStyle.Flex;
            errorLabel.text = $"Username must be at least {minUsernameLength} characters long";
            return;
        };

        if (usernameInput.text.Length > maxUsernameLength)
        {
            errorLabel.style.display = DisplayStyle.Flex;
            errorLabel.text = $"Username must be less than {maxUsernameLength} characters long";
            return;
        };

        var username = usernameInput.text;
        PlayerPrefs.SetString("username", username);
        usernameLabel.text = username;
        userModal.style.display = DisplayStyle.None;
    }

    void Start()
    {
        var username = PlayerPrefs.GetString("username", "");
        usernameLabel.text = username;

        if (username == "")
        {
            userModal.style.display = DisplayStyle.Flex;
        }
    }

    void Update()
    {
        fpsTimer += Time.deltaTime;

        if (fpsTimer >= fpsInterval)
        {
            var roundFps = Mathf.Round(1 / Time.deltaTime);
            fps.text = $"{roundFps}FPS";
            fpsTimer = 0;
        }
    }
}