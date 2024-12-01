using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Events;

public class DuplicateCard : MonoBehaviour
{
    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private Canvas targetCanvas;

    [SerializeField]
    private float delayBetweenSpawns = 0.2f;

    [SerializeField]
    private bool hasSpawnedCard = false;

    private Vector2[] positions;
    private List<Card> cards;
    private Vector3 startScale;
    private Vector3 endScale;

    [SerializeField] GameObject spawnPoint;


    void Update()
    {
        // if (Input.GetMouseButtonDown(0))
        // {
        //     PointerEventData pointerData = new PointerEventData(EventSystem.current);
        //     pointerData.position = Input.mousePosition;

        //     List<RaycastResult> results = new List<RaycastResult>();
        //     EventSystem.current.RaycastAll(pointerData, results);

        //     bool clickedOnThisCard = false;
        //     foreach (RaycastResult result in results)
        //     {
        //         if (result.gameObject == gameObject)
        //         {
        //             clickedOnThisCard = true;
        //             break;
        //         }
        //     }

        //     if (clickedOnThisCard && !hasSpawnedCard)
        //     {
        //         Debug.Log("Clic sur la carte principale");
        //         StartCoroutine(SpawnCardsRoutine());
        //     }
        // }
    }

    public void StartSpawnCards(Vector2[] positions, List<Card> cards, Vector3 startScale, Vector3 endScale)
    {
        if (!hasSpawnedCard)
        {
            this.positions = positions;
            this.cards = cards;
            this.startScale = startScale;
            this.endScale = endScale;
            StartCoroutine(SpawnCardsRoutine());
        }
    }

    IEnumerator SpawnCardsRoutine()
    {
        hasSpawnedCard = true;
        Vector2 startPosition = spawnPoint.GetComponent<RectTransform>().anchoredPosition;

        if (cardPrefab != null && targetCanvas != null)
        {
            int i = 0;
            foreach (Vector2 endPosition in positions)
            {
                Card duplicateCard = cards[i];
                RectTransform duplicateRect = duplicateCard.GetComponent<RectTransform>();
                duplicateRect.anchoredPosition = startPosition;

                // Animer la carte
                StartCoroutine(AnimateCardDealing(duplicateCard, startPosition, endPosition));

                // Attendre que l'animation de distribution soit terminée avant de jouer l'animation d'apparition
                yield return StartCoroutine(AnimateCardDealing(duplicateCard, startPosition, endPosition));

                i++;
                yield return new WaitForSeconds(delayBetweenSpawns);
            }
            CardManager.Instance.isSpawningCards = false;
        }
    }

    private IEnumerator AnimateCardDealing(Card card, Vector2 startPos, Vector2 endPos)
    {
        float duration = 0.2f;
        float elapsedTime = 0;

        RectTransform rectTransform = card.GetComponent<RectTransform>();

        Vector2 controlPoint = (startPos + endPos) / 2 + new Vector2(0, 100);

        Vector3 startRotation = new Vector3(0, 0, -45);
        Vector3 endRotation = Vector3.zero;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            Vector2 position = QuadraticBezier(startPos, controlPoint, endPos, smoothT);
            rectTransform.anchoredPosition = position;

            rectTransform.localRotation = Quaternion.Euler(Vector3.Lerp(startRotation, endRotation, smoothT));
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, smoothT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = endPos;
        rectTransform.localRotation = Quaternion.Euler(endRotation);
        rectTransform.localScale = endScale;
    }

    private Vector2 QuadraticBezier(Vector2 start, Vector2 control, Vector2 end, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector2 p = uu * start;
        p += 2 * u * t * control;
        p += tt * end;

        return p;
    }
}