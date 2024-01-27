using UnityEngine;
using UnityEngine.UIElements;

public class VolumeManager : MonoBehaviour
{
    private UIDocument uIDocument;
    private VisualElement root;
    private Slider globalMusicSlider;
    private Slider effectsSlider;

    private void OnEnable()
    {
        uIDocument = GetComponent<UIDocument>();
        root = uIDocument.rootVisualElement;
        globalMusicSlider = root.Q<Slider>("GlobalMusicSlider");
        effectsSlider = root.Q<Slider>("EffectMusicSlider");

        globalMusicSlider.value = PlayerPrefs.GetFloat("GlobalMusicVolume", 0.5f);
        effectsSlider.value = PlayerPrefs.GetFloat("EffectsVolume", 1);

        globalMusicSlider.RegisterCallback<ChangeEvent<float>>(HandleGlobalMusicVolumeChange);
        effectsSlider.RegisterCallback<ChangeEvent<float>>(HandleEffectsVolumeChange);
        globalMusicSlider.label = $"Global Music ({globalMusicSlider.value * 100:0}%)";
        effectsSlider.label = $"Effects Music ({effectsSlider.value * 100:0}%)";
    }

    private void HandleGlobalMusicVolumeChange(ChangeEvent<float> evt)
    {
        PlayerPrefs.SetFloat("GlobalMusicVolume", evt.newValue);
        MusicManager.Instance.SetGlobalMusicVolume(evt.newValue);
        globalMusicSlider.label = $"Global Music ({evt.newValue * 100:0}%)";
    }

    private void HandleEffectsVolumeChange(ChangeEvent<float> evt)
    {
        PlayerPrefs.SetFloat("EffectsVolume", evt.newValue);
        MusicManager.Instance.SetEffectVolume(evt.newValue);
        effectsSlider.label = $"Effects Music ({evt.newValue * 100:0}%)";
    }
}
