using TMPro;
using UnityEngine;

public class GameTime : MonoBehaviour
{
    private TextMeshProUGUI timeText;

    void Start()
    {
        timeText = GetComponent<TextMeshProUGUI>();
    }

    void FixedUpdate()
    {
        timeText.text = LightManager.Instance.GetTimeOfDay().ToString("hh\\:mm");
    }
}
