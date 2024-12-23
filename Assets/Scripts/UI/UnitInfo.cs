using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UnitInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI unitName;
    [SerializeField] private TextMeshProUGUI unitHp;
    [SerializeField] private TextMeshProUGUI unitRp;
    [SerializeField] private TextMeshProUGUI unitMv;

    public void UpdateText()
    {
        Unit selectedUnit = ObjectManager.instance.selectedUnit;
        unitName.text = selectedUnit.unitName;
        unitHp.text = "HP: " + selectedUnit.hp + "/" + selectedUnit.maxHp;
        if (selectedUnit is ColossusUnit)
        {
            ColossusUnit selectedColossusUnit = (ColossusUnit) ObjectManager.instance.selectedUnit;
            unitRp.text = "RP: " + selectedColossusUnit.rp + "/" + selectedColossusUnit.maxRp;
        }
        else
        {
            unitRp.text = "";
        }
        unitMv.text = "MV: " + selectedUnit.remainingMoves + "/" + selectedUnit.moveRange;
    }

    public void SetVisibility(bool visibility)
    {
        if (visibility)
        {
            unitName.enabled = true;
            unitHp.enabled = true;
            unitRp.enabled = true;
            unitMv.enabled = true;
            UpdateText();
        }
        else
        {
            unitName.enabled = false;
            unitHp.enabled = false;
            unitRp.enabled = false;
            unitMv.enabled = false;
        }
    }
}
