using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
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

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        PaintBorders();
    }

    void PaintBorders()
    {
        foreach (Tile tile in hexa.tiles)
        {
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
}
