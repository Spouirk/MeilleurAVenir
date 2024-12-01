using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DialogueObject;
using UnityEngine.Events;
using System;
using System.Runtime.InteropServices;

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
    [SerializeField] SlowTyper descriptionBody;
    [SerializeField] PauseMenu pauseMenu;

    [Header("Settings")]
    [SerializeField] float voyanteTextDelay = 2.0f;
    [SerializeField] float voyanteTextDuration = 3.0f;
    [SerializeField] float voyanteTextShakeIntensity = 2.0f;

    [Header("Client")]
    [SerializeField] float clientWalkTime = 3.0f;
    [SerializeField] GameObject client;
    [SerializeField] GameObject clientOrigin;
    [SerializeField] GameObject clientDestination;
    [SerializeField] GameObject clientOutPoint;

    [Header("Skins")]
    [SerializeField] Sprite SalaryMan;
    [SerializeField] Sprite Religieux;
    [SerializeField] Sprite Ado;
    [SerializeField] Sprite Vache;

    [Header("Cards skin")]
    [SerializeField] Sprite cardLovers;
    [SerializeField] Sprite cardChariot;
    [SerializeField] Sprite cardPapesse;
    [SerializeField] Sprite cardDeath;
    [SerializeField] Sprite cardHermit;
    [SerializeField] Sprite cardLoversInverse;
    [SerializeField] Sprite cardChariotInverse;
    [SerializeField] Sprite cardPapesseInverse;
    [SerializeField] Sprite cardDeathInverse;
    [SerializeField] Sprite cardHermitInverse;

    [Header("Fade Settings")]
    [SerializeField] float fadeDuration = 1.0f;
    [SerializeField] float blackScreenDuration = 1.0f;

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
        if (newNode.IsNewWeek())
        {
            ClearCards();
            SpawnCards(newNode);
            InitializeCards(newNode);
        }
        if (newNode.IsNewClient())
        {
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
            case "Vache":
                client.GetComponent<Image>().sprite = Vache;
                break;
            case "Melvin":
                client.GetComponent<Image>().sprite = Vache;
                break;
            default:
                break;
        }
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
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.GetComponent<Image>().color = Color.Lerp(Color.black, Color.clear, t / fadeDuration);
            yield return null;
        }

        controller.ChooseResponse(0);
    }

    private void SpawnCards(Node newNode)
    {
        for (int i = 0; i < newNode.responses.Count; i++)
        {
            Card newCard = Instantiate(cardPrefab, CardManager.Instance.cardsHolder.transform).GetComponent<Card>();

            newCard.gameObject.name = newNode.responses[i].displayText;
            RectTransform cardRectTransform = newCard.GetComponent<RectTransform>();
            RectTransform canvasRectTransform = CardManager.Instance.canvasRectTransform;
            cardRectTransform.anchoredPosition = new Vector2(i * 100 - 200, -200);
            newCard.SetName(newNode.responses[i].displayText);
            newCard.SetButtonAction(delegate { OnNodeSelected(i); });

            ChangeCardSkin(newCard, newNode.responses[i].displayText);

            cards.Add(newCard);
        }
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
