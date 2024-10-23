using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildButton : UIButton
{
    private Building building;
    public Building Building { get {return building;} set{building = value; InitializeButtonText();} }

    public void InitializeButtonText()
    {
        buttonText.text = "Build " + building.name;
    }

    public override void OnButtonPressed()
    {
        BuilderUnit builderUnit = (BuilderUnit) ObjectManager.instance.selectedUnit;
        builderUnit.BuildSubObject(building.citySubObjectPrefab);
        builderUnit.HandleActionPerformed();
    }
}
