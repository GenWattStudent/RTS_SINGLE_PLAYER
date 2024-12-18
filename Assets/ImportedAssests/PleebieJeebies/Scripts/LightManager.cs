﻿using System;
using System.Collections.Generic;
using UnityEngine;

// Make this change in editor
// [ExecuteInEditMode]
public class LightManager : MonoBehaviour
{
    [SerializeField, Header("Managed Objects")] private Light DirectionalLight = null;
    [SerializeField] private LightPreset DayNightPreset, LampPreset;
    private List<Light> SpotLights = new List<Light>();

    [SerializeField, Range(0, 1440), Header("Modifiers"), Tooltip("The game's current time of day")] private float TimeOfDay;
    // [SerializeField, Tooltip("Angle to rotate the sun")] private float SunDirection = 170f;
    [SerializeField, Tooltip("How fast time will go")] private float TimeMultiplier = 1;
    [SerializeField] private bool ControlLights = true;
    public float UpdateInterval = 0.5f;
    private float updateTimer = 0;

    private const float inverseDayLength = 1f / 1440f;
    public float SunDirection = 170f;

    public static LightManager Instance { get; private set; }
    public static bool IsNight => Instance.TimeOfDay > 1200 || Instance.TimeOfDay < 450;

    /// <summary>
    /// On project start, if controlLights is true, collect all non-directional lights in the current scene and place in a list
    /// </summary>
    private void Awake()
    {
        Instance = this;
        updateTimer = UpdateInterval;
        if (ControlLights)
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light li in lights)
            {
                switch (li.type)
                {
                    case LightType.Disc:
                    case LightType.Point:
                    case LightType.Rectangle:
                    case LightType.Spot:
                        SpotLights.Add(li);
                        break;
                    case LightType.Directional:
                    default:
                        break;
                }
            }
        }
    }

    public TimeSpan GetTimeOfDay()
    {
        return TimeSpan.FromMinutes(TimeOfDay);
    }

    /// <summary>
    /// This method will not run if there is no preset set
    /// On each frame, this will calculate the current time of day factoring game time and the time multiplier (1440 is how many minutes exist in a day 24 x 60)
    /// Then send a time percentage to UpdateLighting, to evaluate according to the set preset, what that time of day should look like
    /// </summary>
    private void Update()
    {
        TimeOfDay = TimeOfDay + (Time.deltaTime * TimeMultiplier);
        TimeOfDay = TimeOfDay % 1440;
        updateTimer -= Time.deltaTime;

        if (updateTimer <= 0)
        {
            updateTimer = UpdateInterval;
            UpdateLighting(TimeOfDay * inverseDayLength);
        }
    }

    /// <summary>
    /// Based on the time percentage recieved, set the current scene's render settings and light coloring to the preset
    /// In addition, rotate the directional light (the sun) according to the current time
    /// </summary>
    /// <param name="timePercent"></param>
    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = DayNightPreset.AmbientColour.Evaluate(timePercent);

        //Set the directional light (the sun) according to the time percent

        if (DirectionalLight.enabled == true)
        {
            DirectionalLight.color = DayNightPreset.DirectionalColour.Evaluate(timePercent);
            // float rotationAngle = timePercent * 175f;



            // Set the rotation of the DirectionalLight
            // DirectionalLight.transform.rotation = Quaternion.Euler(rotationAngle, SunDirection, 0);
            // DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, SunDirection, 0));
        }
    }
}