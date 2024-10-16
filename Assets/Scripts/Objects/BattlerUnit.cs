using System;
using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using TMPro;
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
            result += UnityEngine.Random.Range(1, 6);
        }
        return result;
    }

    //Launch an attack against given targets using selectedWeapon.
    public IEnumerator AttackTargets(List<Unit> targetUnits)
    {
        foreach (Unit unit in targetUnits)
        {
            yield return StartCoroutine(Shoot(unit.tileIndex, unit));
        }
    }

    public IEnumerator Shoot(int targetTileIndex, Unit targetUnit)
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

        float startTime = Time.time;
        float t = 0;

        bool attackFired = false;
        float attackFiredTime = 0.25f;
        while (t < 1f)
        {
            t = (Time.time - startTime) / selectedWeapon.shootDuration;
            if (!attackFired && t >= attackFiredTime)
            {
                MakeAttack(targetUnit);
                attackFired = true;
            }
            yield return null;
        }

        foreach (Animator animator in weaponAnimators)
        {
            animator.SetBool("shooting", false);
        }
    }
    
    //Rolls an attack and potentially damages a unit based on selectedWeapon's stats
    public void MakeAttack(Unit targetUnit)
    {
        int attackRoll = RollD6(2 + selectedWeapon.accuracyDice) + selectedWeapon.accuracyMod;
        StartCoroutine(DisplayAttackText(attackRoll));
        if (attackRoll >= targetUnit.evasionTarget)
        {
            //weapon hit
            targetUnit.TakeDamage(RollD6(selectedWeapon.damageDice) + selectedWeapon.damageMod);
        }
        else
        {
            //weapon miss
        }
    }

    IEnumerator DisplayAttackText(int attackRoll)
    {
        SetUnitTargetTextVisibility(true);
        TextMeshProUGUI tmp = unitTargetText.GetComponent<WorldPositionElement>().UIObject.GetComponent<TextMeshProUGUI>();
        tmp.text = attackRoll.ToString();

        yield return new WaitForSeconds(2);

        SetUnitTargetTextVisibility(false);
        tmp.text = evasionTarget.ToString();
    }
}