using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexasphereGrid;

public class WorldGenerator : MonoBehaviour
{
    Hexasphere hexa;
    [SerializeField] private float waterLevel;

    // Start is called before the first frame update
    void Start()
    {
        hexa = Hexasphere.GetInstance("Hexasphere");

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
                hexa.SetTileColor(tile.index, new Color(0, 85, 245));
                hexa.SetTileCanCross(tile.index, false);
            }
            else
            {
                hexa.SetTileColor(tile.index, new Color(sample, sample, sample));
                hexa.SetTileCanCross(tile.index, true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
