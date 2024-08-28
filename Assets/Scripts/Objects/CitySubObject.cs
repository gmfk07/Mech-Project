using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CitySubObject : Object
{
    [HideInInspector] public City owningCity;
    private WorldPositionButton citySubObjectButton;
    private WorldPositionButton citySubObjectOccupiedButton;
    public Dictionary<Resource, int> resourceProductionDict = new Dictionary<Resource, int>();
    //Workaround for Unity not having dictionaries in inspector
    [SerializeField] private List<Resource> resourceProductionDictKeys;
    [SerializeField] private List<int> resourceProductionDictValues;
    [HideInInspector] public bool worked = false;

    new void Start()
    {
        base.Start();
        for (int i=0; i<resourceProductionDictKeys.Count; i++)
        {
            resourceProductionDict.Add(resourceProductionDictKeys[i], resourceProductionDictValues[i]);
        }
        CreateCitySubObjectButtons();
        UpdateCitySubObjectButtonColor();
    }

    void CreateCitySubObjectButtons()
    {
        citySubObjectButton = UICanvas.instance.CreateCitySubObjectButton(this);
        SetCitySubObjectButtonActive(false);
        citySubObjectOccupiedButton = UICanvas.instance.CreateCitySubObjectOccupiedButton(this);
        SetCitySubObjectOccupiedButtonActive(false);
    }

    public void SetCitySubObjectButtonActive(bool enabled)
    {
        citySubObjectButton.gameObject.SetActive(enabled);
    }

    public void SetCitySubObjectOccupiedButtonActive(bool enabled)
    {
        citySubObjectOccupiedButton.gameObject.SetActive(enabled);
    }

    public void HandleSubObjectButtonClicked()
    {
        if (!worked && owningCity.HasAvailablePopulation(1))
        {
            StartBeingWorked();
        }
        else if (worked)
        {
            StopBeingWorked();
        }
        UpdateCitySubObjectButtonColor();
        UICanvas.instance.UpdateResourceList();
    }

    //Stops being worked, giving a population back to owner city and removing production.
    private void StopBeingWorked()
    {
        worked = false;
        owningCity.ChangeAvailablePopulation(1);
        foreach (Resource resource in resourceProductionDict.Keys)
        {
            owningCity.resourceProductionDict[resource] -= resourceProductionDict[resource];
            if (owningCity.resourceProductionDict[resource] == 0)
            {
                owningCity.resourceProductionDict.Remove(resource);
            }
        }
    }

    //Takes a population from owner City and becomes worked, adding production to owner City.
    private void StartBeingWorked()
    {
        worked = true;
        owningCity.ChangeAvailablePopulation(-1);
        foreach (Resource resource in resourceProductionDict.Keys)
        {
            if (owningCity.resourceProductionDict.ContainsKey(resource))
            {
                owningCity.resourceProductionDict[resource] += resourceProductionDict[resource];
            }
            else
            {
                owningCity.resourceProductionDict.Add(resource, resourceProductionDict[resource]);
            }
        }
    }

    public void HandleSubObjectOccupiedButtonClicked(City selectedCity)
    {
        //Add a pop back to owning city if worked
        if (worked)
        {
            StopBeingWorked();
        }
        owningCity = selectedCity;
        SetCitySubObjectButtonActive(true);
        SetCitySubObjectOccupiedButtonActive(false);
    }

    public void UpdateCitySubObjectButtonColor()
    {
        if (worked)
        {
            citySubObjectButton.gameObject.GetComponentInChildren<Button>().colors = ColorBlock.defaultColorBlock;
        }
        else
        {
            ColorBlock unworkedColorBlock = ColorBlock.defaultColorBlock;
            unworkedColorBlock.normalColor = Color.gray;
            unworkedColorBlock.selectedColor = Color.gray;
            citySubObjectButton.gameObject.GetComponentInChildren<Button>().colors = unworkedColorBlock;
        }
    }
}
