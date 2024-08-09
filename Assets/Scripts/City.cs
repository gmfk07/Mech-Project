using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using UnityEngine;

public class City : Object
{
    [SerializeField] private int borderDistance;
    [SerializeField] private Material borderMaterialLand;
    [SerializeField] private Material borderMaterialWater;

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
            }
            hexa.SetTileMaterial(tileIndex, borderMaterialWater, false);
        }
    }
}
