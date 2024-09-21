using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkipButton : UIButton
{
    public override void OnButtonPressed()
    {
        ObjectManager.instance.SkipSelectedUnitTurn();
    }
}
