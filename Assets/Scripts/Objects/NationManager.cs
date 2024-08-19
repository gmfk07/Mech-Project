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

    public void Start() {
        instance = this;
        nations = new List<Nation>() { new Nation() };
        nationCityNameLists.Add(new List<string>() {"Avesta", "Partollin", "Loadae", "Tarvoa"});
        nationCityNameLists.Add(new List<string>() {"Gupite", "Isnamaya", "Utpiya Mite", "Tarahayate"});
    }
}

public class Nation
{
    public List<City> cities = new List<City>();
}