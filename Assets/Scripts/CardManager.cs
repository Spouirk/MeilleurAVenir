using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardManager : MonoBehaviour
{

    public static CardManager Instance;

    Dictionary<string, string> dialogueVariable = new Dictionary<string, string>();

    public string GetVariable(string variableName)
    {
        string returnValue;
        bool isSet = dialogueVariable.TryGetValue(variableName, out returnValue);
        UnityEngine.Assertions.Assert.IsTrue(isSet,variableName + " is not not defined");
        return returnValue;
    }

    public void SetVariable(string variableName, string value) 
    {
        if (dialogueVariable.ContainsKey(variableName)) dialogueVariable[variableName] = value;
        else dialogueVariable.Add(variableName, value);
    }

    [Header("Falling Parameters")]
    public float acceleration = 9.8f;

    [Header("Scene References")]
    public GameObject dragZoneObject;
    public RectTransform canvasRectTransform;
    public GameObject cardsHolder;

    public bool canPlayCard = false;

    private void Awake() {
        Instance = this;
    }
}
