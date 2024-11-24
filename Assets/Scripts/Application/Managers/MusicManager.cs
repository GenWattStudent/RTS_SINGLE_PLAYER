using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MusicData
{
    public AudioClip clip;
    public MusicManager.MusicType type;
}

public class MusicManager : Singleton<MusicManager>
{
    public enum MusicType
    {
        Menu,
        Game,
        BuildingExplosion,
        UnitExplosion,
        UnitTankAttack,
        Laser,
        HeroAttack,
        ProjectileExplosion,
    }

    [SerializeField] private AudioSource globalMusic;
    [SerializeField] private List<MusicData> effects = new();
    [SerializeField] private int maxOneKindAudioSources = 7;
    private float effectsVolume = 1;
    private Dictionary<MusicType, AudioSource> audioSources = new();

    private void Start()
    {
        effectsVolume = PlayerPrefs.GetFloat("EffectsVolume", 1);
        if (globalMusic == null) return;
        globalMusic.loop = true;
        globalMusic.volume = PlayerPrefs.GetFloat("GlobalMusicVolume", 0.5f);
        globalMusic.Play();
    }

    public void SetGlobalMusicVolume(float volume)
    {
        if (globalMusic == null) return;
        globalMusic.volume = volume;
    }

    public void SetEffectVolume(float volume)
    {
        effectsVolume = volume;
    }

    public void PlayMusic(MusicType type, Vector3 position)
    {
        if (effectsVolume == 0) return;

        GetMusicData(type, out var clip);

        if (clip != null)
        {
            var audioGameObject = new GameObject($"Audio {type}");
            audioGameObject.transform.position = position;
            var audioSource = audioGameObject.AddComponent<AudioSource>();

            audioSource.clip = clip;
            audioSource.volume = effectsVolume;
            audioSource.Play();
            Destroy(audioGameObject, clip.length);
        }
    }

    private void GetMusicData(MusicType type, out AudioClip clip)
    {
        clip = null;

        foreach (var effect in effects)
        {
            if (effect.type == type)
            {
                clip = effect.clip;
                break;
            }
        }
    }
}
