using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NationManager : MonoBehaviour
{
    public static NationManager instance;
    public List<Nation> nations = new List<Nation>();
    public List<List<String>> nationCityNameLists = new List<List<string>>();

    public void Start() {
        instance = this;
        nations = new List<Nation>() { new Nation() };
        nationCityNameLists.Add(new List<string>() {"Gupite", "Avesta", "Partollin"});
    }
}

public class Nation
{
    public List<City> cities = new List<City>();
}