using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{

    public SettingsObject settingValues;


    public Slider masterSlider;
    public Slider musicSlider;
    public Slider effectsSlider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake() {
        LoadSettings();
    }

    private void OnEnable() {
        LoadSettings();
    }

    public void LoadSettings() {
        masterSlider.value = settingValues.masterVolume;
        musicSlider.value = settingValues.musicVolume;
        effectsSlider.value = settingValues.effectsVolume;
    }


    // Update is called once per frame
    void Update()
    {


        settingValues.masterVolume = masterSlider.value;
        settingValues.musicVolume = musicSlider.value;
        settingValues.effectsVolume = effectsSlider.value;

        settingValues.ApplySettings();
    }



}
