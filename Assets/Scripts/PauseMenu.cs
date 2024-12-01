using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    private bool gameIsPaused;

    void Start()
    {
        pausePanel.SetActive(false);
        gameIsPaused = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            Debug.Log("Escape pressed");
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
        CloseSettings();
    }

    public void Settings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void MainMenu()
    {
        gameIsPaused = false;
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }

    public bool IsGamePaused() {
        return gameIsPaused;
    }
}
