using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexasphereGrid;

enum PaintMode { None, Unit, City }

public class ObjectManager : MonoBehaviour
{
    Hexasphere hexa;

    public static ObjectManager instance;

    [SerializeField] GameObject unitPrefab;
    [SerializeField] GameObject cityPrefab;

    Dictionary<int, Unit> tileUnitDict = new Dictionary<int, Unit>();
    Dictionary<int, City> tileCityDict = new Dictionary<int, City>();
    public Dictionary<int, List<Unit>> playerUnitDict = new Dictionary<int, List<Unit>>();

    [HideInInspector] public Unit selectedUnit = null;
    PaintMode paintMode = PaintMode.None;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        hexa = Hexasphere.GetInstance("Hexasphere");
        hexa.OnTileClick += TileClick;
        hexa.OnTileRightClick += TileRightClick;
        hexa.OnTileMouseOver += TileMouseOver;
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 220, 30), "Paint None"))
        {
            paintMode = PaintMode.None;
        }
        if (GUI.Button(new Rect(10, 50, 220, 30), "Paint Unit"))
        {
            paintMode = PaintMode.Unit;
        }
        if (GUI.Button(new Rect(10, 90, 220, 30), "Paint City"))
        {
            paintMode = PaintMode.City;
        }
    }

    public void HandleNewTurn()
    {
        foreach (Unit unit in tileUnitDict.Values)
        {
            unit.HandleNewTurn();
        }
    }

    public void GetNextActiveUnit()
    {

    }

    void TileMouseOver(Hexasphere hexa, int tileIndex)
    {
        hexa.highlightEnabled = false;
        hexa.ClearTiles(true, false, false);

        if (selectedUnit != null)
        {
            hexa.highlightEnabled = true;
            List<int> path = hexa.FindPath(selectedUnit.tileIndex, tileIndex, 0, -1, false);
            if (path != null && path.Count > 0)
            {
                path.RemoveAt(path.Count - 1);
                path.Add(selectedUnit.tileIndex);
                if (path.Count <= selectedUnit.remainingMoves)
                {
                    hexa.SetTileColor(path, Color.white, true);
                }
                else
                {
                    hexa.SetTileColor(path, Color.red, true);
                }
            }
        }
    }

    void TileClick(Hexasphere hexa, int tileIndex)
    {
        if (paintMode == PaintMode.Unit)
        {
            if (!tileUnitDict.ContainsKey(tileIndex) && hexa.GetTileCanCross(tileIndex))
            {
                // Create the tile prefab
                BuilderUnit unit = Instantiate(unitPrefab).GetComponent<BuilderUnit>();
                tileUnitDict.Add(tileIndex, unit);
                unit.tileIndex = tileIndex;

                // Parent it to hexasphere, so it rotates along it
                unit.transform.SetParent(hexa.transform);

                // Position capsule on top of tile
                unit.transform.position = hexa.GetTileCenter(tileIndex);

                hexa.FlyTo(tileIndex, 0.5f);

                //Add to player's dict
                if (playerUnitDict.ContainsKey(0))
                {
                    playerUnitDict[0].Add(unit);
                }
                else
                {
                    playerUnitDict.Add(0, new List<Unit>() {unit});
                }
                selectedUnit = unit;
                unit.active = true;
                UIManager.instance.UpdateMainButton();
            }
            else if (tileUnitDict.ContainsKey(tileIndex))
            {
                selectedUnit = tileUnitDict[tileIndex].GetComponent<Unit>();
                hexa.FlyTo(tileIndex, 0.5f);
            }
        }
        else if (paintMode == PaintMode.City)
        {
            if (!tileCityDict.ContainsKey(tileIndex) && hexa.GetTileCanCross(tileIndex))
            {
                // Create the tile sprite
                City city = Instantiate(cityPrefab).GetComponent<City>();
                tileCityDict.Add(tileIndex, city);
                city.tileIndex = tileIndex;

                // Parent it to hexasphere, so it rotates along it
                city.transform.SetParent(hexa.transform);

                // Position capsule on top of tile
                city.transform.position = hexa.GetTileCenter(tileIndex);

                NationManager.instance.nations[TurnManager.instance.currentPlayer].cities.Add(city);

                hexa.FlyTo(tileIndex, 0.5f);
            }
            else
            {
                hexa.FlyTo(tileIndex, 0.5f);
            }
        }
        else if (paintMode == PaintMode.None)
        {
            if (tileUnitDict.ContainsKey(tileIndex))
            {
                selectedUnit = tileUnitDict[tileIndex].GetComponent<Unit>();
            }
            else
            {
                selectedUnit = null;
            }

            hexa.FlyTo(tileIndex, 0.5f);
        }
    }

    void TileRightClick(Hexasphere hexa, int tileIndex)
    {
        if (selectedUnit != null && !selectedUnit.IsMoving() && !tileUnitDict.ContainsKey(tileIndex))
        {
            List<int> path = hexa.FindPath(selectedUnit.tileIndex, tileIndex, 0, -1, false);
            if (path != null && path.Count > 0 && path.Count <= selectedUnit.remainingMoves)
            {
                //Path is valid and within range
                selectedUnit.path = path;
                selectedUnit.StopAllCoroutines();
                selectedUnit.StartCoroutine(selectedUnit.MoveUnit());

                tileUnitDict.Remove(selectedUnit.tileIndex);
                selectedUnit.tileIndex = tileIndex;
                tileUnitDict[tileIndex] = selectedUnit;

                selectedUnit.remainingMoves -= path.Count;
                if (selectedUnit.remainingMoves == 0)
                {
                    selectedUnit.active = false;
                    UIManager.instance.UpdateMainButton();
                }

                hexa.FlyTo(tileIndex, 0.5f);
            }
        }
    }
}
