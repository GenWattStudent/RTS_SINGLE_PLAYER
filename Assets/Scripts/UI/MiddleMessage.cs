using UnityEngine.UIElements;

public class MiddleMessage : ToolkitHelper
{
    private Label timeToSpawnEnemyText;
    private VisualElement timerPanel;
    public static MiddleMessage Instance;

    public void ShowTimerPanel() {
        timerPanel.style.display = DisplayStyle.Flex;
    }

    public void HideTimerPanel() {
        timerPanel.style.display = DisplayStyle.None;
    }

    public void SetText(string message) {
        timeToSpawnEnemyText.text = message;
    }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        timeToSpawnEnemyText = GetLabel("TimeToSpawnEnemyText");
        timerPanel = GetVisualElement("TimerPanel");
    }
}
