using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;
    [HideInInspector] public int currentPlayer = 0;

    void Start()
    {
        instance = this;
    }
}
