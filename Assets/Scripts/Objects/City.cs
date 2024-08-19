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
    [SerializeField] private Material borderMaterialLand;
    [SerializeField] private Material borderMaterialWater;
    [HideInInspector] public List<int> tilesWithinBorders { get; private set; } = new List<int>();
    [HideInInspector] public List<CitySubObject> citySubObjects = new List<CitySubObject>();
    private int population = 1;
    private WorldPositionButton cityButton;
    [HideInInspector] public String cityName;
    private Dictionary<Resource, int> resourceProductionDict = new Dictionary<Resource, int>();

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        PaintBorders();
        StartCoroutine(CreateCityButton());
    }

    IEnumerator CreateCityButton()
    {
        while (HUDCanvas.instance == null)
        {
            yield return null;
        }
        cityButton = HUDCanvas.instance.CreateCityButton(this);
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
            //Make a path to the city and see if it's short enough
            List<int> path = hexa.FindPath(tileIndex, tile.index, 0, -1, true);
            if (path != null && path.Count <= borderDistance)
            {
                if (WorldGenerator.instance.waterTiles.Contains(tile.index))
                {
                    hexa.SetTileMaterial(tile.index, borderMaterialWater, false);
                }
                else
                {
                    hexa.SetTileMaterial(tile.index, borderMaterialLand, false);
                }
                tilesWithinBorders.Add(tile.index);
            }
            hexa.SetTileMaterial(tileIndex, borderMaterialLand, false);
        }
    }

    public void HandleClicked()
    {
        hexa.FlyTo(tileIndex, 0.5f);
        HUDCanvas.instance.SetSelectedCity(this);
        HUDCanvas.instance.SetCityPanelVisible(true);
    }
}
