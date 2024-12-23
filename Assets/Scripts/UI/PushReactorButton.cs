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
        ColossusUnit selectedColossusUnit = (ColossusUnit) ObjectManager.instance.selectedUnit;
        buttonText.text = "Push Reactor\n-" + selectedColossusUnit.pushReactorCost + "RP";
    }

    public override void OnButtonPressed()
    {
        ObjectManager.instance.PushSelectedUnitReactor();
    }
}
