using System.Collections;
using TMPro;
using UnityEngine;

public enum PopupType {
    Info,
    Error,
    Standard
}

public class Popup : MonoBehaviour
{
    [SerializeField] private PopupType popupType = PopupType.Standard;
    [SerializeField] private TextMeshProUGUI popupText;

    public void Show(string message, float duration = 2f) {
        if (duration > 0) {
            StartCoroutine(ShowPopup(message, duration));
            return;
        } 

        ShowPopup(message);
    }

    public void SetColor() {
        switch (popupType) {
            case PopupType.Info:
                popupText.color = Color.blue;
                break;
            case PopupType.Error:
                popupText.color = Color.red;
                break;
            case PopupType.Standard:
                popupText.color = Color.white;
                break;
        }
    }

    public void ShowPopup(string message) {
        popupText.text = message;
        SetColor();
    }

    private IEnumerator ShowPopup(string message, float duration) {
        popupText.text = message;
        SetColor();
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}
