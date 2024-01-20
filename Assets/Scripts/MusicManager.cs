using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : Singleton<MusicManager>
{
    [SerializeField] private AudioSource globalMusic;
    // Start is called before the first frame update
    void Start()
    {
        globalMusic.loop = true;
        globalMusic.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
