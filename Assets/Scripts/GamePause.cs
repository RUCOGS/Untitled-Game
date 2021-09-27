using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePause : MonoBehaviour
{

    public static bool isPaused = false;
    public GameObject PauseMenu;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape)) {
            isPaused = !isPaused;
            UpdatePause();
        }

    }

    public void UnPause() {
        isPaused = false;
        UpdatePause();
    }

    void UpdatePause() {

        if (!isPaused) {
            Time.timeScale = 1;
            PauseMenu.SetActive(false);
        } else {
            Time.timeScale = 0;
            PauseMenu.SetActive(true);
        }
    }
}
