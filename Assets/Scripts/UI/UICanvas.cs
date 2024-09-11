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
    [SerializeField] GameObject citySubObjectButtonPrefab;
    [SerializeField] GameObject citySubObjectOccupiedButtonPrefab;
    [SerializeField] GameObject unitButtonPrefab;
    [SerializeField] Image cityPanel;
    [SerializeField] UnitPanel unitPanel;
    [SerializeField] TextMeshProUGUI cityNameText;
    [SerializeField] TextMeshProUGUI cityPopulationText;
    [SerializeField] UIResourceList uiResourceList;
    [HideInInspector] City selectedCity;
    [SerializeField] private Button mainButton;
    private MainButtonState mainButtonState;

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
        if (mainButtonState == MainButtonState.NextTurn)
        {
            TurnManager.instance.HandleNewTurn();
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
        GameObject cityButton = Instantiate(cityButtonPrefab, transform);
        cityButton.GetComponent<WorldPositionButton>().targetTransform = city.transform;
        cityButton.GetComponentInChildren<Button>().onClick.AddListener(city.HandleClicked);
        return cityButton.GetComponent<WorldPositionButton>();
    }

    public WorldPositionButton CreateCitySubObjectButton(CitySubObject citySubObject)
    {
        GameObject citySubObjectButton = Instantiate(citySubObjectButtonPrefab, transform);
        citySubObjectButton.GetComponent<WorldPositionButton>().targetTransform = citySubObject.transform;
        citySubObjectButton.GetComponentInChildren<Button>().onClick.AddListener(citySubObject.HandleSubObjectButtonClicked);
        return citySubObjectButton.GetComponent<WorldPositionButton>(); 
    }

    public WorldPositionButton CreateCitySubObjectOccupiedButton(CitySubObject citySubObject)
    {
        GameObject citySubObjectOccupiedButton = Instantiate(citySubObjectOccupiedButtonPrefab, transform);
        citySubObjectOccupiedButton.GetComponent<WorldPositionButton>().targetTransform = citySubObject.transform;
        citySubObjectOccupiedButton.GetComponentInChildren<Button>().onClick.AddListener(delegate{citySubObject.HandleSubObjectOccupiedButtonClicked(selectedCity);});
        return citySubObjectOccupiedButton.GetComponent<WorldPositionButton>(); 
    }

    public WorldPositionButton CreateUnitButton(Unit unit)
    {
        GameObject unitButton = Instantiate(unitButtonPrefab, transform);
        unitButton.GetComponent<WorldPositionButton>().targetTransform = unit.transform;
        unitButton.GetComponentInChildren<Button>().onClick.AddListener(delegate{ObjectManager.instance.HandleTileSelected(unit.tileIndex);});
        return unitButton.GetComponent<WorldPositionButton>(); 
    }

    public void SetSelectedCity(City selectedCity)
    {
        this.selectedCity = selectedCity;
        cityNameText.text = selectedCity.cityName;
    }

    public void SetCityPanelVisible(bool visibility)
    {
        cityPanel.enabled = visibility;
        cityNameText.enabled = visibility;
        cityPopulationText.enabled = visibility;
        uiResourceList.enabled = visibility;
        
        if (visibility)
        {
            ObjectManager.instance.selectedUnit = null;
            uiResourceList.enabled = true;
            UpdateResourceList();
        }
        else
        {
            uiResourceList.ClearResourceDisplay();
        }

        if (selectedCity != null)
        {
            foreach (CitySubObject citySubObject in selectedCity.citySubObjects)
            {
                if (citySubObject.owningCity == selectedCity)
                {
                    citySubObject.SetCitySubObjectButtonActive(visibility);
                    citySubObject.SetCitySubObjectOccupiedButtonActive(false);
                }
                else
                {
                    citySubObject.SetCitySubObjectButtonActive(false);
                    citySubObject.SetCitySubObjectOccupiedButtonActive(visibility);
                }
            }
        }
    }

    public void UpdateResourceList()
    {
        List<Resource> resources = new List<Resource>();
        List<int> amounts = new List<int>();
        foreach (Resource resource in selectedCity.resourceProductionDict.Keys)
        {
            resources.Add(resource);
            amounts.Add(selectedCity.resourceProductionDict[resource]);
        }

        uiResourceList.resources = resources;
        uiResourceList.resourceAmounts = amounts;
        uiResourceList.UpdateResourceDisplay();
    }

    public void SetCityPopulationText(int availablePopulation, int totalPopulation)
    {
        cityPopulationText.text = "Pop: " + availablePopulation.ToString() + "/" + totalPopulation.ToString();
    }
}
