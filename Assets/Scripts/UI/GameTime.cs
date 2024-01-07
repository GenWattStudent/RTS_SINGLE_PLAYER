using UnityEngine;
using UnityEngine.UIElements;

public class GameTime : MonoBehaviour
{
    private UIDocument UIDocument;
    private Label timeText;
    private VisualElement root;

    void Start()
    {
        UIDocument = GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;
        timeText = root.Q<Label>("ClockLabel");
    }

    void FixedUpdate()
    {
        timeText.text = LightManager.Instance.GetTimeOfDay().ToString("hh\\:mm");
    }
}
