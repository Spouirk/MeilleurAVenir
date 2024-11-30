using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private bool gameIsPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            if(gameIsPaused) {
                ResumeGame();
            }
            else {
                PauseGame();
            }
        }
    }

    private void PauseGame() {
        pausePanel.SetActive(true);
        gameIsPaused = true;
    }

    public void ResumeGame() 
    {
        pausePanel.SetActive(false);
        gameIsPaused = false;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Menu");
        gameIsPaused = false;
    }
}
