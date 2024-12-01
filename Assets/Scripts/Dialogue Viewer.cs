using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DialogueObject;
using UnityEngine.Events;
using System;
using System.Runtime.InteropServices;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

public class DialogueViewer : MonoBehaviour
{

    [Header("References")]
    [SerializeField] DialogueController controller;
    [SerializeField] TMPro.TextMeshProUGUI txtTitle;
    [SerializeField] SlowTyper txtBody;
    [SerializeField] List<Card> cards = new List<Card>();
    [SerializeField] TMPro.TextMeshProUGUI voyanteText;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject fadePanel;
    [SerializeField] TMPro.TextMeshProUGUI aubeText;
    [SerializeField] TMPro.TextMeshProUGUI newWeekText;
    [SerializeField] TMPro.TextMeshProUGUI remainingText;
    [SerializeField] SlowTyper descriptionBody;
    [SerializeField] PauseMenu pauseMenu;
    [SerializeField] DuplicateCard cardSpawner;
    [SerializeField] AudioSource cardUsedSound;

    [Header("Settings")]
    [SerializeField] float voyanteTextDelay = 2.0f;
    [SerializeField] float voyanteTextDuration = 3.0f;
    [SerializeField] float voyanteTextShakeIntensity = 2.0f;
    [SerializeField] float dealTime = 0.4f;

    [Header("Client")]
    [SerializeField] float clientWalkTime = 3.0f;
    [SerializeField] GameObject client;
    [SerializeField] GameObject clientOrigin;
    [SerializeField] GameObject clientDestination;
    [SerializeField] GameObject clientOutPoint;
    private string currentClientEntrySound;

    [Header("Skins")]
    [SerializeField] Sprite SalaryMan;
    [SerializeField] Sprite Religieux;
    [SerializeField] Sprite Ado;
    [SerializeField] Sprite Vache;
    [SerializeField] Sprite Politicien;
    [SerializeField] Sprite HommeMarie;

    [Header("Cards skin")]
    [SerializeField] Sprite cardLovers;
    [SerializeField] Sprite cardChariot;
    [SerializeField] Sprite cardPapesse;
    [SerializeField] Sprite cardDeath;
    [SerializeField] Sprite cardHermit;
    [SerializeField] Sprite cardEmperor;
    [SerializeField] Sprite cardLoversInverse;
    [SerializeField] Sprite cardChariotInverse;
    [SerializeField] Sprite cardPapesseInverse;
    [SerializeField] Sprite cardDeathInverse;
    [SerializeField] Sprite cardHermitInverse;
    [SerializeField] Sprite cardEmperorInverse;

    [Header("Fade Settings")]
    [SerializeField] float fadeDuration = 1.0f;
    [SerializeField] float weekFadeDuration = 3.0f;
    [SerializeField] float blackScreenDuration = 1.0f;
    [SerializeField] float defaultFadeDuration = 1.0f;

