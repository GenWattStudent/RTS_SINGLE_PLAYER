using UnityEngine;
using UnityEngine.UIElements;

public class FPSCounter : ToolkitHelper
{
    private int currentFPS;
    private Label fpsLabel;
    public float lowFPS = 30f;
    public float highFPS = 60f;
    public float updateTimer = 0;
    public float updateInterval = 0.5f;

    void Start()
    {
        fpsLabel = GetLabel("FPSLabel");
    }

    private void UpdateFPS() {
        currentFPS = (int)(1f / Time.unscaledDeltaTime);
        fpsLabel.text = $"{currentFPS}FPS";

        if (currentFPS < lowFPS) {
            SetDanger(fpsLabel);
        } else if (currentFPS > highFPS) {
            SetSuccess(fpsLabel);
        } 
    }

    void Update()
    {
        updateTimer += Time.unscaledDeltaTime;

        if (updateTimer > updateInterval) {
            UpdateFPS();
            updateTimer = 0;
            return;
        }
    }
}
