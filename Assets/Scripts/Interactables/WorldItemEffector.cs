using UnityEngine;
using UnityEngine.Events;

public abstract class WorldItemEffector: MonoBehaviour {
    public bool canBeManipulated;

    public UnityEvent itemEvent;

    public virtual bool CanUse() {
        return true;
    }

    public virtual void Use() {
        itemEvent.Invoke();
    }
}