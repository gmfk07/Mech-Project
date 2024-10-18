using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NationManager : MonoBehaviour
{
    public static NationManager instance;
    public List<Nation> nations = new List<Nation>();
    public List<List<String>> nationCityNameLists = new List<List<string>>();
    public List<Dictionary<Resource, int>> nationResourceDicts = new List<Dictionary<Resource, int>>();
    public int nationCount;
    public List<Material> landBorderMaterials = new List<Material>();
    public List<Material> waterBorderMaterials = new List<Material>();
    public List<Building> startingAvailableBuildings;

    public void Start() {
        instance = this;
        nations = new List<Nation>();
        for (int i=0; i<nationCount; i++)
        {
            nations.Add(new Nation(i, landBorderMaterials[i], waterBorderMaterials[i], startingAvailableBuildings));
        }
        nationCityNameLists.Add(new List<string>() {"Avesta", "Partollin", "Loadae", "Tarvoa"});
        nationCityNameLists.Add(new List<string>() {"Gupite", "Isnamaya", "Utpiya Mite", "Tarahayate"});
    }
}

public class Nation
{
    public List<City> cities = new List<City>();
    public List<Building> availableBuildings = new List<Building>();
    public Material landBorderMaterial;
    public Material waterBorderMaterial;
    public int player;

    public Nation(int player, Material landBorderMaterial, Material waterBorderMaterial, List<Building> availableBuildings)
    {
        this.player = player;
        this.landBorderMaterial = landBorderMaterial;
        this.waterBorderMaterial = waterBorderMaterial;
        this.availableBuildings = availableBuildings;
    }
}