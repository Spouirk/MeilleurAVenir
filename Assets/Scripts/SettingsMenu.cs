using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Linq;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private TMPro.TMP_Dropdown resolutionDropDown;
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private Slider generalSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider;
    Resolution[] resolutions;

    private int currentResolution;

    void Start()
    {
        resolutions = Screen.resolutions
                            .Where(resolution => is169(resolution))
                            .Select(resolution => new Resolution {width = resolution.width, height = resolution.height}).Distinct().ToArray();
        resolutionDropDown.ClearOptions();

        List<string> resolutionOptions = new List<string>();

        currentResolution = -1;
        for(int i = 0; i < resolutions.Length; i++) {
            resolutionOptions.Add(resolutions[i].width + "x" + resolutions[i].height);
            if(resolutions[i].width == Screen.width && resolutions[i].height == Screen.height) {
                currentResolution = i;
            }
        }

        currentResolution = currentResolution == -1 ? resolutions.Length - 1 : currentResolution;
        resolutionDropDown.AddOptions(resolutionOptions);
        resolutionDropDown.value = currentResolution;
        resolutionDropDown.RefreshShownValue();
        fullScreenToggle.isOn = Screen.fullScreen;

        float general = 0f;
        float music = 0f;
        float sound = 0f;
        audioMixer.GetFloat("volume", out general);
        audioMixer.GetFloat("music", out music);
        audioMixer.GetFloat("sound", out sound);
        generalSlider.value = general;
        musicSlider.value = music;
        soundSlider.value = sound;
    }

    public void SetMainVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("music", volume);
    }

    public void SetSoundVolume(float volume)
    {
        audioMixer.SetFloat("sound", volume);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Resolution resolution = resolutions[currentResolution];
        Screen.SetResolution(resolution.width, resolution.height, isFullScreen);
        //Screen.fullScreen = isFullScreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    private static bool is169(Resolution resolution)
    {
        int width = resolution.width;
        int height = resolution.height;
        return width / height == 16 / 9 && (width % height) % (16 % 9) == 0 && width % 16 == 0 && height % 9 == 0;
    }

}
