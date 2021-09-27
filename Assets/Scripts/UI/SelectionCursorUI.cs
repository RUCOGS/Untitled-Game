using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SelectionCursorUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler {

    public Image CursorImage;

    public UnityEvent OnClick;

    private string InitiationString;
    private TextMeshProUGUI textUI;

    public void Start() {
        CursorImage.enabled = false;
        textUI = GetComponent<TextMeshProUGUI>();
        InitiationString = textUI.text;
    }

    public void OnPointerClick(PointerEventData eventData) {
        //Do the things
        OnClick.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        textUI.text = " " + InitiationString;
        CursorImage.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        textUI.text = InitiationString;
        CursorImage.enabled = false;
    }
}
