using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexasphereGrid;
using Unity.VisualScripting;
using System.Linq;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator instance;

    Hexasphere hexa;
    [SerializeField] private Material waterMaterial;
    [SerializeField] private float waterLevel;
    [SerializeField] private float forestCutoff;
    [SerializeField] private int oreCount;
    [SerializeField] private float heightScale;
    [SerializeField] private GameObject forestPrefab;
    [SerializeField] private GameObject orePrefab;
    [HideInInspector] public List<int> waterTiles;
    [HideInInspector] public List<int> forestTiles;
    [HideInInspector] public List<int> oreTiles;  

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
            }
        }

        List<Tile> shuffledTiles = hexa.tiles.ToList();
        IListExtensions.Shuffle<Tile>(shuffledTiles);
        //Generate ores
        int currentOres = 0;
        foreach (Tile tile in shuffledTiles)
        {
            if (currentOres <= oreCount && !waterTiles.Contains(tile.index))
            {
                // Create the tile prefab
                GameObject oreObject = Instantiate(orePrefab);
                oreTiles.Add(tile.index);

                // Parent it to hexasphere, so it rotates along it
                oreObject.transform.SetParent(hexa.transform);

                // Position forest on top of tile
                oreObject.transform.position = hexa.GetTileCenter(tile.index);

                oreObject.transform.LookAt(hexa.transform.position);
                oreObject.transform.Rotate(-90, 0, 0, Space.Self);

                currentOres++;
            }
        }

        //Generate forests
        noiseOffsetX = Random.Range(0, 1000);
        noiseOffsetY = Random.Range(0, 1000);
        foreach (Tile tile in hexa.tiles)
        {
            Vector2 LatLon = hexa.GetTileLatLon(tile.index);
            float sample = noise.GetNoise(LatLon.x + noiseOffsetX, LatLon.y + noiseOffsetY);
            if (sample <= forestCutoff && !waterTiles.Contains(tile.index) && !oreTiles.Contains(tile.index))
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
