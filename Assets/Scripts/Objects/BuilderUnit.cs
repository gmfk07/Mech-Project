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
                BuildMine();
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

    void BuildMine()
    {
        CitySubObject mine = Instantiate(minePrefab).GetComponent<CitySubObject>();
        mine.tileIndex = tileIndex;

        mine.transform.SetParent(hexa.transform);
        mine.transform.position = hexa.GetTileCenter(tileIndex);

        City owningCity = FindOwningCity(tileIndex);
        owningCity.citySubObjects.Add(mine);
    }
}
