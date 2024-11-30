using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    void Start()
    {
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        Screen.fullScreen = true;
    }

    public void StartGame()
    {
        //Changer nom de la scène à load
        SceneManager.LoadScene("Chloe");
    }

    public void Settings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void Credits()
    {
        creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
