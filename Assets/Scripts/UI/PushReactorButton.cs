using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PushReactorButton : UIButton
{
    void Start()
    {
        InitializeButtonText();
    }

    public void InitializeButtonText()
    {
        buttonText.text = "Push Reactor\n-" + ObjectManager.instance.selectedUnit.pushReactorCost + "RP";
    }

    public override void OnButtonPressed()
    {
        ObjectManager.instance.PushSelectedUnitReactor();
    }
}
