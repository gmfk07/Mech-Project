using System;
using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ColossusUnit : Unit
{
    [HideInInspector] public int rp;
    public int maxRp;
    public int pushReactorCost;

    override protected void Awake()
    {
        base.Awake();
        rp = maxRp;
        pushReactorCost = 1;
    }

    public bool ShouldMeltdown()
    {
        return rp == 0;
    }

    public override void RefreshActions()
    {
        base.RefreshActions();
        pushReactorCost = 1;
    }
}
