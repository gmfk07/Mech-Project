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
    [SerializeField] GameObject popIconPrefab;
    [SerializeField] Image cityPanel;
    [SerializeField] UnitPanel unitPanel;
    [SerializeField] TextMeshProUGUI cityNameText;
    [SerializeField] ResourceList resourceList;
    [HideInInspector] City selectedCity;
    [SerializeField] private Button mainButton;
    private MainButtonState mainButtonState;
    [SerializeField] private Transform worldElementParent;
    public Transform popIconParent;

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
            ObjectManager.instance.DeselectUnit();
            TurnManager.instance.HandleNewTurn();
            UpdateMainButton();
        }
        else if (mainButtonState == MainButtonState.NextUnit)
        {
            int tileSelected = -1;
            foreach (Unit unit in ObjectManager.instance.playerUnitDict[TurnManager.instance.currentPlayer])
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

    public void SetSelectedCity(City selectedCity)
    {
        this.selectedCity = selectedCity;
        cityNameText.text = selectedCity.cityName;
    }

    public void SetCityPanelVisible(bool visibility)
    {
        cityPanel.enabled = visibility;
        cityNameText.enabled = visibility;
        resourceList.enabled = visibility;
        
        if (visibility)
        {
            ObjectManager.instance.selectedUnit = null;
            resourceList.enabled = true;
            UpdateResourceList();
            UpdatePopDisplay();
            ObjectManager.instance.DeselectUnit();

            if (selectedCity != null)
            {
                foreach (CitySubObject citySubObject in selectedCity.citySubObjects)
                {
                    if (citySubObject.owningCity == selectedCity)
                    {
                        citySubObject.SetCitySubObjectPopPanelActive(true);
                    }
                    else
                    {
                        citySubObject.SetCitySubObjectPopPanelActive(false);
                    }
                }
            }
        }
        else
        {
            resourceList.ClearResourceDisplay();
            ClearPopDisplay();
            if (selectedCity != null)
            {
                foreach (CitySubObject citySubObject in selectedCity.citySubObjects)
                {
                    citySubObject.SetCitySubObjectPopPanelActive(false);
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

        resourceList.resources = resources;
        resourceList.resourceAmounts = amounts;
        resourceList.UpdateResourceDisplay();
    }

    public void UpdatePopDisplay()
    {
        ClearPopDisplay();
        for (int i=0; i < selectedCity.GetAvailablePopCount(); i++)
        {
            if (selectedCity.GetPop(i).working == null)
            {
                GameObject created = Instantiate(popIconPrefab, popIconParent);
                created.GetComponent<PopIcon>().popIndex = i;
                created.GetComponent<PopIcon>().owningCity = selectedCity;
            }
        }
    }

    public void ClearPopDisplay()
    {
        foreach (Transform child in popIconParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void UpdateUnitPanel()
    {
        unitPanel.UpdateUnitPanel();
    }
}
