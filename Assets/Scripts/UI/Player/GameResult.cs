using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameResult : NetworkToolkitHelper
{
    [SerializeField] private GameObject localUIObject;

    private UIDocument localUIDocument;
    private Label resultText;
    private Button goToMainMenuButton;
    private VisualElement resultModal;
    private bool isResultSet = false;

    private void GoToMainMenu(ClickEvent ev)
    {
        SceneManager.LoadScene(0);
    }

    private void Awake()
    {
        if (localUIDocument == null)
        {
            // instantiate the prefab
            localUIDocument = Instantiate(localUIObject).GetComponent<UIDocument>();
        }
    }

    private void Start()
    {
        var localRoot = localUIDocument.rootVisualElement;

        resultText = localRoot.Q<Label>("GameResult");
        goToMainMenuButton = localRoot.Q<Button>("MainMenuButtonResult");
        resultModal = localRoot.Q<VisualElement>("ResultModal");
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
