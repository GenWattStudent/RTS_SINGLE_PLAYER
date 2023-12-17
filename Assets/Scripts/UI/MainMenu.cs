using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startGameButton;

    private void OnEnable()
    {
        startGameButton.onClick.AddListener(StartGame);
    }


    private void OnDisable()
    {
        startGameButton.onClick.RemoveListener(StartGame);
    }

    private void StartGame()
    {
        SceneManager.LoadScene("Game");
    }
}
