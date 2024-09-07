using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Weapon", order = 2)]
public class Weapon : ScriptableObject
{
    public int accuracyDice;
    public int accuracyMod;
    public int damageDice;
    public int damageMod;
}
