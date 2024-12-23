using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StockpileResourceList : MonoBehaviour
{
    [HideInInspector] public List<Resource> resources = new List<Resource>();
    [HideInInspector] public List<int> resourceAmounts = new List<int>();
    [SerializeField] private GameObject resourceCounterPrefab;

    public void UpdateResourceDisplay()
    {
        ClearResourceDisplay();
        for (int i=0; i<resources.Count; i++)
        {
            GameObject resourceCounter = Instantiate(resourceCounterPrefab, transform);
            resourceCounter.GetComponentInChildren<Image>().sprite = resources[i].sprite;
            resourceCounter.GetComponentInChildren<TextMeshProUGUI>().text = resourceAmounts[i].ToString();
        }
    }

    public void ClearResourceDisplay()
    {
        for (int i=0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}