    public bool canGetToNextDialogue;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<DialogueController>();
        controller.onEnteredNode += OnNodeEntered;
        controller.InitializeDialogue();
    }

    private void OnNodeEntered(Node newNode)
    {
        CardManager.Instance.canPlayCard = false;
        if (newNode.IsEndNode()) {
            SceneManager.LoadScene("Menu");
        }
        if (newNode.isDescription())
        {
            ShowDescription(newNode);
            return;
        }
        descriptionBody.transform.parent.gameObject.SetActive(false);
        if (newNode.isFade())
        {
            StartCoroutine(Fade());
            if (client.transform.position == clientDestination.transform.position) StartCoroutine(ClientExitAnimation());
        }
        if (newNode.IsEndOfWeek()) {
            StartCoroutine(FadeEndOfWeek(newNode));
            ClearCards();
            if (client.transform.position == clientDestination.transform.position) StartCoroutine(ClientExitAnimation());
        } 
        if (newNode.IsNewWeek())
        {
            ClearCards();
            SpawnCards(newNode);
            InitializeCards(newNode);
        }
        if (newNode.IsNewClient())
        {
            PlayClientEntranceSound(newNode);
            StartCoroutine(ClientWalkAnimation());
            ChangeClientSkin(newNode.title.Split("_")[0]);
            if (newNode.IsQuestion())
            {
                if (!newNode.IsNewWeek()) SetCards(newNode);
                StartCoroutine(DelayedAction(clientWalkTime, delegate { ShowPNJQuestion(newNode); }));
            }
            else
            {
                StartCoroutine(DelayedAction(clientWalkTime, delegate { ShowPNJDialogue(newNode); }));
                StartCoroutine(WaitUntilNextDialogue(delegate { controller.ChooseResponse(0); }));
            }
            return;
        }
        if (newNode.IsQuestion())
        {
            if (!newNode.IsNewWeek()) SetCards(newNode);
            ShowPNJQuestion(newNode);
            return;
        }
        if (newNode.IsPNJNode())
        {
            ShowPNJDialogue(newNode);
            StartCoroutine(WaitUntilNextDialogue(delegate { controller.ChooseResponse(0); }));
        }
        if (newNode.IsVoyanteNode())
        {
            voyanteText.text = newNode.GetText();
            StartCoroutine(ShowVoyanteDialogue(voyanteTextDelay, voyanteTextDuration));
            StartCoroutine(ShakeDialogue(voyanteText, voyanteTextDelay + voyanteTextDuration));
            StartCoroutine(DelayedAction(voyanteTextDelay + voyanteTextDuration, delegate { controller.ChooseResponse(0); }));
        }
    }

    private void OnNodeSelected(int indexChosen)
    {
        controller.ChooseResponse(indexChosen);
    }

    private void ChangeClientSkin(string skinName)
    {
        switch (skinName)
        {
            case "Alex":
                client.GetComponent<Image>().sprite = SalaryMan;
                break;
            case "Religieux":
                client.GetComponent<Image>().sprite = Religieux;
                break;
            case "Cassandre":
                client.GetComponent<Image>().sprite = Ado;
                break;
            case "Marguerite":
                client.GetComponent<Image>().sprite = Vache;
                break;
            case "Melvin":
                client.GetComponent<Image>().sprite = Politicien;
                break;
            case "Eustache":
                client.GetComponent<Image>().sprite = HommeMarie;
                break;
            default:
                break;
        }
    }

    private void PlayClientEntranceSound(Node newNode) {
        switch (newNode.title.Split("_")[0])
        {
            case "Alex":
                currentClientEntrySound = "clientSalarieEntree";
                break;
            case "Religieux":
                currentClientEntrySound = "clientReligieuxEntree";
                break;
            case "Cassandre":
                currentClientEntrySound = "clientAdoEntree";
                break;
            case "Vache":
                currentClientEntrySound = "clientVacheEntree";
                break;
            case "Melvin":
                currentClientEntrySound = "clientPolitiqueEntree";
                break;
            default:
                break;
        }
        AudioManager.Instance.PlaySound(currentClientEntrySound);
        clientWalkTime = AudioManager.Instance.GetLength(currentClientEntrySound);
        fadeDuration = AudioManager.Instance.GetLength(currentClientEntrySound);
    }

    private void ClearCards()
    {
        foreach (Card card in cards)
        {
            Destroy(card.gameObject);
        }
        cards.Clear();
    }

    private IEnumerator Fade()
    {
        txtTitle.transform.parent.gameObject.SetActive(false);


        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.GetComponent<Image>().color = Color.Lerp(Color.clear, Color.black, t / fadeDuration);
            yield return null;
        }
        yield return new WaitForSeconds(blackScreenDuration);

        t = 0f;
        fadeDuration = defaultFadeDuration;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.GetComponent<Image>().color = Color.Lerp(Color.black, Color.clear, t / fadeDuration);
            yield return null;
        }

        controller.ChooseResponse(0);
    }

    private IEnumerator FadeEndOfWeek(Node newNode) {
        txtTitle.transform.parent.gameObject.SetActive(false);
        aubeText.gameObject.SetActive(true);
        newWeekText.gameObject.SetActive(true);
        remainingText.gameObject.SetActive(true);
        switch (newNode.GetText()) {
            case "Semaine 1":
                newWeekText.text = "Premiere Semaine";
                remainingText.text = "-3 Semaines Restantes-";
                break;
            case "Semaine 2":
                newWeekText.text = "Deuxieme Semaine";
                remainingText.text = "-2 Semaines Restantes-";
                break;
            case "Semaine 3":
                newWeekText.text = "Derniere Semaine";
                remainingText.text = "-1 Semaines Restantes-";
                break;
        }

        float t = 0f;
        while (t < weekFadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.GetComponent<Image>().color = Color.Lerp(Color.clear, Color.black, t / fadeDuration);
            aubeText.color = Color.Lerp(Color.clear, Color.white, t / fadeDuration);
            newWeekText.color = Color.Lerp(Color.clear, Color.white, t / fadeDuration);
            remainingText.color = Color.Lerp(Color.clear, Color.white, t / fadeDuration);
            yield return null;
        }
        yield return new WaitForSeconds(blackScreenDuration);

        t = 0f;
        fadeDuration = defaultFadeDuration;
        while (t < weekFadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.GetComponent<Image>().color = Color.Lerp(Color.black, Color.clear, t / fadeDuration);
            aubeText.color = Color.Lerp(Color.white, Color.clear, t / fadeDuration);
            newWeekText.color = Color.Lerp(Color.white, Color.clear, t / fadeDuration);
            remainingText.color = Color.Lerp(Color.white, Color.clear, t / fadeDuration);
            yield return null;
        }

        aubeText.gameObject.SetActive(false);
        newWeekText.gameObject.SetActive(false);
        remainingText.gameObject.SetActive(false);
        
        controller.ChooseResponse(0);
    }

    private void SpawnCards(Node newNode)
    {
        Debug.Log("Spawning cards");
        CardManager.Instance.isSpawningCards = true;
        Vector2[] spawnPositions = new Vector2[] { new Vector2(-200, -200), new Vector2(-100, -200), new Vector2(0, -200), new Vector2(100, -200) };

        Vector3 startScale = new Vector3(0, 0, 0);
        Vector3 endScale = new Vector3(cardPrefab.transform.localScale.x, cardPrefab.transform.localScale.y, cardPrefab.transform.localScale.z);

        for (int i = 0; i < newNode.responses.Count; i++)
        {
            Card newCard = Instantiate(cardPrefab, CardManager.Instance.cardsHolder.transform).GetComponent<Card>();

            newCard.gameObject.name = newNode.responses[i].displayText;
            RectTransform cardRectTransform = newCard.GetComponent<RectTransform>();
            RectTransform canvasRectTransform = CardManager.Instance.canvasRectTransform;
            newCard.transform.localScale = startScale;
            newCard.SetName(newNode.responses[i].displayText);
            newCard.SetButtonAction(delegate { OnNodeSelected(i); });

            ChangeCardSkin(newCard, newNode.responses[i].displayText);

            cards.Add(newCard);
        }

        cardSpawner.StartSpawnCards(spawnPositions, cards, startScale, endScale, dealTime);
    }

    private void ChangeCardSkin(Card card, string skinName)
    {
        switch (skinName)
        {
            case "Les Amoureux":
                card.SetImage(cardLovers);
                break;
            case "Le Chariot":
                card.SetImage(cardChariot);
                break;
            case "La Papesse":
                card.SetImage(cardPapesse);
                break;
            case "La Mort":
                card.SetImage(cardDeath);
                break;
            case "L'Hermite":
                card.SetImage(cardHermit);
                break;
            case "L'Empereur":
                card.SetImage(cardEmperor);
                break;
            case "Les Amoureux Inversés":
                card.SetImage(cardLoversInverse);
                break;
            case "Le Chariot Inversé":
                card.SetImage(cardChariotInverse);
                break;
            case "La Papesse Inversée":
                card.SetImage(cardPapesseInverse);
                break;
            case "La Mort Inversée":
                card.SetImage(cardDeathInverse);
                break;
            case "L'Hermite Inversé":
                card.SetImage(cardHermitInverse);
                break;
            case "L'Empereur Inversé":
                card.SetImage(cardEmperorInverse);
                break;
            default:
                break;
        }
    }

    private void InitializeCards(Node newNode)
    {
        bool isInverse = newNode.IsInverse();
        for (int i = 0; i < newNode.responses.Count; i++)
        {
            int currentIndex = i;
            Card card = cards[currentIndex];

            cards[currentIndex].SetButtonAction(delegate { UseCard(card, currentIndex); });
            if (isInverse) {
                StartCoroutine(FlipCard(card, newNode.responses[i].displayText));
                continue;
            }
            cards[currentIndex].SetName(newNode.responses[currentIndex].displayText);
            ChangeCardSkin(card, newNode.responses[i].displayText);
        }
    }

    private void SetCards(Node newNode)
    {
        bool isInverse = newNode.IsInverse();
        for (int i = 0; i < newNode.responses.Count; i++)
        {
            int currentIndex = i;

            foreach (Card card in cards)
            {
                if (card.GetName() == newNode.responses[i].displayText.Split(" Inversé")[0])
                {
                    card.SetButtonAction(delegate { UseCard(card, currentIndex); });
                    if (isInverse) {
                        StartCoroutine(FlipCard(card, newNode.responses[i].displayText));
                        continue;
                    }
                    card.SetName(newNode.responses[i].displayText);
                    ChangeCardSkin(card, newNode.responses[i].displayText);
                }
            }
        }
    }

    private IEnumerator FlipCard(Card card, string newName) {

        float flipTime = 0.5f;
        
        CardManager.Instance.canPlayCard = false;

        float t = 0;
        while (t < flipTime)
        {
            t += Time.deltaTime;
            card.transform.rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0, 0, 180), t / flipTime);
            yield return null;
        }

        card.SetName(newName);
        ChangeCardSkin(card, newName);
        card.transform.rotation = Quaternion.Euler(0, 0, 0);
        CardManager.Instance.canPlayCard = true;
    }

    public void UseCard(Card card, int index)
    {
        AudioManager.Instance.PlaySound("cardUsed");
        OnNodeSelected(index);
        cards.Remove(card);
        Destroy(card.gameObject);
    }

    private void ShowPNJQuestion(Node newNode)
    {
        txtTitle.transform.parent.gameObject.SetActive(true);
        txtTitle.text = newNode.title.Split("_")[0];
        txtBody.Begin(newNode.GetText());
        txtBody.SetFinishAction(delegate { CardManager.Instance.canPlayCard = true; });
    }

    private void ShowPNJDialogue(Node newNode)
    {
        canGetToNextDialogue = false;
        txtTitle.transform.parent.gameObject.SetActive(true);
        txtTitle.text = newNode.title.Split("_")[0];
        txtBody.Begin(newNode.GetText());
        txtBody.SetFinishAction(delegate { StartCoroutine(WaitForInputSkip()); });
    }

    private void ShowDescription(Node newNode)
    {
        descriptionBody.transform.parent.gameObject.SetActive(true);
        descriptionBody.Begin(newNode.GetText());
        descriptionBody.SetFinishAction(delegate { StartCoroutine(WaitForInputSkip()); });
    }

    private IEnumerator WaitUntilNextDialogue(UnityAction action)
    {
        canGetToNextDialogue = false;
        while (!canGetToNextDialogue)
        {
            yield return null;
        }
        action();
    }

    private IEnumerator WaitForInputSkip()
    {
        while (pauseMenu.IsGamePaused() || !Input.GetKeyDown(KeyCode.Space) && !Input.GetMouseButtonDown(0))
        {
            yield return null;
        }
        controller.ChooseResponse(0);
    }

    private IEnumerator ClientWalkAnimation()
    {
        client.transform.position = clientOrigin.transform.position;

        float t = 0;
        while (t < clientWalkTime)
        {
            t += Time.deltaTime;
            client.transform.position = Vector3.Lerp(clientOrigin.transform.position, clientDestination.transform.position, t / clientWalkTime);
            yield return null;
        }

        client.transform.position = clientDestination.transform.position;

        yield return null;
    }

    private IEnumerator ClientExitAnimation()
    {
        client.transform.position = clientDestination.transform.position;

        AudioManager.Instance.PlaySound(currentClientEntrySound);

        float t = 0;
        while (t < clientWalkTime)
        {
            t += Time.deltaTime;
            client.transform.position = Vector3.Lerp(clientDestination.transform.position, clientOutPoint.transform.position, t / clientWalkTime);
            yield return null;
        }

        client.transform.position = clientOutPoint.transform.position;

        yield return null;
    }

    private IEnumerator ShowVoyanteDialogue(float delay, float duration)
    {
        voyanteText.gameObject.SetActive(true);
        Color color = voyanteText.color;
        Color fullColor = new Color(color.r, color.g, color.b, 1.0f);
        Color clearColor = new Color(color.r, color.g, color.b, 0.0f);
        voyanteText.color = fullColor;

        float t = 0;

        yield return new WaitForSeconds(delay);

        while (t < duration)
        {
            t += Time.deltaTime;
            voyanteText.color = Color.Lerp(fullColor, clearColor, t / duration);
            yield return null;
        }
    }

    private IEnumerator ShakeDialogue(TMPro.TextMeshProUGUI text, float duration)
    {
        Vector3 originalPosition = text.transform.position;
        float shakeAmount = voyanteTextShakeIntensity;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            text.transform.position = originalPosition + UnityEngine.Random.insideUnitSphere * shakeAmount;
            yield return null;
        }

        text.transform.position = originalPosition;
    }

    private IEnumerator DelayedAction(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
}
