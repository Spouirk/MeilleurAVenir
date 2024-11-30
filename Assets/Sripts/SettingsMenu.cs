using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Linq;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] TMPro.TMP_Dropdown resolutionDropDown;
    [SerializeField] private Toggle toggle;
    Resolution[] resolutions;

    void Start()
    {
        resolutions = Screen.resolutions.Select(resolution => new Resolution {width = resolution.width, height = resolution.height}).Distinct().ToArray();
        resolutionDropDown.ClearOptions();

        List<string> resolutionOptions = new List<string>();

        int currentResolution = 0;
        for(int i = 0; i < resolutions.Length; i++) {
            resolutionOptions.Add(resolutions[i].width + "x" + resolutions[i].height);
            if(resolutions[i].width == Screen.width && resolutions[i].height == Screen.height) {
                currentResolution = i;
            }
        }

        resolutionDropDown.AddOptions(resolutionOptions);
        resolutionDropDown.value = currentResolution;
        resolutionDropDown.RefreshShownValue();
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

}
