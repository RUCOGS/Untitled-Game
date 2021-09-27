using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SettingsData", menuName = "ScriptableObjects/Settings", order = 1)]
public class SettingsObject : ScriptableObject
{
    public AudioMixer mixer;

    public float masterVolume;
    public float musicVolume;
    public float effectsVolume;



    public static float ConvertToVolume(float i) {
        if(i == 0) {
            i = 0.001f;
        }

        return Mathf.Log(i) * 20;
    }

    public void ApplySettings() {

        mixer.SetFloat("MasterVolume", ConvertToVolume(masterVolume));
        mixer.SetFloat("MusicVolume", ConvertToVolume(musicVolume));
        mixer.SetFloat("SoundFXVolume", ConvertToVolume(effectsVolume));

    }
}
