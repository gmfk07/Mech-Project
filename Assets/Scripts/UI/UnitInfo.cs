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

    void UpdateText()
    {
        Unit selectedUnit = ObjectManager.instance.selectedUnit;
        unitName.text = selectedUnit.unitName;
        unitHp.text = "HP: " + selectedUnit.hp + "/" + selectedUnit.maxHp;
        unitRp.text = "RP: " + selectedUnit.rp + "/" + selectedUnit.maxRp;
    }

    public void SetVisibility(bool visibility)
    {
        if (visibility)
        {
            unitName.enabled = true;
            unitHp.enabled = true;
            unitRp.enabled = true;
            UpdateText();
        }
        else
        {
            unitName.enabled = false;
            unitHp.enabled = false;
            unitRp.enabled = false;
        }
    }
}
