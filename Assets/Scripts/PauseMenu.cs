using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private bool gameIsPaused;

    void Start()
    {
        pausePanel.SetActive(false);
        gameIsPaused = false;
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
        gameIsPaused = true;
        Time.timeScale = 0;
        pausePanel.SetActive(true);
    }

    public void ResumeGame() 
    {
        gameIsPaused = false;
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }

    public void MainMenu()
    {
        gameIsPaused = false;
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }
}
