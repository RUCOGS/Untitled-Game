using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarAdvanced : MonoBehaviour
{
    public HealthManager health;

    public RectTransform HealthBar;
    private Image healthBarImage;
    public RectTransform DamageBar;

    public Color normal_health;
    public Color low_health;

    [Range(0,1)]
    public float lowHealthPercentage = 0.2f;

    public bool IsVertical = false;
    public float LerpSpeed;

    public float ChunkDelay = 0.5f;
    private float curChunkDelay;
    // Start is called before the first frame update
    void Start() {
        healthBarImage = HealthBar.GetComponent<Image>();
        health.OnChange.AddListener(OnChange);
    }

    public void OnChange(int amount) {
        UpdateBar();
    }

    public void UpdateBar() {

        float percentage = (float)health.Health / health.MaxHealth;
        curChunkDelay = ChunkDelay;
        if (percentage <= lowHealthPercentage) {
            healthBarImage.color = low_health;
        } else {
            healthBarImage.color = normal_health;
        }

        Vector2 anchor = HealthBar.anchorMax;
        if (IsVertical) {
            anchor.y = Mathf.Clamp(percentage, 0, 1);
        } else {
            anchor.x = Mathf.Clamp(percentage, 0, 1);
        }
        HealthBar.anchorMax = anchor;

        HealthBar.sizeDelta = Vector2.zero;
    }




    public void Update() {
        if(curChunkDelay > 0) {
            curChunkDelay -= Time.deltaTime;
            return;
        }

        if(DamageBar.anchorMax.sqrMagnitude < HealthBar.anchorMax.sqrMagnitude) {
            DamageBar.anchorMax = HealthBar.anchorMax;
        }

        DamageBar.anchorMax = Vector2.Lerp(DamageBar.anchorMax, HealthBar.anchorMax, Time.deltaTime * LerpSpeed);
        DamageBar.sizeDelta = Vector2.zero;


    }

}
