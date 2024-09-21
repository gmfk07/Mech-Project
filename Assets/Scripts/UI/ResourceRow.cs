using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceRow : MonoBehaviour
{
    [HideInInspector] public Dictionary<Resource, int> resourceAmountDict = new Dictionary<Resource, int>();
    [SerializeField] private GameObject resourceCounterPrefab;

    void Start()
    {
        foreach (Resource resource in resourceAmountDict.Keys)
        {
            GameObject child = Instantiate(resourceCounterPrefab, transform);
            child.GetComponentInChildren<Image>().sprite = resource.sprite;
            child.GetComponentInChildren<TextMeshProUGUI>().text = resourceAmountDict[resource].ToString();
        }
    }
}
