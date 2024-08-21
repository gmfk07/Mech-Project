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
    [SerializeField] Image cityPanel;
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
        citySubObjectButton.GetComponentInChildren<Button>().onClick.AddListener(citySubObject.HandleClicked);
        return citySubObjectButton.GetComponent<WorldPositionButton>(); 
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
        
        if (visibility)
        {
            ObjectManager.instance.selectedUnit = null;
            uiResourceList.enabled = true;
            UpdateResourceList();
        }

        if (selectedCity != null)
        {
            foreach (CitySubObject citySubObject in selectedCity.citySubObjects)
            {
                citySubObject.SetCitySubObjectButtonActive(visibility);
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

    public void SetCityPopulation(int availableCitizens, int totalCitizens)
    {
        cityPopulationText.text = "Pop: " + availableCitizens.ToString() + "/" + totalCitizens.ToString();
    }
}
