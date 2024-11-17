using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Settings : NetworkToolkitHelper
{
    private Button settingsButton;
    private Button mainMenuButton;
    private Button backButton;
    private Button soundButton;
    private Button closeSettingsButton;
    private VisualElement settingsContainer;
    private VisualElement settingsBox;
    public bool isSettingsOpen = false;

    void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        settingsButton = GetButton("SettingsButton");
        mainMenuButton = GetButton("MainMenuButton");
        backButton = GetButton("BackButton");
        settingsContainer = GetVisualElement("SettingsModal");
        soundButton = GetButton("SoundsSettings");
        settingsBox = GetVisualElement("SettingsBox");
        closeSettingsButton = GetButton("CloseSettings");

        settingsButton.RegisterCallback<ClickEvent>(OnSettingsButtonClick);
        mainMenuButton.RegisterCallback<ClickEvent>(OnMainMenuButtonClick);
        backButton.RegisterCallback<ClickEvent>(OnBackButtonClick);
        soundButton.RegisterCallback<ClickEvent>(OnSoundButtonClick);
        closeSettingsButton.RegisterCallback<ClickEvent>(OnCloseSettingsButtonClick);
    }

    private void OnSettingsButtonClick(ClickEvent ev)
    {
        OpenSettings();
    }

    private void OnMainMenuButtonClick(ClickEvent ev)
    {
        LoadMainMenu();
    }

    private void OnSoundButtonClick(ClickEvent ev)
    {
        settingsBox.style.display = DisplayStyle.Flex;
    }

    private void OnCloseSettingsButtonClick(ClickEvent ev)
    {
        settingsBox.style.display = DisplayStyle.None;
    }

    private void OpenSettings()
    {
        settingsContainer.style.display = DisplayStyle.Flex;
        isSettingsOpen = true;
    }

    private void OnBackButtonClick(ClickEvent ev)
    {
        CloseSettings();
    }

    private void CloseSettings()
    {
        settingsContainer.style.display = DisplayStyle.None;
        isSettingsOpen = false;
    }

    private void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isSettingsOpen)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }
    }
}
