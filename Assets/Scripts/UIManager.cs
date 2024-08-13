using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HexasphereGrid;

enum MainButtonState { NextTurn, NextUnit }
public class UIManager : MonoBehaviour
{
    Hexasphere hexa;

    public static UIManager instance;

    [SerializeField] private Button mainButton;
    private MainButtonState mainButtonState;

    void Start() {
        hexa = Hexasphere.GetInstance("Hexasphere");
        instance = this;
    }

    public void UpdateMainButton() {
        DetermineMainButtonState();
        UpdateMainButtonText();
    }

    void DetermineMainButtonState() {
        bool unitActive = false;
        if (ObjectManager.instance.playerUnitDict.ContainsKey(TurnManager.instance.currentPlayer))
        {
            foreach (Unit unit in ObjectManager.instance.playerUnitDict[TurnManager.instance.currentPlayer])
            {
                if (unit.active) { unitActive = true; }
            }
        }
        if (unitActive)
        {
            mainButtonState = MainButtonState.NextUnit;
        }
        else
        {
            mainButtonState = MainButtonState.NextTurn; 
        }
    }

    void UpdateMainButtonText() {
       switch (mainButtonState)
       {
            case MainButtonState.NextTurn:
                mainButton.GetComponentInChildren<TextMeshProUGUI>().text = "End Turn";
            break;
            case MainButtonState.NextUnit:
                mainButton.GetComponentInChildren<TextMeshProUGUI>().text = "Next Unit";
            break;
       } 
    }

    public void HandleMainButtonPressed() {
        if (mainButtonState == MainButtonState.NextTurn)
        {
            ObjectManager.instance.HandleNewTurn();
            UpdateMainButton();
        }
        else if (mainButtonState == MainButtonState.NextUnit)
        {
            int tileToFlyTo = -1;
            foreach (Unit unit in ObjectManager.instance.playerUnitDict[TurnManager.instance.currentPlayer])
            {
                if (tileToFlyTo == -1 && unit.active)
                {
                    ObjectManager.instance.selectedUnit = unit;
                    tileToFlyTo = unit.tileIndex;
                }
            }
            hexa.FlyTo(tileToFlyTo, 0.5f);
            
        }
    }
}
