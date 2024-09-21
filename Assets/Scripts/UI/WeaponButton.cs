using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponButton : UIButton
{
    private Weapon weapon;
    public Weapon Weapon { get {return weapon;} set{weapon = value; InitializeButtonText();} }

    public void InitializeButtonText()
    {
        string accuracyDiceString = (2 + weapon.accuracyDice) + "D6";
        string accuracyModString = weapon.accuracyMod == 0 ? "" : "+" + weapon.accuracyMod;
        string damageDiceString = (weapon.damageDice) + "D6";
        string damageModString = weapon.damageMod == 0 ? "" : "+" + weapon.damageMod;
        buttonText.text = weapon.name + "\nACC: " + accuracyDiceString + accuracyModString + "\nDMG: " + damageDiceString + damageModString;
    }

    public override void OnButtonPressed()
    {
        ObjectManager.instance.selectedWeapon = weapon;
        ObjectManager.instance.StartTargeting();
    }
}
