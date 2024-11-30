using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;
    private bool isDragging = false;

    // Paramètres d'animation
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float animationDuration = 0.2f;
    private Vector3 originalScale;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;
        originalScale = transform.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        StartCoroutine(ScaleAnimation(1.05f)); // Légère augmentation pendant le drag
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        StartCoroutine(ScaleAnimation(1f)); // Retour à la taille normale
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDragging)
        {
            StartCoroutine(ScaleAnimation(hoverScale));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDragging)
        {
            StartCoroutine(ScaleAnimation(1f));
        }
    }

    private IEnumerator ScaleAnimation(float targetScale)
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScaleVector = originalScale * targetScale;
        float elapsedTime = 0;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            
            // Animation avec easing
            progress = Mathf.SmoothStep(0, 1, progress);
            
            transform.localScale = Vector3.Lerp(startScale, targetScaleVector, progress);
            yield return null;
        }

        transform.localScale = targetScaleVector;
    }

    public void PlayAppearAnimation()
    {
        StartCoroutine(AppearAnimation());
    }

    private IEnumerator AppearAnimation()
    {
        Image image = GetComponent<Image>();
        if (image != null)
        {
            // Commencer invisible et petit
            transform.localScale = Vector3.zero;
            Color startColor = image.color;
            startColor.a = 0;
            image.color = startColor;

            float elapsedTime = 0;
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / animationDuration;
                
                // Animation avec easing
                progress = Mathf.SmoothStep(0, 1, progress);
                
                // Scale up et fade in
                transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
                startColor.a = progress;
                image.color = startColor;
                
                yield return null;
            }

            transform.localScale = originalScale;
            startColor.a = 1;
            image.color = startColor;
        }
    }
}
