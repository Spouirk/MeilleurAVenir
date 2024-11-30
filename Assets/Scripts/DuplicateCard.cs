using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DuplicateCard : MonoBehaviour
{
    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private Canvas targetCanvas;

    [SerializeField]
    private float delayBetweenSpawns = 0.2f;

    [SerializeField]
    private bool isSpawning = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            bool clickedOnThisCard = false;
            foreach (RaycastResult result in results)
            {
                if (result.gameObject == gameObject)
                {
                    clickedOnThisCard = true;
                    break;
                }
            }

            if (clickedOnThisCard && !isSpawning)
            {
                Debug.Log("Clic sur la carte principale");
                StartCoroutine(SpawnCardsRoutine());
            }
        }
    }

    IEnumerator SpawnCardsRoutine()
    {
        isSpawning = true;
        RectTransform mainCardRect = GetComponent<RectTransform>();
        Vector2 startPosition = mainCardRect.anchoredPosition;

        Vector2[] clonePositions = new Vector2[]
        {
            new Vector2( 249, -360),
            new Vector2(-108, -360),
            new Vector2(-464, -360),
            new Vector2(-811, -360)
        };

        Debug.Log($"Nombre de positions : {clonePositions.Length}");

        if (cardPrefab != null && targetCanvas != null)
        {
            foreach (Vector2 endPosition in clonePositions)
            {
                // Créer le clone
                GameObject duplicateCard = Instantiate(cardPrefab, targetCanvas.transform);
                RectTransform duplicateRect = duplicateCard.GetComponent<RectTransform>();
                duplicateRect.anchoredPosition = startPosition;

                // Supprimer les scripts DuplicateCard existants
                DuplicateCard[] duplicateScripts = duplicateCard.GetComponentsInChildren<DuplicateCard>();
                foreach (DuplicateCard script in duplicateScripts)
                {
                    if (script != this) Destroy(script);
                }

                // Récupérer ou ajouter le DraggableCard
                DraggableCard draggableCard = duplicateCard.GetComponent<DraggableCard>();
                if (draggableCard == null)
                {
                    draggableCard = duplicateCard.AddComponent<DraggableCard>();
                }

                // S'assurer que la carte est visible
                Image cardImage = duplicateCard.GetComponent<Image>();
                if (cardImage != null)
                {
                    Color color = cardImage.color;
                    color.a = 1f;
                    cardImage.color = color;
                }

                // Logs de debug
                Debug.Log($"Création de la carte à la position: {endPosition}");
                Debug.Log($"Canvas scale factor: {targetCanvas.scaleFactor}");
                Debug.Log($"Card Image component exists: {cardImage != null}");

                // Animer la carte
                StartCoroutine(AnimateCardDealing(duplicateCard, startPosition, endPosition));

                // Attendre que l'animation de distribution soit terminée avant de jouer l'animation d'apparition
                yield return StartCoroutine(AnimateCardDealing(duplicateCard, startPosition, endPosition));
                draggableCard.PlayAppearAnimation();

                yield return new WaitForSeconds(delayBetweenSpawns);
            }
        }
        else
        {
            Debug.LogError("Card Prefab ou Canvas non assigné!");
        }

        isSpawning = false;
    }

    private IEnumerator AnimateCardDealing(GameObject card, Vector2 startPos, Vector2 endPos)
    {
        float duration = 0.5f;
        float elapsedTime = 0;

        RectTransform rectTransform = card.GetComponent<RectTransform>();

        Vector2 controlPoint = (startPos + endPos) / 2 + new Vector2(0, 100);

        Vector3 startRotation = new Vector3(0, 0, -45);
        Vector3 endRotation = Vector3.zero;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

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