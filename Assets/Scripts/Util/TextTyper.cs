using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextTyper : MonoBehaviour
{

    private string Text;
    public TextMeshProUGUI textUI;
    public TextMeshPro textGame;

    public bool PlayOnAwake;

    public UnityEvent OnFinished;

    public float TypeDelay;

    bool GotText = false;

    void Awake() {
        EnableType();
    }

    private void OnEnable() {
        EnableType();
    }




    private void EnableType() {
        if (!GotText) {
            if (textUI != null) Text = textUI.text;
            if (textGame != null) Text = textGame.text;
            GotText = true;
            Debug.Log("Got text: " + Text);
        }

        SetText("");
        if (PlayOnAwake) {
            StartType();
        }
    }
    public void StartType() {
        StartCoroutine(TypeEnumerator());
    }

    public IEnumerator TypeEnumerator() {
        Debug.Log("Playing text: " + Text);
        string currentText = "";
        for(int i = 0; i < Text.Length; i++) {
            currentText += Text[i];
            SetText(currentText);
            yield return new WaitForSeconds(TypeDelay);
        }
        Debug.Log("Finished text: " + currentText);

        OnFinished.Invoke();
    }

    public void SetText(string text) {
        if (textUI != null) textUI.text = text;
        if (textGame != null) textGame.text = text;
    }

    
}
