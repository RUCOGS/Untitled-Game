using UnityEngine;

public class Coin : MonoBehaviour, IAutoPickup {

    private PlayerInventory playerInventory;

    private void Start() {
        playerInventory = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerInventory>();
    }

    public void Use() {
        playerInventory.AddCoins(1);
    }

    public void OnPickup() {
        Destroy(gameObject);
    }
}
