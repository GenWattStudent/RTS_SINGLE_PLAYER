using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameResult : NetworkToolkitHelper
{
    private Label resultText;
    private Button goToMainMenuButton;
    private VisualElement resultModal;
    private bool isResultSet = false;

    private void GoToMainMenu(ClickEvent ev)
    {
        SceneManager.LoadScene(0);
    }

    void Start()
    {
        resultText = GetLabel("GameResult");
        goToMainMenuButton = GetButton("MainMenuButtonResult");
        resultModal = GetVisualElement("ResultModal");
    }

    public void Victory()
    {
        // change text to "Victory
        if (isResultSet && !IsOwner) return;
        resultModal.style.display = DisplayStyle.Flex;
        resultText.text = "You win!";
        goToMainMenuButton.RegisterCallback<ClickEvent>(GoToMainMenu);
        isResultSet = true;
    }

    public void Defeat()
    {
        // change text to "Defeat"
        if (isResultSet && !IsOwner) return;
        resultModal.style.display = DisplayStyle.Flex;
        resultText.text = "You lose!";
        goToMainMenuButton.RegisterCallback<ClickEvent>(GoToMainMenu);
        isResultSet = true;
    }
}
