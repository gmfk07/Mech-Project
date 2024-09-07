using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using UnityEngine;

public class BuilderUnit : Unit
{
    [SerializeField] private GameObject metalMinePrefab;
    [SerializeField] private GameObject rareEarthMinePrefab;

    void Update()
    {
        if (!IsMoving() && Input.GetButtonDown("Build") && ObjectManager.instance.selectedUnit == this)
        {
            if (WorldGenerator.instance.metalOreTiles.Contains(tileIndex) && GetOwningCities(tileIndex).Count > 0 && !ObjectManager.instance.tileCitySubObjectDict.ContainsKey(tileIndex))
            {
                BuildSubObject(metalMinePrefab);
            }
            if (WorldGenerator.instance.rareEarthOreTiles.Contains(tileIndex) && GetOwningCities(tileIndex).Count > 0 && !ObjectManager.instance.tileCitySubObjectDict.ContainsKey(tileIndex))
            {
                BuildSubObject(rareEarthMinePrefab);
            }
        }
    }

    //A tile can be owned by multiple cities, but all cities owning a tile must be the same player's.
    List<City> GetOwningCities(int tileIndex)
    {
        List<City> owningCities = new List<City>();
        foreach (Nation nation in NationManager.instance.nations)
        {
            foreach (City city in nation.cities)
            {
                if (city.tilesWithinBorders.Contains(tileIndex))
                {
                    owningCities.Add(city);
                }
            }
        }
        return owningCities;
    }

    void BuildSubObject(GameObject prefab)
    {
        CitySubObject citySubObject = Instantiate(prefab).GetComponent<CitySubObject>();
        citySubObject.tileIndex = tileIndex;

        citySubObject.transform.SetParent(hexa.transform);
        citySubObject.transform.position = hexa.GetTileCenter(tileIndex);

        List<City> owningCities = GetOwningCities(tileIndex);
        foreach (City city in owningCities)
        {
            city.citySubObjects.Add(citySubObject);
        }
        citySubObject.owningCity = owningCities[0];
        ObjectManager.instance.tileCitySubObjectDict.Add(tileIndex, citySubObject);
    }
}
