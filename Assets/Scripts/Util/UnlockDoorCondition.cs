using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnlockDoorCondition : MonoBehaviour
{
    [Serializable]
    public class Condition {
        public int id;
        public bool value;
    }

    public List<Condition> conditions = new List<Condition>();

    public UnityEvent OnAllTrue;

    public void EnableId(int id) {
        for(int i = 0; i < conditions.Count; i++) {
            if(conditions[i].id == id) {
                conditions[i].value = true;
                break;
            }
        }

        UpdateCheck();
    }

    // Update is called once per frame
    void UpdateCheck()
    {
     
        for(int i = 0; i < conditions.Count; i++) {
            if (!conditions[i].value) {
                return;
            }
        }

        OnAllTrue.Invoke();
    }
}
