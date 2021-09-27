using UnityEngine.UI;
using UnityEngine;

public class CoinCounter : MonoBehaviour {
    public PlayerInventory coinInventory;
    public Text text;
    void Start() {
        text.text = "0";
    }

    // Update is called once per frame
    void FixedUpdate() {
        text.text = coinInventory.GetCoins().ToString();
    }
}
