using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HexasphereGrid;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

enum MainButtonState { NextUnit, NextTurn }

public class UICanvas : MonoBehaviour
{
    public static UICanvas instance;
    private Hexasphere hexa;
    [SerializeField] GameObject cityButtonPrefab;
    [SerializeField] GameObject citySubObjectPopPanelPrefab;
    [SerializeField] GameObject unitButtonPrefab;
    [SerializeField] GameObject worldPositionTextPrefab;
    [SerializeField] CityPanel cityPanel;
    [SerializeField] UnitPanel unitPanel;
    [SerializeField] StockpileResourceList stockpileResourceList;
    [SerializeField] private Button mainButton;
    private MainButtonState mainButtonState;
    [SerializeField] private Transform worldElementParent;

    void Start()
    {
        instance = this;
        hexa = Hexasphere.GetInstance("Hexasphere");
        hexa.OnDragStart += (hexa) => SetCityPanelVisible(false);
        hexa.OnTileClick += (hexa, tileIndex) => SetCityPanelVisible(false);
        hexa.OnTileRightClick += (hexa, tileIndex) => SetCityPanelVisible(false);
        StartCoroutine(UpdateMainButtonEnumerator());
        SetCityPanelVisible(false);
        unitPanel.SetVisibility(false);
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            SetCityPanelVisible(false);
        }
    }

    public void HandleNewTurn()
    {
        UpdateStockpileResourceList();
    }
 
    //Update the main button once ObjectManager and TurnManager are initialized.
    IEnumerator UpdateMainButtonEnumerator() {
        while (ObjectManager.instance == null || TurnManager.instance == null)
        {
            yield return null;
        }
        UpdateMainButton();
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
        int currentPlayer = TurnManager.instance.currentPlayer;
        if (mainButtonState == MainButtonState.NextTurn)
        {
            TurnManager.instance.HandleNewTurn();
            UpdateMainButton();
        }
        else if (mainButtonState == MainButtonState.NextUnit && ObjectManager.instance.playerUnitDict.ContainsKey(currentPlayer))
        {
            int tileSelected = -1;
            foreach (Unit unit in ObjectManager.instance.playerUnitDict[currentPlayer])
            {
                if (tileSelected == -1 && unit.active)
                {
                    tileSelected = unit.tileIndex;
                }
            }
            ObjectManager.instance.HandleTileSelected(tileSelected);
        }
    }

    public void HandleUnitSelected()
    {
        unitPanel.SetVisibility(true);
    }

    public void HandleUnitDeselected()
    {
        unitPanel.SetVisibility(false);
    }

    public WorldPositionButton CreateCityButton(City city)
    {
        GameObject cityButton = Instantiate(cityButtonPrefab, worldElementParent);
        cityButton.GetComponent<WorldPositionButton>().targetTransform = city.transform;
        cityButton.GetComponentInChildren<Button>().onClick.AddListener(city.HandleClicked);
        return cityButton.GetComponent<WorldPositionButton>();
    }

    public CitySubObjectPopPanel CreateCitySubObjectPopPanel(CitySubObject citySubObject)
    {
        GameObject citySubObjectButton = Instantiate(citySubObjectPopPanelPrefab, worldElementParent);
        citySubObjectButton.GetComponent<WorldPositionElement>().targetTransform = citySubObject.transform;
        return citySubObjectButton.GetComponent<CitySubObjectPopPanel>(); 
    }

    public WorldPositionButton CreateUnitButton(Unit unit)
    {
        GameObject unitButton = Instantiate(unitButtonPrefab, worldElementParent);
        unitButton.GetComponent<WorldPositionButton>().targetTransform = unit.transform;
        unitButton.GetComponentInChildren<Button>().onClick.AddListener(delegate{ObjectManager.instance.HandleTileSelected(unit.tileIndex);});
        return unitButton.GetComponent<WorldPositionButton>(); 
    }

    public WorldPositionElement CreateWorldPositionText(Transform transform)
    {
        GameObject worldPositionText = Instantiate(worldPositionTextPrefab, worldElementParent);
        worldPositionText.GetComponent<WorldPositionElement>().targetTransform = transform;
        return worldPositionText.GetComponent<WorldPositionElement>(); 
    }

    public void UpdateStockpileResourceList()
    {
        Dictionary<Resource, int> resourceDict = NationManager.instance.nationResourceDicts[TurnManager.instance.currentPlayer];
        List<Resource> resources = resourceDict.Keys.ToList();
        List<int> resourceAmounts = new List<int>();
        foreach (Resource resource in resources)
        {
            resourceAmounts.Add(resourceDict[resource]);
        }
        stockpileResourceList.resources = resources;
        stockpileResourceList.resourceAmounts = resourceAmounts;
        stockpileResourceList.UpdateResourceDisplay();
    }

    public void UpdateUnitPanel()
    {
        unitPanel.UpdateUnitPanel();
    }

    public void SetCityPanelVisible(bool visibility)
    {
        cityPanel.SetCityPanelVisible(visibility);
    }

    public void SetCityPanelSelectedCity(City selectedCity)
    {
        cityPanel.SetSelectedCity(selectedCity);
    }

    public void UpdateCityPanelRecruiting()
    {
        cityPanel.UpdateCityPanelRecruiting();
    }

    public void UpdateCityResourceList()
    {
        cityPanel.UpdateCityResourceList();
    }

    public void ReparentTransformToCityPanel(Transform toReparent)
    {
        cityPanel.ReparentTransformToCityPanel(toReparent);
    }
}
