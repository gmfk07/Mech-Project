using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexasphereGrid;
using Unity.VisualScripting;
using System.Linq;
using System.Globalization;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator instance;

    Hexasphere hexa;
    [SerializeField] private Material groundMaterial;
    [SerializeField] private Material waterMaterial;
    [SerializeField] private Material iceMaterial;
    [SerializeField] private float waterLevel;
    [SerializeField] private float forestCutoff;
    [SerializeField] private int metalOreCount;
    [SerializeField] private int rareEarthOreCount;
    [SerializeField] private float heightScale;
    [SerializeField] private GameObject forestPrefab;
    [SerializeField] private GameObject metalOrePrefab;
    [SerializeField] private GameObject rareEarthOrePrefab;
    [SerializeField] private int poleRadius;
    [HideInInspector] public List<int> waterTiles;
    [HideInInspector] public List<int> iceTiles;
    [HideInInspector] public List<int> forestTiles;
    [HideInInspector] public List<int> metalOreTiles;
    [HideInInspector] public List<int> rareEarthOreTiles;
    [HideInInspector] public int northPoleTile;
    [HideInInspector] public int southPoleTile;
    [SerializeField] private Texture2D grassTexture;

    // Start is called before the first frame update
    void Start()
    {
        hexa = Hexasphere.GetInstance("Hexasphere");
        instance = this;

        float noiseOffsetX = Random.Range(0, 1000);
        float noiseOffsetY = Random.Range(0, 1000);
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        //Generate tile height
        foreach (Tile tile in hexa.tiles)
        {
            Vector2 LatLon = hexa.GetTileLatLon(tile.index);
            float sample = noise.GetNoise(LatLon.x + noiseOffsetX, LatLon.y + noiseOffsetY);
            if  (sample <= waterLevel)
            {
                hexa.SetTileMaterial(tile.index, waterMaterial, false);
                hexa.SetTileCanCross(tile.index, false);
                hexa.SetTileExtrudeAmount(tile.index, 0);
                waterTiles.Add(tile.index);
            }
            else
            {
                hexa.SetTileExtrudeAmount(tile.index, (sample - waterLevel)*heightScale);
                hexa.SetTileCanCross(tile.index, true);
                hexa.SetTileTexture(tile.index, grassTexture);
                hexa.SetTileMaterial(tile.index, groundMaterial);
            }
        }

        //Generate poles
        northPoleTile = hexa.tiles[0].index;
        southPoleTile = hexa.GetTileAtPolarOpposite(northPoleTile);
        hexa.SetTileMaterial(northPoleTile, iceMaterial);
        hexa.SetTileMaterial(southPoleTile, iceMaterial);
        foreach (int tileIndex in hexa.GetTilesWithinSteps(northPoleTile, poleRadius+1, false))
        {
            hexa.SetTileMaterial(tileIndex, waterMaterial);
            hexa.SetTileCanCross(tileIndex, false);
            hexa.SetTileExtrudeAmount(tileIndex, 0);
            waterTiles.Add(tileIndex);
        }
        foreach (int tileIndex in hexa.GetTilesWithinSteps(northPoleTile, poleRadius, false))
        {
            hexa.SetTileMaterial(tileIndex, iceMaterial);
            waterTiles.Remove(tileIndex);
            iceTiles.Add(tileIndex);
        }
        foreach (int tileIndex in hexa.GetTilesWithinSteps(southPoleTile, poleRadius+1, false))
        {
            hexa.SetTileMaterial(tileIndex, waterMaterial);
            hexa.SetTileCanCross(tileIndex, false);
            hexa.SetTileExtrudeAmount(tileIndex, 0);
            waterTiles.Add(tileIndex);
        }
        foreach (int tileIndex in hexa.GetTilesWithinSteps(southPoleTile, poleRadius, false))
        {
            hexa.SetTileMaterial(tileIndex, iceMaterial);
            waterTiles.Remove(tileIndex);
            iceTiles.Add(tileIndex);
        }

        List<Tile> shuffledTiles = hexa.tiles.ToList();
        IListExtensions.Shuffle<Tile>(shuffledTiles);
        //Generate ores
        int currentMetalOres = 0;
        foreach (Tile tile in shuffledTiles)
        {
            if (currentMetalOres <= metalOreCount && !waterTiles.Contains(tile.index) && !iceTiles.Contains(tile.index))
            {
                // Create the tile prefab
                GameObject oreObject = Instantiate(metalOrePrefab);
                metalOreTiles.Add(tile.index);

                // Parent it to hexasphere, so it rotates along it
                oreObject.transform.SetParent(hexa.transform);

                // Position forest on top of tile
                oreObject.transform.position = hexa.GetTileCenter(tile.index);

                oreObject.transform.LookAt(hexa.transform.position);
                oreObject.transform.Rotate(-90, 0, 0, Space.Self);

                currentMetalOres++;
            }
        }
        int currentRareEarthOres = 0;
        foreach (Tile tile in shuffledTiles)
        {
            if (currentRareEarthOres <= rareEarthOreCount && !metalOreTiles.Contains(tile.index) && !waterTiles.Contains(tile.index) && !iceTiles.Contains(tile.index))
            {
                // Create the tile prefab
                GameObject oreObject = Instantiate(rareEarthOrePrefab);
                rareEarthOreTiles.Add(tile.index);

                // Parent it to hexasphere, so it rotates along it
                oreObject.transform.SetParent(hexa.transform);

                // Position forest on top of tile
                oreObject.transform.position = hexa.GetTileCenter(tile.index);

                oreObject.transform.LookAt(hexa.transform.position);
                oreObject.transform.Rotate(-90, 0, 0, Space.Self);

                currentRareEarthOres++;
            }
        }

        //Generate forests
        noiseOffsetX = Random.Range(0, 1000);
        noiseOffsetY = Random.Range(0, 1000);
        foreach (Tile tile in hexa.tiles)
        {
            Vector2 LatLon = hexa.GetTileLatLon(tile.index);
            float sample = noise.GetNoise(LatLon.x + noiseOffsetX, LatLon.y + noiseOffsetY);
            if (sample <= forestCutoff && !waterTiles.Contains(tile.index) && !iceTiles.Contains(tile.index) && !metalOreTiles.Contains(tile.index) && !rareEarthOreTiles.Contains(tile.index))
            {
                // Create the tile prefab
                GameObject forestObject = Instantiate(forestPrefab);
                forestTiles.Add(tile.index);

                // Parent it to hexasphere, so it rotates along it
                forestObject.transform.SetParent(hexa.transform);

                // Position forest on top of tile
                forestObject.transform.position = hexa.GetTileCenter(tile.index);

                forestObject.transform.LookAt(hexa.transform.position);
                forestObject.transform.Rotate(-90, 0, 0, Space.Self);
            }
        }
    }
}
