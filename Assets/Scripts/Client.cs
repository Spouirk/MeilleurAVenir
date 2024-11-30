using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour
{
    public enum ClientType {
        Salarie,
        Ado,
        Vache,
        HommeMarie,
        Politique,
        Religieux
    };

    public ClientType type;
}
