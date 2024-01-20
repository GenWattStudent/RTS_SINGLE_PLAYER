using UnityEngine;
using UnityEngine.UIElements;

public class GameTime : MonoBehaviour
{
    private UIDocument UIDocument;
    private Label timeText;
    private VisualElement dayIcon;
    private VisualElement root;
    [SerializeField] private Texture2D dayIconTexture;
    [SerializeField] private Texture2D nightIconTexture;

    void Start()
    {
        UIDocument = GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;
        timeText = root.Q<Label>("ClockLabel");
        dayIcon = root.Q<VisualElement>("DayNightIcon");
    }

    void FixedUpdate()
    {
        if (LightManager.IsNight)
        {
            dayIcon.style.backgroundImage = nightIconTexture;
        }
        else
        {
            dayIcon.style.backgroundImage = dayIconTexture;
        }

        timeText.text = LightManager.Instance.GetTimeOfDay().ToString("hh\\:mm");
    }
}
