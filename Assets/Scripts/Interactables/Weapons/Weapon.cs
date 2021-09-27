using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Weapon : MonoBehaviour, IManualInteract {
    public bool equipped;

    private PlayerInventory PlayerInventory;

    protected int layerMask;

    public bool FrozenUntilEquiped = false;

    protected virtual void Start() {
        PlayerInventory = Player.instance.inventory;
        layerMask = LayerMask.GetMask("Enemy", "Default");

        if (FrozenUntilEquiped) {
            GetComponent<Rigidbody2D>().isKinematic = true;
        }
    }

    protected virtual void Update() {
        if (Input.GetMouseButton(0) && equipped) {
            Attack();
        }
    }

    protected abstract void Attack();

    public virtual void Use() {
        if (FrozenUntilEquiped) {
            GetComponent<Rigidbody2D>().isKinematic = false;
        }

        PlayerInventory.EquipNewWeapon(this);
    }
    public bool CanUse() {
        return true;
    }
}
