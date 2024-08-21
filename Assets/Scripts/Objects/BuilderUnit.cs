using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using UnityEngine;

public class BuilderUnit : Unit
{
    [SerializeField] private GameObject minePrefab;

    void Update()
    {
        if (!IsMoving() && Input.GetButtonDown("Build"))
        {
            if (WorldGenerator.instance.oreTiles.Contains(tileIndex) && FindOwningCity(tileIndex) != null)
            {
                BuildSubObject(minePrefab);
            }
        }
    }

    City FindOwningCity(int tileIndex)
    {
        foreach (Nation nation in NationManager.instance.nations)
        {
            foreach (City city in nation.cities)
            {
                if (city.tilesWithinBorders.Contains(tileIndex)) { return city; }
            }
        }
        return null;
    }

    void BuildSubObject(GameObject prefab)
    {
        CitySubObject citySubObject = Instantiate(prefab).GetComponent<CitySubObject>();
        citySubObject.tileIndex = tileIndex;

        citySubObject.transform.SetParent(hexa.transform);
        citySubObject.transform.position = hexa.GetTileCenter(tileIndex);

        City owningCity = FindOwningCity(tileIndex);
        owningCity.citySubObjects.Add(citySubObject);
        citySubObject.owner = owningCity;
    }
}
