using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using UnityEngine;

public class BattlerUnit : Unit
{
    [SerializeField] private int hardpoints;
    public List<Weapon> weaponList;
    [HideInInspector] public Weapon selectedWeapon;

    override protected void Start()
    {
        base.Start();
        selectedWeapon = weaponList[0];
    }

    //Returns the result of rolling amount D6s. amount=0 will return 0.
    private int RollD6(int amount)
    {
        int result = 0;
        for (int i=0; i<amount; i++)
        {
            result += Random.Range(1, 6);
        }
        return result;
    }

    //Launch an attack against given targets using selectedWeapon.
    public void AttackTargets(List<Unit> targetUnits)
    {
        foreach (Unit unit in targetUnits)
        {
            if (RollD6(2 + selectedWeapon.accuracyDice) + selectedWeapon.accuracyMod >= unit.evasionTarget)
            {
                //weapon hit
                unit.TakeDamage(RollD6(selectedWeapon.damageDice) + selectedWeapon.damageMod);
            }
            else
            {
                //weapon miss
            }
        }
    }


}