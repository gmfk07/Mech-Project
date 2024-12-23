using System;
using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class City : Object
{
    [SerializeField] private int borderDistance;
    [HideInInspector] public Material landBorderMaterial;
    [HideInInspector] public Material waterBorderMaterial;
    //A tile can be owned by multiple cities, but all cities owning a tile must be the same player's.
    [HideInInspector] public List<int> tilesWithinBorders { get; private set; } = new List<int>();
    //Contains all subobjects in borders, not just those assigned to the city. For that, check the subObjects' owner
    [HideInInspector] public List<CitySubObject> citySubObjects = new List<CitySubObject>();
    private List<Pop> pops = new List<Pop>() { new Pop(1, 1), new Pop(1, 1) };
    private List<int> availablePopIndices = new List<int> { 0, 1 };
    private WorldPositionButton cityButton;
    [HideInInspector] public string cityName;
    [HideInInspector] public Dictionary<Resource, int> resourceProductionDict = new Dictionary<Resource, int>();
    [HideInInspector] public Nation owningNation;
    [SerializeField] private Resource recruitmentResource;
    public Recruitable recruiting;
    private int savedRecruitment = 0;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        landBorderMaterial = owningNation.landBorderMaterial;
        waterBorderMaterial = owningNation.waterBorderMaterial;
        PaintBorders();
        StartCoroutine(CreateCityButton());
        citySubObjects = ObjectManager.instance.GetSubObjectsInCityBorders(this);
    }

    IEnumerator CreateCityButton()
    {
        while (UICanvas.instance == null)
        {
            yield return null;
        }
        cityButton = UICanvas.instance.CreateCityButton(this);
        ChangeName(cityName);
    }

    public void ChangeName(String newName)
    {
        cityName = newName;
        if (cityButton != null)
        {
            cityButton.GetComponentInChildren<TextMeshProUGUI>().text = newName;
        }
    }

    //Fill in borders and add any CitySubObjects within borders to citySubObject
    void PaintBorders()
    {
        foreach (Tile tile in hexa.tiles)
        {
            //Is the border tile already taken by another player?
            bool tileTaken = false;
            foreach (Nation nation in NationManager.instance.nations)
            {
                foreach (City city in nation.cities)
                {
                    if (city.tilesWithinBorders.Contains(tile.index) && city.owningNation.player != TurnManager.instance.currentPlayer)
                    {
                        tileTaken = true;
                    }
                }
            }
            if (tileTaken)
            {
                continue;
            }
            //Make a path to the city and see if it's short enough
            List<int> path = hexa.FindPath(tileIndex, tile.index, 0, -1, true);
            if (path != null && path.Count <= borderDistance && !WorldGenerator.instance.iceTiles.Contains(tile.index))
            {
                if (WorldGenerator.instance.waterTiles.Contains(tile.index))
                {
                    hexa.SetTileMaterial(tile.index, waterBorderMaterial, false);
                }
                else
                {
                    hexa.SetTileMaterial(tile.index, landBorderMaterial, false);
                }
                tilesWithinBorders.Add(tile.index);
                if (ObjectManager.instance.tileCitySubObjectDict.ContainsKey(tileIndex))
                {
                    citySubObjects.Add(ObjectManager.instance.tileCitySubObjectDict[tileIndex]);
                }
            }
        }
        hexa.SetTileMaterial(tileIndex, landBorderMaterial, false);
    }

    public void HandleClicked()
    {
        UICanvas.instance.SetCityPanelVisible(false);
        hexa.FlyTo(tileIndex, 0.5f);
        UICanvas.instance.SetCityPanelSelectedCity(this);
        UICanvas.instance.SetCityPanelVisible(true);
    }

    //Sets a pop to be available for city use or occupied.
    public void SetPopAvailable(int index, bool available)
    {
        if (!available && availablePopIndices.Contains(index))
        {
            availablePopIndices.Remove(index);
        }
        else if (available && !availablePopIndices.Contains(index))
        {
            availablePopIndices.Add(index);
        }
    }

    public int GetAvailablePopCount()
    {
        return availablePopIndices.Count;
    }

    public Pop GetPop(int index)
    {
        return pops[index];
    }

    public void ProduceResources()
    {
        foreach (Resource resource in resourceProductionDict.Keys)
        {
            if (resource.global)
            {
                NationManager.instance.HandleNationResourceChange(owningNation.player, resource, resourceProductionDict[resource]);
            }
            else
            {
                if (resource == recruitmentResource)
                {
                    savedRecruitment += resourceProductionDict[resource];
                }
            }
        }
    }

    public void TryRecruit()
    {
        if (recruiting)
        {
            bool canRecruit = true;
            for (int i=0; i < recruiting.costResourceList.Count; i++)
            {
                Resource resource = recruiting.costResourceList[i];
                int cost = recruiting.costValueList[i];
                if (!resource.global)
                {
                    if (resource == recruitmentResource && savedRecruitment < cost)
                    {
                        canRecruit = false;
                    }
                }
                else if (NationManager.instance.GetNationResourceCount(TurnManager.instance.currentPlayer, resource) < cost)
                {
                    canRecruit = false;
                }
            }

            if (canRecruit)
            {
                for (int i=0; i < recruiting.costResourceList.Count; i++)
                {
                    Resource resource = recruiting.costResourceList[i];
                    int cost = recruiting.costValueList[i];
                    if (!resource.global)
                    {
                        if (resource == recruitmentResource)
                        {
                            savedRecruitment -= cost;
                        }
                    }
                    else
                    {
                        NationManager.instance.HandleNationResourceChange(TurnManager.instance.currentPlayer, resource, -cost);
                    }
                }
                ObjectManager.instance.CreateUnitFromRecruitable(tileIndex, recruiting);
            }
        }
    }

    public void SetRecruiting(Recruitable recruiting)
    {
        this.recruiting = recruiting;
        UICanvas.instance.UpdateCityPanelRecruiting();
    }
}

public class Pop
{
    public int wealth;
    public float comfort;
    public CitySubObject working = null;

    public Pop(int wealth, float comfort)
    {
        this.wealth = wealth;
        this.comfort = comfort;
    }
}