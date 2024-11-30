using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CardButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

    private bool buttonPressed;
    private Card card;

    private void Awake() {
        card = transform.parent.GetComponent<Card>();
    }

    public void OnPointerDown(PointerEventData eventData){
        buttonPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData){
        buttonPressed = false;
    }

    private void Update() {
        card.OnHeldDown(buttonPressed);
    }
}