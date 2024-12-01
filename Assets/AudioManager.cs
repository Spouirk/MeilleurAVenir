using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] AudioSource mainMusic;
    [SerializeField] AudioSource cardUsedSound;
    [SerializeField] AudioSource cardHoverSound;
    [SerializeField] AudioSource cardDealSound;

    [SerializeField] AudioSource clientReligieuxEntree;
    [SerializeField] AudioSource clientVacheEntree;
    [SerializeField] AudioSource clientPolitiqueEntree;
    [SerializeField] AudioSource clientAdoEntree;
    [SerializeField] AudioSource clientMarieEntree;
    [SerializeField] AudioSource clientSalarieEntree;

    private void Awake() {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(string soundName)
    {
        switch (soundName)
        {
            case "cardDeal":
                cardDealSound.Play();
                break;
            case "cardUsed":
                cardUsedSound.Play();
                break;
            case "cardHover":
                cardHoverSound.Play();
                break;
            case "clientReligieuxEntree":
                clientReligieuxEntree.Play();
                break;
            case "clientVacheEntree":
                clientVacheEntree.Play();
                break;
            case "clientPolitiqueEntree":
                clientPolitiqueEntree.Play();
                break;
            case "clientAdoEntree":
                clientAdoEntree.Play();
                break;
            case "clientMarieEntree":
                clientMarieEntree.Play();
                break;
            case "clientSalarieEntree":
                clientSalarieEntree.Play();
                break;
        }
    }

    public float GetLength(string soundName)
    {
        switch (soundName)
        {
            case "cardUsed":
                return cardUsedSound.clip.length;
            case "cardHover":
                return cardHoverSound.clip.length;
            case "clientReligieuxEntree":
                return clientReligieuxEntree.clip.length;
            case "clientVacheEntree":
                return clientVacheEntree.clip.length;
            case "clientPolitiqueEntree":
                return clientPolitiqueEntree.clip.length;
            case "clientAdoEntree":
                return clientAdoEntree.clip.length;
            case "clientMarieEntree":
                return clientMarieEntree.clip.length;
            case "clientSalarieEntree":
                return clientSalarieEntree.clip.length;
            default:
                return 0f;
        }
    }
}
