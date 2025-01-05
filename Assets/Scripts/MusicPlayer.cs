using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    private EventInstance _music;
    
    void Start()
    {
        _music = RuntimeManager.CreateInstance("event:/Ambience/Forest");
        _music.start();
    }

    private void OnDestroy()
    {
        _music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _music.release();
    }
}
