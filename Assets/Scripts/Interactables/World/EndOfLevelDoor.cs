using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndOfLevelDoor : WorldItemEffector, IManualInteract {
    public Player player;

    public float transitionDelay;
    
    private PlayerInventory playerInv;

    public int coinsRequired;

    public String nextLevel;

    private void Start() {
        canBeManipulated = true;
        playerInv = player.GetComponentInChildren<PlayerInventory>();
    }

    public override void Use() {
        if (playerInv.GetCoins() >= coinsRequired) {
            base.Use();
            //SceneManager.LoadSceneAsync(nextLevel, LoadSceneMode.Single);
            StartCoroutine(DelayExit());
        }
    }

    public IEnumerator DelayExit() {
        yield return new WaitForSeconds(transitionDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }



}
