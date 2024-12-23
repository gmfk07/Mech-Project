using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class NationManager : MonoBehaviour
{
    public static NationManager instance;
    public List<Nation> nations = new List<Nation>();
    public List<List<String>> nationCityNameLists = new List<List<string>>();
    public List<List<City>> nationCityLists = new List<List<City>>();
    public List<Dictionary<Resource, int>> nationResourceDicts = new List<Dictionary<Resource, int>>();
    public int nationCount;
    public List<Material> landBorderMaterials = new List<Material>();
    public List<Material> waterBorderMaterials = new List<Material>();
    public List<Building> startingAvailableBuildings;
    public List<Recruitable> startingAvailableRecruitables;

    public void Start() {
        instance = this;
        nations = new List<Nation>();
        for (int i=0; i<nationCount; i++)
        {
            nations.Add(new Nation(i, landBorderMaterials[i], waterBorderMaterials[i], startingAvailableBuildings, startingAvailableRecruitables));
            nationCityLists.Add(new List<City>());
            nationResourceDicts.Add(new Dictionary<Resource, int>());
        }
        nationCityNameLists.Add(new List<string>() {"Avesta", "Partollin", "Loadae", "Tarvoa"});
        nationCityNameLists.Add(new List<string>() {"Gupite", "Isnamaya", "Utpiya Mite", "Tarahayate"});
    }

    public void HandleNationResourceChange(int nationIndex, Resource resource, int amount)
    {
        if (resource.stockpilable)
        {
            Dictionary<Resource, int> resourceDict = nationResourceDicts[nationIndex];
            if (resourceDict.ContainsKey(resource))
            {
                resourceDict[resource] = Mathf.Max(0, resourceDict[resource] + amount);
            }
            else if (amount > 0)
            {
                resourceDict.Add(resource, amount);
            }
        }
    }

    public int GetNationResourceCount(int nationIndex, Resource resource)
    {
        if (nationResourceDicts[nationIndex].ContainsKey(resource))
        {
            return nationResourceDicts[nationIndex][resource];
        }
        return 0;
    }
}

public class Nation
{
    public List<City> cities = new List<City>();
    public List<Building> availableBuildings = new List<Building>();
    public List<Recruitable> availableRecruitables = new List<Recruitable>();
    public Material landBorderMaterial;
    public Material waterBorderMaterial;
    //Please keep player equal to the index of the nation in NationManager's nations
    public int player;

    public Nation(int player, Material landBorderMaterial, Material waterBorderMaterial, List<Building> availableBuildings, List<Recruitable> availableRecruitables)
    {
        this.player = player;
        this.landBorderMaterial = landBorderMaterial;
        this.waterBorderMaterial = waterBorderMaterial;
        this.availableBuildings = availableBuildings;
        this.availableRecruitables = availableRecruitables;
    }
}