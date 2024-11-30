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
    
    private float acceleration = 9.8f;
    private float currentVelocity;

    private void Awake() {
        dragZoneObject = CardManager.instance.dragZoneObject;
        dragZonePanel = dragZoneObject.GetComponent<RectTransform>();
        dragZoneImage = dragZoneObject.GetComponent<Image>();

        canvasRectTransform = CardManager.instance.canvasRectTransform;
        cardRectTransform = GetComponent<RectTransform>();

        acceleration = CardManager.instance.acceleration;
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
        if (!isHeldDown && !isDragging) return;
        if (!isHeldDown && isDragging) {
            if (isDragging && RectTransformUtility.RectangleContainsScreenPoint(dragZonePanel, Input.mousePosition, null)) {
                cardUsedAction.Invoke();
                gameObject.SetActive(false);
            }
            dragZoneImage.color = new Color(0, 0, 0, 0);
            isDragging = false;
            return;
        }
        if (!isDragging) {
            isDragging = true;
            grabOffset = transform.position - Input.mousePosition;
        }
        dragZoneImage.color = RectTransformUtility.RectangleContainsScreenPoint(dragZonePanel, Input.mousePosition, null) ? Color.green : Color.red;
        transform.position = Input.mousePosition + grabOffset;
    }

    private bool IsAboveBottomOfCanvas()
    {
        float cardBottomY = cardRectTransform.position.y - (cardRectTransform.rect.height * cardRectTransform.lossyScale.y / 2);
        float canvasBottomY = canvasRectTransform.position.y - (canvasRectTransform.rect.height * canvasRectTransform.lossyScale.y / 2);
        return cardBottomY > canvasBottomY;
    }
}
