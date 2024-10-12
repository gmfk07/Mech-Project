using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using UnityEngine;

public class BattlerUnit : Unit
{
    [SerializeField] private int hardpoints;
    public List<Weapon> weaponList;
    [HideInInspector] public Weapon selectedWeapon;
    public List<Animator> weaponAnimators;

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

    public IEnumerator Shoot(int targetTileIndex)
    {
        Vector3 localStartPositionNoExtrusion = hexa.GetTileCenter(tileIndex, worldSpace: false, includeExtrusion: false);
        Vector3 globalStartPositionNoExtrusion = hexa.transform.TransformPoint(localStartPositionNoExtrusion);
        Vector3 localStartPositionExtrusion = hexa.GetTileCenter(tileIndex, worldSpace: false);

        Vector3 localTargetPositionNoExtrusion = hexa.GetTileCenter(targetTileIndex, worldSpace: false, includeExtrusion: false);
        Vector3 globalTargetPositionNoExtrusion = hexa.transform.TransformPoint(localTargetPositionNoExtrusion);
        Vector3 localTargetPositionExtrusion = hexa.GetTileCenter(targetTileIndex, worldSpace: false);

        float rightDot = Vector3.Dot(transform.right, globalTargetPositionNoExtrusion - globalStartPositionNoExtrusion);
        float forwardDot = Vector3.Dot(transform.forward.normalized, (globalTargetPositionNoExtrusion - globalStartPositionNoExtrusion).normalized);
        float maxRightDot = 0.01f;

        while (Mathf.Abs(rightDot) > maxRightDot || forwardDot < 0)
        {
            rightDot = Vector3.Dot(transform.right, globalTargetPositionNoExtrusion - globalStartPositionNoExtrusion);
            forwardDot = Vector3.Dot(transform.forward.normalized, (globalTargetPositionNoExtrusion - globalStartPositionNoExtrusion).normalized);
            if (rightDot > 0)
            {
                unitAnimator.SetBool("turnRight", true);
                unitAnimator.SetBool("turnLeft", false);
            }
            else
            {
                unitAnimator.SetBool("turnLeft", true);
                unitAnimator.SetBool("turnRight", false);
            }
            transform.localPosition = localStartPositionExtrusion;

            yield return null;
        }

        unitAnimator.SetBool("turnLeft", false);
        unitAnimator.SetBool("turnRight", false);
        foreach (Animator animator in weaponAnimators)
        {
            animator.SetBool("shooting", true);
        }
    }
}