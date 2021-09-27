using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShaker : MonoBehaviour
{

    public CinemachineVirtualCamera virtualCamera;

    private void Start() {
        if (virtualCamera == null) { 
            virtualCamera = Player.instance.PlayerCamera;
            Player.instance.camShaker = this;
        }
    }


    public float default_amp;
    public float default_freq;
    public float default_time;

    public void AddShake() {
        StartCoroutine(ShakerCoroutine(default_amp, default_freq, default_time));
    }
    public void AddShake(float amp, float freq, float time) {
        StartCoroutine(ShakerCoroutine(amp, freq, time));
    }

    public IEnumerator ShakerCoroutine(float amp, float freq, float time) {
        var noise = this.virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        
        noise.m_AmplitudeGain += amp;
        noise.m_FrequencyGain += freq;

        yield return new WaitForSeconds(time);

        noise.m_AmplitudeGain -= amp;
        noise.m_FrequencyGain -= freq;
    }

}
