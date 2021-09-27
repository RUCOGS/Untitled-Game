using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public Weapon equippedWeapon;
    private Transform equippedWeaponTransform;

    public Transform playerTransform;

    public float throwStrength;

    private float gunDist;

    private int coins;
    
    public void ConsumeCurrentlyHeld() {
        if (equippedWeapon) {
            Destroy(equippedWeapon.gameObject);
            equippedWeapon = null;
            equippedWeaponTransform = null;
        }
    }

    public void EquipNewWeapon(Weapon newWeapon) {
        if (equippedWeapon) { // Only trigger if there was a weapon equipped prior
            equippedWeapon.equipped = false;
            Rigidbody2D oldWeaponRb = equippedWeapon.gameObject.GetComponent<Rigidbody2D>();
            ResetRb(oldWeaponRb);
            ThrowWeapon(oldWeaponRb);
        }

        equippedWeapon = newWeapon;
        equippedWeaponTransform = newWeapon.transform;
        newWeapon.equipped = true;
        ResetRb(equippedWeapon.gameObject.GetComponent<Rigidbody2D>());
        if (equippedWeapon is Gun) {
            gunDist = ((Gun)equippedWeapon).holdDist;
        }
    }

    /*
     * Resets Rigidbody to no rotation, or velocity, and inverses `simulated` property
     */
    private static void ResetRb(Rigidbody2D rb) {
        rb.simulated = !rb.simulated;
        rb.rotation = 0;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
    }
    
    private void ThrowWeapon(Rigidbody2D rb) {
        rb.AddForce(
            PlayerToMouse().normalized * throwStrength,
            ForceMode2D.Impulse);
        rb.angularVelocity = 720f;
    }

    private Vector3 PlayerToMouse() {
        return Camera.allCameras[0].ScreenToWorldPoint(Input.mousePosition) - playerTransform.position;
    }

    private void Update() {
        if (!equippedWeapon) return;
        
        switch (equippedWeapon) {
            case Gun _:
                var playerToCursorVec = PlayerToMouse();
                var newRot = new Vector3(playerToCursorVec.x, playerToCursorVec.y, 0).normalized;
                equippedWeaponTransform.right = newRot;
                if (equippedWeaponTransform.eulerAngles.y != 0) { // Hacky fix for weapon rotating weirdly when z went to 0 between 180 and -180
                    equippedWeaponTransform.eulerAngles = new Vector3(0f, 0f, 180f);
                }

                ((Gun)equippedWeapon).flipY(newRot.x < 0);

                var eulerZ = equippedWeaponTransform.rotation.eulerAngles.z;
                var gunAngle = (newRot.x < 0 && eulerZ == 0f  ? 180f : eulerZ) * Mathf.Deg2Rad;
                equippedWeaponTransform.position = playerTransform.position +
                                                   gunDist * new Vector3(Mathf.Cos(gunAngle), Mathf.Sin(gunAngle), 0);
                break;
            case Sword _:
                break;
        }
    }
    public void AddCoins(int num) {
        coins += num;
    }

    public int GetCoins() {
        return coins;
    }
}
