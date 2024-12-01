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
        pausePanel.SetActive(true);
    }

    public void ResumeGame() 
    {
        gameIsPaused = false;
        pausePanel.SetActive(false);
    }

    public void MainMenu()
    {
        gameIsPaused = false;
        SceneManager.LoadScene("Menu");
    }

    public bool IsGamePaused() {
        return gameIsPaused;
    }
}
