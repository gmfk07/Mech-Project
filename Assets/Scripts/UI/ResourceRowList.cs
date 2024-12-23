using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceRowList : MonoBehaviour
{
    [HideInInspector] public List<Resource> resources = new List<Resource>();
    [HideInInspector] public List<int> resourceAmounts = new List<int>();
    [SerializeField] private int resourcesPerRow;
    [SerializeField] private GameObject resourceRowPrefab;

    public void UpdateResourceDisplay()
    {
        ClearResourceDisplay();
        for (int i=0; i < Mathf.Ceil((float) resources.Count / resourcesPerRow); i++)
        {
            GameObject resourceRowObject = Instantiate(resourceRowPrefab, transform);
            ResourceRow resourceRow = resourceRowObject.GetComponent<ResourceRow>();
            
            int count = resources.Count % resourcesPerRow;
            //If not last row or modulo is 0, we want full resources on the row
            if (i != Mathf.Ceil((float) resources.Count / resourcesPerRow)-1 || count == 0)
            {
                count = resourcesPerRow;
            }
            Dictionary<Resource, int> resourceAmountDict = new Dictionary<Resource, int>();
            for (int j=0; j < count; j++)
            {
                resourceAmountDict.Add(resources[resourcesPerRow * i + j], resourceAmounts[resourcesPerRow * i + j]);
            }
            resourceRow.InitializeResources(resourceAmountDict);
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
