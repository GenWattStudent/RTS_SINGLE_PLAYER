// using TMPro;
using UnityEngine;

public class ScreenController : MonoBehaviour
{
    // [SerializeField] private TextMeshProUGUI text;
    private ProgresBar progresBar;

    public void SetText(string text) {
        // this.text.text = text;
    }

    public void SetProgresBar(float value, float maxValue) {
        progresBar.UpdateProgresBar(value, maxValue);
    }

    void Awake()
    {
        progresBar = GetComponentInChildren<ProgresBar>();
    }
}
