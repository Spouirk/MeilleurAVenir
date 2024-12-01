using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip musique;
    [SerializeField] private AudioSource audioSource;
    private static bool firstStart = true;

    // Start is called before the first frame update
    void Start()
    {
        if(firstStart) {
            firstStart = false;
            DontDestroyOnLoad(audioSource);
            audioSource.clip = musique;
            audioSource.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!audioSource.isPlaying) {
            audioSource.Play();
        }
    }

    void OnApplicationQuit()
    {
        firstStart = true;
    }
}
