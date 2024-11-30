using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{

    public static CardManager instance;

    [Header("Falling Parameters")]
    public float acceleration = 9.8f;

    [Header("Scene References")]
    public GameObject dragZoneObject;
    public RectTransform canvasRectTransform;

    public bool canPlayCard = false;

    private void Awake() {
        instance = this;
    }
}
