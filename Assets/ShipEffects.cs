using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ShipEffects : MonoBehaviour {
    public Player player;
    public CinemachineVirtualCamera vCam;
    public float k = 8f;
    
    private SpriteRenderer sr;

    private void Start() {
        sr = player.GetComponentInChildren<SpriteRenderer>();
    }

    public void Run() {
        sr.enabled = false;
        player.transform.parent = transform;
        foreach (var sr in player.GetComponentInChildren<PlayerInventory>().equippedWeapon.GetComponentsInChildren<SpriteRenderer>()) {
            sr.enabled = false;
        }
        player.GetComponentInChildren<PlayerInventory>().equippedWeapon.enabled = false;
        vCam.Priority = 11;
        //vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain =
        StartCoroutine(RotateAndZoom());
    }

    private IEnumerator RotateAndZoom() {
        yield return new WaitForSeconds(1f);
        for (float i = 0; i <= 4; i += .011f) {
            //vCam.m_Lens.Dutch = -90f * ((i - 5) / 10);
            //vCam.m_Lens.OrthographicSize = 10 - Mathf.Cos((i - 5) * Mathf.PI / 10);
             float x = Mathf.Clamp(i - 3, 0, 1);
             vCam.GetComponent<CinemachineStoryboard>().m_Alpha = (Mathf.Exp(k * x) - 1) / (Mathf.Exp(k) - 1);
            yield return null;
        }

        vCam.GetComponent<CinemachineStoryboard>().m_Alpha = 1;
        
    }
}
