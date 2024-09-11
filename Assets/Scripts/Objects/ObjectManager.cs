using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexasphereGrid;
using Unity.PlasticSCM.Editor.WebApi;

enum PaintMode { None, BuilderUnit, BattlerUnit, City }

public class ObjectManager : MonoBehaviour
{
    Hexasphere hexa;

    public static ObjectManager instance;

    [SerializeField] GameObject builderUnitPrefab;
    [SerializeField] GameObject battlerUnitPrefab;
    [SerializeField] GameObject cityPrefab;

    Dictionary<int, Unit> tileUnitDict = new Dictionary<int, Unit>();
    Dictionary<int, City> tileCityDict = new Dictionary<int, City>();
    public Dictionary<int, CitySubObject> tileCitySubObjectDict = new Dictionary<int, CitySubObject>();
    public Dictionary<int, List<Unit>> playerUnitDict = new Dictionary<int, List<Unit>>();

    [HideInInspector] public Unit selectedUnit = null;
    PaintMode paintMode = PaintMode.None;
    private bool targeting = false;
    [SerializeField] private Texture2D targetTexture;

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
        if (GUI.Button(new Rect(850, 10, 220, 30), "Paint None"))
        {
            paintMode = PaintMode.None;
        }
        if (GUI.Button(new Rect(850, 50, 220, 30), "Paint Builder Unit"))
        {
            paintMode = PaintMode.BuilderUnit;
        }
        if (GUI.Button(new Rect(850, 90, 220, 30), "Paint Battler Unit"))
        {
            paintMode = PaintMode.BattlerUnit;
        }
        if (GUI.Button(new Rect(850, 130, 220, 30), "Paint City"))
        {
            paintMode = PaintMode.City;
        }
    }

    public void HandleNewTurn()
    {
        selectedUnit = null;
        foreach (Unit unit in tileUnitDict.Values)
        {
            if (playerUnitDict.ContainsKey(TurnManager.instance.currentPlayer) && playerUnitDict[TurnManager.instance.currentPlayer].Contains(unit))
            {
                unit.RefreshMoves();
            }
        }
    }

    public List<CitySubObject> GetSubObjectsInCityBorders(City city)
    {
        List<CitySubObject> citySubObjects = new List<CitySubObject>();
        foreach (int tile in city.tilesWithinBorders)
        {
            if (tileCitySubObjectDict.ContainsKey(tile))
            {
                citySubObjects.Add(tileCitySubObjectDict[tile]);
            }
        }
        return citySubObjects;
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
        //Paint Unit
        if (paintMode == PaintMode.BuilderUnit || paintMode == PaintMode.BattlerUnit)
        {
            if (!tileUnitDict.ContainsKey(tileIndex) && hexa.GetTileCanCross(tileIndex))
            {
                // Create the tile prefab
                Unit unit = null;
                if (paintMode == PaintMode.BuilderUnit)
                {
                    unit = Instantiate(builderUnitPrefab).GetComponent<BuilderUnit>();
                }
                else if (paintMode == PaintMode.BattlerUnit)
                {
                    unit = Instantiate(battlerUnitPrefab).GetComponent<BattlerUnit>();
                }
                tileUnitDict.Add(tileIndex, unit);
                int currentPlayer = TurnManager.instance.currentPlayer;
                unit.tileIndex = tileIndex;
                unit.owningNation = NationManager.instance.nations[currentPlayer];

                // Parent it to hexasphere, so it rotates along it
                unit.transform.SetParent(hexa.transform);

                // Position capsule on top of tile
                unit.transform.position = hexa.GetTileCenter(tileIndex);

                hexa.FlyTo(tileIndex, 0.5f);

                //Add to player's dict
                if (playerUnitDict.ContainsKey(currentPlayer))
                {
                    playerUnitDict[currentPlayer].Add(unit);
                }
                else
                {
                    playerUnitDict.Add(currentPlayer, new List<Unit>() {unit});
                }
                selectedUnit = unit;
                unit.active = true;
                UICanvas.instance.UpdateMainButton();
                UICanvas.instance.HandleUnitSelected();
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
                // Create the tile prefab
                City city = Instantiate(cityPrefab).GetComponent<City>();
                tileCityDict.Add(tileIndex, city);
                city.tileIndex = tileIndex;

                // Parent it to hexasphere, so it rotates along it
                city.transform.SetParent(hexa.transform);

                // Position capsule on top of tile
                city.transform.position = hexa.GetTileCenter(tileIndex);

                // City
                city.owningNation = NationManager.instance.nations[TurnManager.instance.currentPlayer];
                List<City> currentCities = city.owningNation.cities;
                currentCities.Add(city);
                city.ChangeName(NationManager.instance.nationCityNameLists[TurnManager.instance.currentPlayer][currentCities.Count - 1]);

                hexa.FlyTo(tileIndex, 0.5f);
            }
            else
            {
                hexa.FlyTo(tileIndex, 0.5f);
            }
        }
        else if (paintMode == PaintMode.None)
        {
            HandleTileSelected(tileIndex);
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
                    UICanvas.instance.UpdateMainButton();
                }

                hexa.FlyTo(tileIndex, 0.5f);
            }
        }
    }

    public void HandleTileSelected(int tileIndex)
    {
        if (targeting)
        {
            if (tileUnitDict.ContainsKey(tileIndex) && !playerUnitDict[TurnManager.instance.currentPlayer].Contains(tileUnitDict[tileIndex]))
            {
                Unit targetUnit = tileUnitDict[tileIndex].GetComponent<Unit>();
                BattlerUnit attackerUnit = (BattlerUnit)selectedUnit;
                attackerUnit.AttackTargets(new List<Unit>() { targetUnit });
            }
            StopTargeting();
        }
        else
        {
            if (tileUnitDict.ContainsKey(tileIndex) && playerUnitDict[TurnManager.instance.currentPlayer].Contains(tileUnitDict[tileIndex]))
            {
                selectedUnit = tileUnitDict[tileIndex].GetComponent<Unit>();
                UICanvas.instance.HandleUnitSelected();
            }
            else
            {
                selectedUnit = null;
                UICanvas.instance.HandleUnitDeselected();
            }
        }

        hexa.FlyTo(tileIndex, 0.5f);
    }

    //NOTE: Targeting is only valid if the current selectedUnit is a BattlerUnit! 
    public void StartTargeting()
    {
        targeting = true;
        Cursor.SetCursor(targetTexture, Vector2.zero, CursorMode.Auto);
    }

    public void StopTargeting()
    {
        targeting = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
