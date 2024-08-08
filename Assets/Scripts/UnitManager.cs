using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexasphereGrid;

public class UnitManager : MonoBehaviour
{
    Hexasphere hexa;

    [SerializeField] GameObject unitPrefab;

    Dictionary<int, Unit> tileUnitDict = new Dictionary<int, Unit>();

    Unit selectedUnit = null;

    // Start is called before the first frame update
    void Start()
    {
        hexa = Hexasphere.GetInstance("Hexasphere");
        hexa.OnTileClick += TileClick;
        hexa.OnTileRightClick += TileRightClick;
    }

    void TileClick(Hexasphere hexa, int tileIndex)
    {
        if (!tileUnitDict.ContainsKey(tileIndex))
        {
            // Create the tile sprite
            Unit unit = Instantiate(unitPrefab).GetComponent<Unit>();
            tileUnitDict.Add(tileIndex, unit);
            unit.tileIndex = tileIndex;

            // Parent it to hexasphere, so it rotates along it
            unit.transform.SetParent(hexa.transform);

            // Position capsule on top of tile
            unit.transform.position = hexa.GetTileCenter(tileIndex);
        }
        else
        {
            selectedUnit = tileUnitDict[tileIndex].GetComponent<Unit>();
        }
        // Visualize capsule
        hexa.FlyTo(tileIndex, 0.5f);
    }

    void TileRightClick(Hexasphere hexa, int tileIndex)
    {
        if (selectedUnit != null && !selectedUnit.IsMoving())
        {
            List<int> path = hexa.FindPath(selectedUnit.tileIndex, tileIndex);
            selectedUnit.path = path;
            selectedUnit.StopAllCoroutines();
            selectedUnit.StartCoroutine(selectedUnit.MoveUnit());

            tileUnitDict.Remove(selectedUnit.tileIndex);
            selectedUnit.tileIndex = tileIndex;
            tileUnitDict[tileIndex] = selectedUnit;
        }
        // Visualize unit
        hexa.FlyTo(tileIndex, 0.5f);
    }
}
