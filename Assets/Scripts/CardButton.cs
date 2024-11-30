using UnityEngine;
using UnityEngine.EventSystems;

public class CardButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler {

    private bool buttonPressed;
    private Card card;
    private bool isHovering;

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
        card.OnMouseHover(isHovering);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        Debug.Log("Mouse over a card");
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovering = false;
    }
}