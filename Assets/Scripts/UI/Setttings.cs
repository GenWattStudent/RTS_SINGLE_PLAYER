using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Settings : ToolkitHelper
{

    private Button settingsButton;
    private Button mainMenuButton;
    private Button backButton;
    private VisualElement settingsContainer;
    public bool isSettingsOpen = false;

    void Start()
    {
        settingsButton = GetButton("SettingsButton");
        mainMenuButton = GetButton("MainMenuButton");
        backButton = GetButton("BackButton");
        settingsContainer = GetVisualElement("SettingsModal");

        settingsButton.RegisterCallback<ClickEvent>(OnSettingsButtonClick);
        mainMenuButton.RegisterCallback<ClickEvent>(OnMainMenuButtonClick);
        backButton.RegisterCallback<ClickEvent>(OnBackButtonClick);
    }

    private void OnSettingsButtonClick(ClickEvent ev) {
        OpenSettings();
    }

    private void OnMainMenuButtonClick(ClickEvent ev) {
        LoadMainMenu();
    }

    private void OpenSettings() {
        settingsContainer.style.display = DisplayStyle.Flex;
        isSettingsOpen = true;
    }

    private void OnBackButtonClick(ClickEvent ev) {
        CloseSettings();
    }

    private void CloseSettings() {
        settingsContainer.style.display = DisplayStyle.None;
        isSettingsOpen = false;
    }

    private void LoadMainMenu() {
        SceneManager.LoadScene(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (isSettingsOpen) {
                CloseSettings();
            } else {
                OpenSettings();
            }
        }
    }
}
