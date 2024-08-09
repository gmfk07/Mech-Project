using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexasphereGrid;
using Unity.VisualScripting;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator instance;

    Hexasphere hexa;
    [SerializeField] private Material waterMaterial;
    [SerializeField] private float waterLevel;
    [SerializeField] private float heightScale;
    public List<int> waterTiles;

    // Start is called before the first frame update
    void Start()
    {
        hexa = Hexasphere.GetInstance("Hexasphere");
        instance = this;

        float noiseOffsetX = Random.Range(0, 1000);
        float noiseOffsetY = Random.Range(0, 1000);
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        foreach (Tile tile in hexa.tiles)
        {
            Vector2 LatLon = hexa.GetTileLatLon(tile.index);
            float sample = noise.GetNoise(LatLon.x + noiseOffsetX, LatLon.y + noiseOffsetY);
            if  (sample <= waterLevel)
            {
                hexa.SetTileMaterial(tile.index, waterMaterial, false);
                hexa.SetTileCanCross(tile.index, false);
                waterTiles.Add(tile.index);
            }
            else
            {
                hexa.SetTileExtrudeAmount(tile.index, (sample - waterLevel)*heightScale);
                hexa.SetTileCanCross(tile.index, true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
