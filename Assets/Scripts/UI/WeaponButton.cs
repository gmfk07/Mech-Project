using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    private Weapon weapon;
    public Weapon Weapon { get {return weapon;} set{weapon = value; InitializeButtonText();} }

    public void SetVisibility(bool visibility)
    {
        GetComponent<Button>().enabled = visibility;
        buttonText.enabled = visibility;
    }

    public void InitializeButtonText()
    {
        string accuracyDiceString = (2 + weapon.accuracyDice) + "D6";
        string accuracyModString = weapon.accuracyMod == 0 ? "" : "+" + weapon.accuracyMod;
        string damageDiceString = (2 + weapon.damageDice) + "D6";
        string damageModString = weapon.damageMod == 0 ? "" : "+" + weapon.damageMod;
        buttonText.text = weapon.name + "\nACC: " + accuracyDiceString + accuracyModString + "\nDMG: " + damageDiceString + damageModString;
    }
}
