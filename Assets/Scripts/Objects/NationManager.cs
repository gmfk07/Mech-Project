using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NationManager : MonoBehaviour
{
    public static NationManager instance;
    public List<Nation> nations = new List<Nation>();

    public void Start() {
        instance = this;
        nations = new List<Nation>() { new Nation() };
    }
}

public class Nation
{
    public List<City> cities = new List<City>();
}