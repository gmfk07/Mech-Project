using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceRow : MonoBehaviour
{
    [SerializeField] private GameObject resourceCounterPrefab;

    public void InitializeResources(Dictionary<Resource, int> resourceAmountDict)
    {
        foreach (Resource resource in resourceAmountDict.Keys)
        {
            GameObject child = Instantiate(resourceCounterPrefab, transform);
            child.GetComponentInChildren<Image>().sprite = resource.sprite;
            child.GetComponentInChildren<TextMeshProUGUI>().text = resourceAmountDict[resource].ToString();
        }
    }
}
