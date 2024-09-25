using UnityEngine;
using UnityEngine.UIElements;

public class FPSCounter : NetworkToolkitHelper
{
    public float lowFPS = 30f;
    public float highFPS = 60f;
    public float updateTimer = 0;
    public float updateInterval = 0.5f;

    private int currentFPS;
    private Label fpsLabel;

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        fpsLabel = GetLabel("FPSLabel");
    }

    private void UpdateFPS()
    {
        var playerController = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerController>();
        currentFPS = (int)(1f / Time.unscaledDeltaTime);
        // fpsLabel.text = $"{currentFPS}FPS";
        fpsLabel.text = $"Team {playerController.teamType.Value} {currentFPS}FPS {Mathf.Round(playerController.currentPing)}ms";

        if (currentFPS < lowFPS)
        {
            SetDanger(fpsLabel);
        }
        else if (currentFPS > highFPS)
        {
            SetSuccess(fpsLabel);
        }
    }

    private void Update()
    {
        updateTimer += Time.unscaledDeltaTime;

        if (updateTimer > updateInterval)
        {
            UpdateFPS();
            updateTimer = 0;
            return;
        }
    }
}
