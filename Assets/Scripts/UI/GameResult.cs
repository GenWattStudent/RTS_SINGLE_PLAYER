using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameResult : Singleton<GameResult>
{
    [SerializeField] private GameObject resultPrefab;
    [SerializeField] private static Button button;
    [SerializeField] private static TextMeshProUGUI text;

    public  void Victory() {
        // change text to "Victory
        resultPrefab.gameObject.SetActive(true);
        text.text = "You win!";
        button.onClick.AddListener(() => SceneManager.LoadScene(0));
    }

    public  void Defeat() {
        // change text to "Defeat"
        resultPrefab.gameObject.SetActive(true);
        text.text = "You lose!";
        button.onClick.AddListener(() => SceneManager.LoadScene(0));
    }
}
