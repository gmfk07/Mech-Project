using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;
    [HideInInspector] public int currentPlayer = -1;

    void Start()
    {
        instance = this;
        currentPlayer = -1;
        HandleNewTurn();
    }

    public void HandleNewTurn()
    {
        currentPlayer = (currentPlayer + 1) % NationManager.instance.nationCount;
        ObjectManager.instance.HandleNewTurn();
        UICanvas.instance.UpdateStockpileResourceList();
        if (ObjectManager.instance.playerUnitDict.ContainsKey(currentPlayer))
        {
            List<ColossusUnit> toMeltdown = new List<ColossusUnit>();
            foreach (Unit unit in ObjectManager.instance.playerUnitDict[currentPlayer])
            {
                if (unit is ColossusUnit)
                {
                    ColossusUnit colossusUnit = (ColossusUnit) unit;
                    if (colossusUnit.ShouldMeltdown())
                    {
                        toMeltdown.Add(colossusUnit);
                    }
                }
            }

            foreach (ColossusUnit colossusUnit in toMeltdown)
            {
                colossusUnit.DestroyUnit();
            }
        }
        foreach (City city in NationManager.instance.nationCityLists[currentPlayer])
        {
            city.ProduceResources();
        }
        foreach (City city in NationManager.instance.nationCityLists[currentPlayer])
        {
            city.TryRecruit();
        }
        ObjectManager.instance.DeselectUnit();
    }
}
