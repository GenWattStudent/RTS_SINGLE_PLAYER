using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameResult : ToolkitHelper
{
    public static GameResult Instance;
    private Label resultText;
    private Button goToMainMenuButton;
    private VisualElement resultModal;

    private void GoToMainMenu(ClickEvent ev) {
        SceneManager.LoadScene(0);
    }

    void Start() {
        Instance = this;
        resultText = GetLabel("GameResult");
        goToMainMenuButton = GetButton("MainMenuButtonResult");
        resultModal = GetVisualElement("ResultModal");
    }

    public  void Victory() {
        // change text to "Victory
        resultModal.style.display = DisplayStyle.Flex;
        resultText.text = "You win!";
        goToMainMenuButton.RegisterCallback<ClickEvent>(GoToMainMenu);
    }

    public  void Defeat() {
        // change text to "Defeat"
        resultModal.style.display = DisplayStyle.Flex;
        resultText.text = "You lose!";
        goToMainMenuButton.RegisterCallback<ClickEvent>(GoToMainMenu);
    }
}
