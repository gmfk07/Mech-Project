using System;
using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class City : Object
{
    [SerializeField] private int borderDistance;
    [HideInInspector] public Material landBorderMaterial;
    [HideInInspector] public Material waterBorderMaterial;
    //A tile can be owned by multiple cities, but all cities owning a tile must be the same player's.
    [HideInInspector] public List<int> tilesWithinBorders { get; private set; } = new List<int>();
    //Contains all subobjects in borders, not just those assigned to the city. For that, check the subObjects' owner
    [HideInInspector] public List<CitySubObject> citySubObjects = new List<CitySubObject>();
    private int availablePopulation = 1;
    private int totalPopulation = 1;
    private WorldPositionButton cityButton;
    [HideInInspector] public String cityName;
    [HideInInspector] public Dictionary<Resource, int> resourceProductionDict = new Dictionary<Resource, int>();
    [HideInInspector] public Nation owningNation;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        landBorderMaterial = owningNation.landBorderMaterial;
        waterBorderMaterial = owningNation.waterBorderMaterial;
        PaintBorders();
        StartCoroutine(CreateCityButton());
        UICanvas.instance.SetCityPopulationText(availablePopulation, totalPopulation);
        citySubObjects = ObjectManager.instance.GetSubObjectsInCityBorders(this);
    }

    IEnumerator CreateCityButton()
    {
        while (UICanvas.instance == null)
        {
            yield return null;
        }
        cityButton = UICanvas.instance.CreateCityButton(this);
        ChangeName(cityName);
    }

    public void ChangeName(String newName)
    {
        cityName = newName;
        if (cityButton != null)
        {
            cityButton.GetComponentInChildren<TextMeshProUGUI>().text = newName;
        }
    }

    void PaintBorders()
    {
        foreach (Tile tile in hexa.tiles)
        {
            //Is the border tile already taken by another player?
            bool tileTaken = false;
            foreach (Nation nation in NationManager.instance.nations)
            {
                foreach (City city in nation.cities)
                {
                    if (city.tilesWithinBorders.Contains(tile.index) && city.owningNation.player != TurnManager.instance.currentPlayer)
                    {
                        tileTaken = true;
                    }
                }
            }
            if (tileTaken)
            {
                continue;
            }
            //Make a path to the city and see if it's short enough
            List<int> path = hexa.FindPath(tileIndex, tile.index, 0, -1, true);
            if (path != null && path.Count <= borderDistance && !WorldGenerator.instance.iceTiles.Contains(tile.index))
            {
                if (WorldGenerator.instance.waterTiles.Contains(tile.index))
                {
                    hexa.SetTileMaterial(tile.index, waterBorderMaterial, false);
                }
                else
                {
                    hexa.SetTileMaterial(tile.index, landBorderMaterial, false);
                }
                tilesWithinBorders.Add(tile.index);
            }
        }
        hexa.SetTileMaterial(tileIndex, landBorderMaterial, false);
    }

    public void HandleClicked()
    {
        UICanvas.instance.SetCityPanelVisible(false);
        hexa.FlyTo(tileIndex, 0.5f);
        UICanvas.instance.SetSelectedCity(this);
        UICanvas.instance.SetCityPanelVisible(true);
        UICanvas.instance.SetCityPopulationText(availablePopulation, totalPopulation);
    }

    public bool HasAvailablePopulation(int amount=1)
    {
        return availablePopulation >= amount;
    }

    public void ChangeAvailablePopulation(int delta)
    {
        availablePopulation += delta;
        UICanvas.instance.SetCityPopulationText(availablePopulation, totalPopulation);
    }
}
