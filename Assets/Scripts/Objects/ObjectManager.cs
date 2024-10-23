using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexasphereGrid;
using Unity.PlasticSCM.Editor.WebApi;
using System.Linq;
using Unity.VisualScripting;
using System.IO;
using System;

enum PaintMode { None, BuilderUnit, BattlerUnit, City }

public class ObjectManager : MonoBehaviour
{
    Hexasphere hexa;

    public static ObjectManager instance;

    [SerializeField] private float targetCursorOffset;
    [SerializeField] private float heightDifferenceMoveLimit;

    [SerializeField] GameObject builderUnitPrefab;
    [SerializeField] GameObject battlerUnitPrefab;
    [SerializeField] GameObject cityPrefab;

    Dictionary<int, Unit> tileUnitDict = new Dictionary<int, Unit>();
    Dictionary<int, City> tileCityDict = new Dictionary<int, City>();
    public Dictionary<int, CitySubObject> tileCitySubObjectDict = new Dictionary<int, CitySubObject>();
    public Dictionary<int, List<Unit>> playerUnitDict = new Dictionary<int, List<Unit>>();

    [HideInInspector] public Unit selectedUnit = null;
    [HideInInspector] public Weapon selectedWeapon = null;
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
        hexa.OnPathFindingCrossTile += PathFindingCrossTileMoving;
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
                unit.RefreshActions();
            }

            //Only display unitText if the current player doesn't own the unit
            if (playerUnitDict.ContainsKey(TurnManager.instance.currentPlayer))
            {
                unit.SetUnitTargetTextVisibility(!playerUnitDict[TurnManager.instance.currentPlayer].Contains(unit));
            }
            else
            {
                unit.SetUnitTargetTextVisibility(true);
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
        hexa.highlightEnabled = true;
        hexa.ClearTiles(true, false, false);

        if (selectedUnit != null && !targeting)
        {
            hexa.highlightEnabled = false;
            List<int> path = hexa.FindPath(selectedUnit.tileIndex, tileIndex, 0, -1, false);
            if (path != null && path.Count > 0)
            {
                path.Insert(0, selectedUnit.tileIndex);
                if (CalculatePathLength(hexa, path, true) > selectedUnit.remainingMoves)
                {
                    hexa.SetTileColor(path, Color.red, true);
                }
                else
                {
                    for (int i=0; i<path.Count; i++)
                    {
                        if (i == 0)
                        {
                            hexa.SetTileColor(path[i], Color.white, true);
                        }
                        else
                        {
                            int tileCrossCost = (int) PathFindingCrossTileMoving(hexa, path[i], path[i-1]);
                            if (tileCrossCost == 1)
                            {
                                hexa.SetTileColor(path[i], Color.white, true);
                            }
                            else if (tileCrossCost == 2)
                            {
                                hexa.SetTileColor(path[i], Color.yellow, true);
                            }
                        }
                    }
                }
            }
        }
        else if (targeting)
        {
            hexa.highlightEnabled = false;
            hexa.OnPathFindingCrossTile -= PathFindingCrossTileMoving;
            List<int> path = hexa.FindPath(selectedUnit.tileIndex, tileIndex, 0, -1, true);
            path.Insert(0, selectedUnit.tileIndex);
            if (path != null)
            {
                BattlerUnit selectedBattlerUnit = (BattlerUnit) selectedUnit;
                int pathLength = (int) CalculatePathLength(hexa, path, false);
                if (pathLength < selectedBattlerUnit.selectedWeapon.minRange || pathLength > selectedWeapon.maxRange)
                {
                    hexa.SetTileColor(path, Color.red, true);
                }
                else
                {
                    hexa.SetTileColor(path, Color.white, true);
                }
                hexa.OnPathFindingCrossTile += PathFindingCrossTileMoving;
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
                UICanvas.instance.HandleUnitDeselected();
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

                // Position unit on top of tile
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
        if (targeting)
        {
            StopTargeting();
        }
        else if (selectedUnit != null && !selectedUnit.IsMoving() && !tileUnitDict.ContainsKey(tileIndex))
        {
            List<int> path = hexa.FindPath(selectedUnit.tileIndex, tileIndex, 0, -1, false);
            
            if (path != null && path.Count > 0)
            {
                List<int> pathWithStartingTile = new List<int>(path);
                pathWithStartingTile.Insert(0, selectedUnit.tileIndex);

                if (CalculatePathLength(hexa, pathWithStartingTile, true) <= selectedUnit.remainingMoves)
                {
                    //Path is valid and within range
                    selectedUnit.path = path;
                    selectedUnit.StopAllCoroutines();
                    selectedUnit.StartCoroutine(selectedUnit.MoveUnit());

                    tileUnitDict.Remove(selectedUnit.tileIndex);
                    selectedUnit.tileIndex = tileIndex;
                    tileUnitDict[tileIndex] = selectedUnit;

                    selectedUnit.remainingMoves -= (int) CalculatePathLength(hexa, pathWithStartingTile, true);
                    if (selectedUnit.remainingMoves == 0)
                    {
                        if (selectedUnit.hasActed)
                        {
                            selectedUnit.active = false;
                            UICanvas.instance.UpdateMainButton();
                        }
                    }
                    UICanvas.instance.UpdateUnitPanel();

                    hexa.FlyTo(tileIndex, 0.5f);
                }
            }
        }
    }

    public void HandleTileSelected(int tileIndex)
    {
        UICanvas.instance.SetCityPanelVisible(false);
        bool currentPlayerHasUnits = playerUnitDict.ContainsKey(TurnManager.instance.currentPlayer);
        if (targeting)
        {
            if (tileUnitDict.ContainsKey(tileIndex) && currentPlayerHasUnits && !playerUnitDict[TurnManager.instance.currentPlayer].Contains(tileUnitDict[tileIndex]))
            {
                Unit targetUnit = tileUnitDict[tileIndex].GetComponent<Unit>();
                BattlerUnit attackerUnit = (BattlerUnit)selectedUnit;
                StartCoroutine(attackerUnit.AttackTargets(new List<Unit>() { targetUnit }));
                attackerUnit.HandleActionPerformed();
            }
            StopTargeting();
        }
        else
        {
            if (tileUnitDict.ContainsKey(tileIndex) && currentPlayerHasUnits && playerUnitDict[TurnManager.instance.currentPlayer].Contains(tileUnitDict[tileIndex]))
            {
                UICanvas.instance.HandleUnitDeselected();
                selectedUnit = tileUnitDict[tileIndex].GetComponent<Unit>();
                UICanvas.instance.HandleUnitSelected();
            }
            else
            {
                DeselectUnit();
            }
        }

        hexa.FlyTo(tileIndex, 0.5f);
    }

    float PathFindingCrossTileMoving(Hexasphere hexa, int toTileIndex, int fromTileIndex)
    {
        if (Mathf.Abs(hexa.GetTileExtrudeAmount(toTileIndex) - hexa.GetTileExtrudeAmount(fromTileIndex)) > heightDifferenceMoveLimit)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }

    //Calculates the length of a path, assuming the starting tile is index 0. If moving is true, uses PathFindingCrossTileMoving for tile costs
    float CalculatePathLength(Hexasphere hexa, List<int> path, bool moving)
    {
        if (!moving)
        {
            return path.Count;
        }
        float length = 0;
        for (int i=0; i<path.Count; i++)
        {
            if (i > 0)
            {
                if (moving)
                {
                    length += PathFindingCrossTileMoving(hexa, path[i], path[i-1]);
                }
            }
        }
        return length;
    }

    //NOTE: Targeting is only valid if the current selectedUnit is a BattlerUnit! 
    public void StartTargeting()
    {
        targeting = true;
        Cursor.SetCursor(targetTexture, new Vector2(targetCursorOffset, targetCursorOffset), CursorMode.Auto);
    }

    public void StopTargeting()
    {
        targeting = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void SkipSelectedUnitTurn()
    {
        selectedUnit.active = false;
        UICanvas.instance.UpdateMainButton();
        DeselectUnit();
    }

    public void PushSelectedUnitReactor()
    {
        selectedUnit.hasActed = false;
        selectedUnit.remainingMoves = selectedUnit.moveRange;
        selectedUnit.rp = Mathf.Max(0, selectedUnit.rp - selectedUnit.pushReactorCost);
        UICanvas.instance.UpdateUnitPanel();
    }

    public void DeselectUnit()
    {
        selectedUnit = null;
        UICanvas.instance.HandleUnitDeselected();
    }

    //Removes all references to a unit about to be destroyed from ObjectManager.
    public void HandleUnitDestroyed(Unit destroyedUnit)
    {
        int tile = destroyedUnit.tileIndex;
        tileUnitDict.Remove(tile);
        playerUnitDict[destroyedUnit.owningNation.player].Remove(destroyedUnit);
    }
}
