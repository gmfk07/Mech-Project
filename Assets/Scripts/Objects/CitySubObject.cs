using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CitySubObject : Object
{
    [HideInInspector] public City owningCity;
    private CitySubObjectPopPanel citySubObjectPopPanel;
    public Dictionary<Resource, int> resourceProductionDict = new Dictionary<Resource, int>();
    //Workaround for Unity not having dictionaries in inspector
    [SerializeField] private List<Resource> resourceProductionDictKeys;
    [SerializeField] private List<int> resourceProductionDictValues;
    [HideInInspector] public int workerIndex = -1;

    new void Start()
    {
        base.Start();
        for (int i=0; i<resourceProductionDictKeys.Count; i++)
        {
            resourceProductionDict.Add(resourceProductionDictKeys[i], resourceProductionDictValues[i]);
        }
        CreateCitySubObjectPopPanel();
    }

    void CreateCitySubObjectPopPanel()
    {
        citySubObjectPopPanel = UICanvas.instance.CreateCitySubObjectPopPanel(this);
        citySubObjectPopPanel.citySubObject = this;
        SetCitySubObjectPopPanelActive(false);
    }

    public CitySubObjectPopPanel GetCitySubObjectPopPanel()
    {
        return citySubObjectPopPanel;
    }

    public void SetCitySubObjectPopPanelActive(bool enabled)
    {
        citySubObjectPopPanel.gameObject.SetActive(enabled);
    }

    //Stops being worked, giving a population back to owner city and removing production.
    public void StopBeingWorked()
    {
        owningCity.GetPop(workerIndex).working = null;
        owningCity.SetPopAvailable(workerIndex, true);
        workerIndex = -1;
        foreach (Resource resource in resourceProductionDict.Keys)
        {
            owningCity.resourceProductionDict[resource] -= resourceProductionDict[resource];
            if (owningCity.resourceProductionDict[resource] == 0)
            {
                owningCity.resourceProductionDict.Remove(resource);
            }
        }
        UICanvas.instance.UpdateCityResourceList();
    }

    //Given a popIndex, sets this pop as working for this subobject and unavailable and updates resources
    public void StartBeingWorked(int popIndex)
    {
        owningCity.GetPop(popIndex).working = this;
        owningCity.SetPopAvailable(popIndex, false);
        workerIndex = popIndex;

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

        UICanvas.instance.UpdateCityResourceList();
    }
}
