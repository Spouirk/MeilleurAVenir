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

    public bool canGetToNextDialogue;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<DialogueController>();
        controller.onEnteredNode += OnNodeEntered;
        controller.InitializeDialogue();
    }

    private void OnNodeEntered(Node newNode) {
        if (newNode.isDescription()) {
            ShowDescription(newNode);
            return;
        }
        descriptionBody.transform.parent.gameObject.SetActive(false);
        if (newNode.isFade()) {
            StartCoroutine(Fade());
            StartCoroutine(ClientExitAnimation());
        }
        if (newNode.IsNewWeek()) {
            ClearCards();
            SpawnCards(newNode);
            InitializeCards(newNode);
        }
        if (newNode.IsNewClient()) {
            StartCoroutine(ClientWalkAnimation());
            ChangeClientSkin(newNode.title.Split("_")[0]);
            if (newNode.IsQuestion()) {
                if (!newNode.IsNewWeek()) SetCards(newNode);
                StartCoroutine(DelayedAction(clientWalkTime, delegate { ShowPNJQuestion(newNode); }));
            } else {
                StartCoroutine(DelayedAction(clientWalkTime, delegate { ShowPNJDialogue(newNode); }));
                StartCoroutine(WaitUntilNextDialogue(delegate { controller.ChooseResponse(0); }));
            }
            return;
        }
        if (newNode.IsQuestion()) {
            if (!newNode.IsNewWeek()) SetCards(newNode);
            ShowPNJQuestion(newNode);
            return;
        }
        if (newNode.IsPNJNode()) {
            ShowPNJDialogue(newNode);
            StartCoroutine(WaitUntilNextDialogue(delegate { controller.ChooseResponse(0); }));
        }
        if (newNode.IsVoyanteNode()) {
            voyanteText.text = newNode.text;
            StartCoroutine(ShowVoyanteDialogue(voyanteTextDelay, voyanteTextDuration));
            StartCoroutine(ShakeDialogue(voyanteText, voyanteTextDelay + voyanteTextDuration));
            StartCoroutine(DelayedAction(voyanteTextDelay + voyanteTextDuration, delegate { controller.ChooseResponse(0); }));
        }
    }

    private void OnNodeSelected(int indexChosen) {
        controller.ChooseResponse(indexChosen);
    }

    private void ChangeClientSkin(string skinName) {
        switch (skinName) {
            case "Salary Man":
                client.GetComponent<Image>().sprite = SalaryMan;
                break;
            case "Religieux":
                client.GetComponent<Image>().sprite = Religieux;
                break;
            case "Ado":
                client.GetComponent<Image>().sprite = Ado;
                break;
            case "Vache":
                client.GetComponent<Image>().sprite = Vache;
                break;
            default:
                break;
        }
    }

    private void ClearCards() {
        foreach (Card card in cards) {
            Destroy(card.gameObject);
        }
        cards.Clear();
    }

    private IEnumerator Fade() {
        float fadeDuration = 1.0f;
        float blackScreenDuration = 1.0f;
        txtTitle.transform.parent.gameObject.SetActive(false);


        float t = 0f;
        while (t < fadeDuration) {
            t += Time.deltaTime;
            fadePanel.GetComponent<Image>().color = Color.Lerp(Color.clear, Color.black, t / fadeDuration);
            yield return null;
        }
        yield return new WaitForSeconds(blackScreenDuration);

        t = 0f;
        while (t < fadeDuration) {
            t += Time.deltaTime;
            fadePanel.GetComponent<Image>().color = Color.Lerp(Color.black, Color.clear, t / fadeDuration);
            yield return null;
        }

        controller.ChooseResponse(0);
    }

    private void SpawnCards(Node newNode) {
        for (int i = 0; i < newNode.responses.Count; i++) {
            Card newCard = Instantiate(cardPrefab, CardManager.Instance.cardsHolder.transform).GetComponent<Card>();

            newCard.gameObject.name = newNode.responses[i].displayText;
            RectTransform cardRectTransform = newCard.GetComponent<RectTransform>();
            RectTransform canvasRectTransform = CardManager.Instance.canvasRectTransform;
            cardRectTransform.anchoredPosition = new Vector2(i * 100 - 200, -200);
            newCard.SetName(newNode.responses[i].displayText);
            newCard.SetButtonAction(delegate { OnNodeSelected(i); });

            switch (newNode.responses[i].displayText) {
                case "The Lovers":
                    newCard.SetImage(cardLovers);
                    break;
                case "The Chariot":
                    newCard.SetImage(cardChariot);
                    break;
                default:
                    break;
            }

            cards.Add(newCard);
        }
    }

    private void InitializeCards(Node newNode) {
        for (int i = 0; i < newNode.responses.Count; i++) {
            int currentIndex = i;
            Card card = cards[currentIndex];

            cards[currentIndex].SetName(newNode.responses[currentIndex].displayText);
            cards[currentIndex].SetButtonAction(delegate {UseCard(card, currentIndex);});
        }
    }

    private void SetCards(Node newNode) {
        for (int i = 0; i < newNode.responses.Count; i++) {
            int currentIndex = i;

            foreach (Card card in cards) {
                if (card.GetName() == newNode.responses[i].displayText) {
                    card.SetButtonAction(delegate { UseCard(card, currentIndex); });
                }
            }
        }
    }

    public void UseCard(Card card, int index) {
        OnNodeSelected(index);
        cards.Remove(card);
        Destroy(card.gameObject);
    }    

    private void ShowPNJQuestion(Node newNode) {
        txtTitle.transform.parent.gameObject.SetActive(true);
        txtTitle.text = newNode.title.Split("_")[0];
        txtBody.Begin(newNode.text);
        txtBody.SetFinishAction(delegate { CardManager.Instance.canPlayCard = true; });
    } 

    private void ShowPNJDialogue(Node newNode) {
        canGetToNextDialogue = false;
        txtTitle.transform.parent.gameObject.SetActive(true);
        txtTitle.text = newNode.title.Split("_")[0];
        txtBody.Begin(newNode.text);
        txtBody.SetFinishAction(delegate { StartCoroutine(WaitForInputSkip()); });
    }

    private void ShowDescription(Node newNode) {
        descriptionBody.transform.parent.gameObject.SetActive(true);
        descriptionBody.Begin(newNode.text);
        descriptionBody.SetFinishAction(delegate { StartCoroutine(WaitForInputSkip());});
    }

    private IEnumerator WaitUntilNextDialogue(UnityAction action) {
        canGetToNextDialogue = false;
        while (!canGetToNextDialogue) {
            yield return null;
        }
        action();
    }

    private IEnumerator WaitForInputSkip() {
        while (!Input.GetKeyDown(KeyCode.Space) && !Input.GetMouseButtonDown(0)) {
            yield return null;
        }
        controller.ChooseResponse(0);
    }

    private IEnumerator ClientWalkAnimation() {
        client.transform.position = clientOrigin.transform.position;

        float t = 0;
        while (t < clientWalkTime) {
            t += Time.deltaTime;
            client.transform.position = Vector3.Lerp(clientOrigin.transform.position, clientDestination.transform.position, t / clientWalkTime);
            yield return null;
        }

        client.transform.position = clientDestination.transform.position;

        yield return null;
    }

    private IEnumerator ClientExitAnimation() {
        client.transform.position = clientDestination.transform.position;

        float t = 0;
        while (t < clientWalkTime) {
            t += Time.deltaTime;
            client.transform.position = Vector3.Lerp(clientDestination.transform.position, clientOutPoint.transform.position, t / clientWalkTime);
            yield return null;
        }

        client.transform.position = clientOutPoint.transform.position;

        yield return null;
    }

    private IEnumerator ShowVoyanteDialogue(float delay, float duration) {
        voyanteText.gameObject.SetActive(true);
        Color color = voyanteText.color;
        Color fullColor = new Color(color.r, color.g, color.b, 1.0f);
        Color clearColor = new Color(color.r, color.g, color.b, 0.0f);
        voyanteText.color = fullColor;

        float t = 0;

        yield return new WaitForSeconds(delay);

        while (t < duration) {
            t += Time.deltaTime;
            voyanteText.color = Color.Lerp(fullColor, clearColor, t / duration);
            yield return null;
        }
    }

    private IEnumerator ShakeDialogue(TMPro.TextMeshProUGUI text, float duration) {
        Vector3 originalPosition = text.transform.position;
        float shakeAmount = voyanteTextShakeIntensity;
        float t = 0;

        while (t < duration) {
            t += Time.deltaTime;
            text.transform.position = originalPosition + UnityEngine.Random.insideUnitSphere * shakeAmount;
            yield return null;
        }

        text.transform.position = originalPosition;
    }

    private IEnumerator DelayedAction(float delay, Action action) {
        yield return new WaitForSeconds(delay);
        action();
    }
}
