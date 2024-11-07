using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class GameTime : NetworkToolkitHelper
{
    [SerializeField] private Texture2D dayIconTexture;
    [SerializeField] private Texture2D nightIconTexture;
    [SerializeField] private float updateTimer = .7f;

    public NetworkVariable<float> timeOfDay = new(0);
    public NetworkVariable<bool> isNight = new(false);

    private Label timeText;
    private VisualElement dayIcon;
    private float timer;

    void Start()
    {
        UIDocument = GetComponent<UIDocument>();
        root = UIDocument.rootVisualElement;
        timeText = root.Q<Label>("ClockLabel");
        dayIcon = root.Q<VisualElement>("DayNightIcon");

        timeOfDay.OnValueChanged += OnTimeOfDayChanged;
        isNight.OnValueChanged += OnIsNightChanged;
    }

    private void OnTimeOfDayChanged(float oldValue, float newValue)
    {
        TimeSpan time = TimeSpan.FromSeconds(newValue);
        timeText.text = time.ToString("hh':'mm");
    }

    private void OnIsNightChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            dayIcon.style.backgroundImage = nightIconTexture;
        }
        else
        {
            dayIcon.style.backgroundImage = dayIconTexture;
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        timer -= Time.fixedDeltaTime;

        if (timer > 0) return;
        TimeSpan time = LightManager.Instance.GetTimeOfDay();
        // convert to float seconds
        timeOfDay.Value = (float)time.TotalSeconds;
        timer = updateTimer;

        if (LightManager.IsNight)
        {
            isNight.Value = true;
        }
        else
        {
            isNight.Value = false;
        }
    }
}
