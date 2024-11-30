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
    [SerializeField] Card[] cards;
    [SerializeField] TMPro.TextMeshProUGUI voyanteText;

    [Header("Settings")]
    [SerializeField] float voyanteTextDelay = 2.0f;
    [SerializeField] float voyanteTextDuration = 3.0f;
    [SerializeField] float voyanteTextShakeIntensity = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<DialogueController>();
        controller.onEnteredNode += OnNodeEntered;
        controller.InitializeDialogue();
    }

    private void OnNodeEntered(Node newNode) {
        if (newNode.IsVoyanteNode()) {
            voyanteText.text = newNode.text;
            StartCoroutine(ShowVoyanteDialogue(voyanteTextDelay, voyanteTextDuration));
            StartCoroutine(ShakeDialogue(voyanteText, voyanteTextDelay + voyanteTextDuration));
            StartCoroutine(DelayedAction(voyanteTextDelay + voyanteTextDuration, delegate { controller.ChooseResponse(0); }));
            return;
        }

        txtTitle.text = newNode.title.Split("_")[0];
        txtBody.Begin(newNode.text);
        txtBody.SetFinishAction(delegate { CardManager.instance.canPlayCard = true; });
        for (int i = 0; i < newNode.responses.Count; i++) {
            int currentChoiceIndex = i;

            cards[i].SetName(newNode.responses[i].displayText);
            cards[i].SetButtonAction(delegate { OnNodeSelected(currentChoiceIndex); });
        }
    }

    private void OnNodeSelected(int indexChosen) {
        controller.ChooseResponse(indexChosen);
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
