using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;
    [HideInInspector] public int currentPlayer = -1;

    void Start()
    {
        instance = this;
        currentPlayer = -1;
        HandleNewTurn();
    }

    public void HandleNewTurn()
    {
        currentPlayer = (currentPlayer + 1) % NationManager.instance.nationCount;
        ObjectManager.instance.HandleNewTurn();
    }
}
