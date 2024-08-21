using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CitySubObject : Object
{
    [HideInInspector] public City owner;
    private WorldPositionButton citySubObjectButton;
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
        CreateCitySubObjectButton();
        UpdateCitySubObjectButtonColor();
    }

    void CreateCitySubObjectButton()
    {
        citySubObjectButton = UICanvas.instance.CreateCitySubObjectButton(this);
        SetCitySubObjectButtonActive(false);
    }

    public void SetCitySubObjectButtonActive(bool enabled)
    {
        citySubObjectButton.gameObject.SetActive(enabled);
    }

    public void HandleClicked()
    {
        worked = !worked;
        UpdateCitySubObjectButtonColor();
        if (worked)
        {
            foreach (Resource resource in resourceProductionDict.Keys)
            {
                if (owner.resourceProductionDict.ContainsKey(resource))
                {
                    owner.resourceProductionDict[resource] += resourceProductionDict[resource];
                }
                else
                {
                    owner.resourceProductionDict.Add(resource, resourceProductionDict[resource]);
                }
            }
        }
        else
        {
            foreach (Resource resource in resourceProductionDict.Keys)
            {
                owner.resourceProductionDict[resource] -= resourceProductionDict[resource];
                if (owner.resourceProductionDict[resource] == 0)
                {
                    owner.resourceProductionDict.Remove(resource);
                }
            }
        }
        UICanvas.instance.UpdateResourceList();
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
