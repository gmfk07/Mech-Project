using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CityPanel : MonoBehaviour
{
    City selectedCity;
    [SerializeField] TextMeshProUGUI cityNameText;
    [SerializeField] UnityEngine.UI.Image cityPanel;
    [SerializeField] ResourceRowList cityResourceRowList;
    [SerializeField] RecruitmentPanel recruitmentPanel;
    [SerializeField] GameObject popIconPrefab;
    public Transform popIconParent;

    public void SetSelectedCity(City selectedCity)
    {
        this.selectedCity = selectedCity;
        recruitmentPanel.SetSelectedCity(selectedCity);
        cityNameText.text = selectedCity.cityName;
    }

    public void SetCityPanelVisible(bool visibility)
    {
        cityPanel.enabled = visibility;
        cityNameText.enabled = visibility;
        cityResourceRowList.enabled = visibility;
        recruitmentPanel.SetVisible(visibility);
        
        if (visibility)
        {
            ObjectManager.instance.selectedUnit = null;
            cityResourceRowList.enabled = true;
            UpdateCityResourceList();
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
            cityResourceRowList.ClearResourceDisplay();
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

    public void UpdateCityResourceList()
    {
        List<Resource> resources = new List<Resource>();
        List<int> amounts = new List<int>();
        foreach (Resource resource in selectedCity.resourceProductionDict.Keys)
        {
            resources.Add(resource);
            amounts.Add(selectedCity.resourceProductionDict[resource]);
        }

        cityResourceRowList.resources = resources;
        cityResourceRowList.resourceAmounts = amounts;
        cityResourceRowList.UpdateResourceDisplay();
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

    public void ReparentTransformToCityPanel(Transform toReparent)
    {
        toReparent.SetParent(popIconParent);
    }

    public void UpdateCityPanelRecruiting()
    {
        recruitmentPanel.UpdateRecruitmentText();
    }
}
