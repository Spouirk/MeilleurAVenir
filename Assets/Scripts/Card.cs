using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TMPro.TextMeshProUGUI cardName;
    [SerializeField] Button button;
    RectTransform canvasRectTransform;
    RectTransform cardRectTransform;

    [Header("Drag Zone")]
    [SerializeField] GameObject dragZoneObject;
    RectTransform dragZonePanel;
    Image dragZoneImage;

    public TMPro.TextMeshProUGUI debugText;
    
    private float acceleration = 9.8f;
    private float currentVelocity;
    private float defaultScale;
    private bool isFalling;

    private void Awake() {
        dragZoneObject = CardManager.Instance.dragZoneObject;
        dragZonePanel = dragZoneObject.GetComponent<RectTransform>();
        dragZoneImage = dragZoneObject.GetComponent<Image>();

        canvasRectTransform = CardManager.Instance.canvasRectTransform;
        cardRectTransform = GetComponent<RectTransform>();

        acceleration = CardManager.Instance.acceleration;
        defaultScale = cardRectTransform.localScale.x;
    }

    private UnityAction cardUsedAction;
    private Vector3 grabOffset;
    private bool isDragging;

    public bool buttonPressed;

    public void SetName(string name)
    {
        cardName.text = name;
    }

    public void SetButtonAction(UnityAction action)
    {
        cardUsedAction = action;
    }

    private void Update() {
        Fall();
        isFalling = currentVelocity > 0f;
    }

    private void Fall() {
        if (isDragging) return;
        if (IsAboveBottomOfCanvas())
        {
            currentVelocity += acceleration * Time.deltaTime;
            cardRectTransform.position += Vector3.down * currentVelocity * Time.deltaTime;
        }
        else
        {
            currentVelocity = 0f;
        }
    }

    public void OnHeldDown(bool isHeldDown) {
        if (!CardManager.Instance.canPlayCard || (!isHeldDown && !isDragging)) return;
        if (!isHeldDown && isDragging) {
            if (isDragging && RectTransformUtility.RectangleContainsScreenPoint(dragZonePanel, Input.mousePosition, null)) {
                cardUsedAction.Invoke();
                gameObject.SetActive(false);
            }
            dragZoneImage.color = new Color(0, 0, 0, 0);
            isDragging = false;
            RepositionInBounds();
            return;
        }
        if (!isDragging) {
            isDragging = true;
            grabOffset = transform.position - Input.mousePosition;
        }
        dragZoneImage.color = RectTransformUtility.RectangleContainsScreenPoint(dragZonePanel, Input.mousePosition, null) ? Color.green : Color.red;
        transform.position = Input.mousePosition + grabOffset;
    }

    public void OnMouseHover(bool isOver) {
        if (!isDragging && isOver && !isFalling) {
            ZoomOnCard();
            RepositionInBounds();
        } else {
            cardRectTransform.localScale = Vector3.one * defaultScale;
        }
        // Debug.Log("Mouse over " + cardName.text + " " + isOver);
        // if (isOver) Debug.Log("Mouse over " + cardName.text);
    }

    private void ZoomOnCard() {
        cardRectTransform.localScale = Vector3.one * defaultScale * 1.5f;
    }

    public string GetName()
    {
        return cardName.text;
    }

    public void SetImage(Sprite image)
    {
        GetComponent<Image>().sprite = image;
    }

    private bool IsAboveBottomOfCanvas()
    {
        float cardBottomY = cardRectTransform.position.y - (cardRectTransform.rect.height * cardRectTransform.lossyScale.y / 2);
        float canvasBottomY = canvasRectTransform.position.y - (canvasRectTransform.rect.height * canvasRectTransform.lossyScale.y / 2);
        return cardBottomY > canvasBottomY;
    }

    private void RepositionInBounds()
    {
        float cardWidth = cardRectTransform.rect.width * cardRectTransform.lossyScale.x;
        float cardHeight = cardRectTransform.rect.height * cardRectTransform.lossyScale.y;
        float canvasWidth = canvasRectTransform.rect.width * canvasRectTransform.lossyScale.x;
        float canvasHeight = canvasRectTransform.rect.height * canvasRectTransform.lossyScale.y;

        float cardLeftX = cardRectTransform.position.x - (cardWidth / 2);
        float cardRightX = cardRectTransform.position.x + (cardWidth / 2);
        float cardTopY = cardRectTransform.position.y + (cardHeight / 2);
        float cardBottomY = cardRectTransform.position.y - (cardHeight / 2);

        float canvasLeftX = canvasRectTransform.position.x - (canvasWidth / 2);
        float canvasRightX = canvasRectTransform.position.x + (canvasWidth / 2);
        float canvasTopY = canvasRectTransform.position.y + (canvasHeight / 2);
        float canvasBottomY = canvasRectTransform.position.y - (canvasHeight / 2);

        float newX = cardRectTransform.position.x;
        float newY = cardRectTransform.position.y;

        if (cardLeftX < canvasLeftX)
        {
            newX += canvasLeftX - cardLeftX;
        }
        else if (cardRightX > canvasRightX)
        {
            newX -= cardRightX - canvasRightX;
        }

        if (cardTopY > canvasTopY)
        {
            newY -= cardTopY - canvasTopY;
        }
        else if (cardBottomY < canvasBottomY)
        {
            newY += canvasBottomY - cardBottomY;
        }

        cardRectTransform.position = new Vector3(newX, newY, cardRectTransform.position.z);
    }
}
