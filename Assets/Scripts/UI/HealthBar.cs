using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public HealthManager health;

    public RectTransform HealthPanel;

    public bool IsVertical = false;

    // Start is called before the first frame update
    void Start() {
        health.OnChange.AddListener(OnChange);
    }

    public void OnChange(int amount) {
        UpdateBar();
    }

    public void UpdateBar() {


        Vector2 anchor = HealthPanel.anchorMax;
        if (IsVertical) {
            anchor.y = Mathf.Clamp((float)health.Health / health.MaxHealth, 0, 1);
        } else {
            anchor.x = Mathf.Clamp((float)health.Health / (float)health.MaxHealth, 0, 1);
        }
        HealthPanel.anchorMax = anchor;

        HealthPanel.sizeDelta = Vector2.zero;
    }
}
