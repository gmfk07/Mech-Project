using System;
using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Unit : Object
{
    [SerializeField] float moveDuration;
    [HideInInspector] public List<int> path;
    float startTime;
    public int moveRange;
    [HideInInspector] public int remainingMoves;
    //Whether or not this unit has anything they can do (move or act) on this turn
    [HideInInspector] public bool active = true;
    //Whether or not this unit has taken an action (shoot or build) on this turn
    public bool hasActed = false;
    [HideInInspector] public int hp;
    public int maxHp;
    [HideInInspector] public int rp;
    public int maxRp;
    public int evasionTarget;
    public int pushReactorCost;
    public Nation owningNation;
    protected WorldPositionButton unitButton;
    protected WorldPositionElement unitTargetText;
    protected WorldPositionElement unitDamageText;
    [SerializeField] protected List<Sprite> unitButtonImages;
    public string unitName;
    protected Animator unitAnimator;
    [SerializeField] float damageTime = 2;
    [SerializeField] float deathTime = 2;

    protected void Awake()
    {
        hp = maxHp;
        rp = maxRp;
        unitAnimator = GetComponent<Animator>();
        path = new List<int>();
        RefreshActions();
        StartCoroutine(CreateUnitButton());
        StartCoroutine(CreateUnitText());
    }

    public void RefreshActions()
    {
        remainingMoves = moveRange;
        active = true;
        hasActed = false;
        pushReactorCost = 1;
    }

    public bool IsMoving()
    {
        return path.Count > 0;
    }

    public IEnumerator MoveUnit() {
        Vector3 localStartPositionNoExtrusion = hexa.GetTileCenter(tileIndex, worldSpace: false, includeExtrusion: false);
        Vector3 globalStartPositionNoExtrusion = hexa.transform.TransformPoint(localStartPositionNoExtrusion);
        Vector3 localStartPositionExtrusion = hexa.GetTileCenter(tileIndex, worldSpace: false);

        Vector3 localEndPositionNoExtrusion = hexa.GetTileCenter(path[0], worldSpace: false, includeExtrusion: false);
        Vector3 globalEndPositionNoExtrusion = hexa.transform.TransformPoint(localEndPositionNoExtrusion);
        Vector3 localEndPositionExtrusion = hexa.GetTileCenter(path[0], worldSpace: false);

        float rightDot = Vector3.Dot(transform.right, globalEndPositionNoExtrusion - globalStartPositionNoExtrusion);
        float forwardDot = Vector3.Dot(transform.forward.normalized, (globalEndPositionNoExtrusion - globalStartPositionNoExtrusion).normalized);
        float maxRightDot = 0.01f;

        //Rotate towards the destination
        while (Mathf.Abs(rightDot) > maxRightDot || forwardDot < 0)
        {
            rightDot = Vector3.Dot(transform.right, globalEndPositionNoExtrusion - globalStartPositionNoExtrusion);
            forwardDot = Vector3.Dot(transform.forward.normalized, (globalEndPositionNoExtrusion - globalStartPositionNoExtrusion).normalized);
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

        Vector3 up = (transform.position - hexa.transform.position).normalized;

        //Move towards the destination
        startTime = Time.time;
        float t = 0;

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.LookRotation(globalEndPositionNoExtrusion - globalStartPositionNoExtrusion, up);

        while (t < 1f)
        {
            // Compute next position
            t = (Time.time - startTime) / moveDuration;
            t = Mathf.Clamp(t, 0, 1);
            Vector3 surfacePosition = Vector3.Lerp(localStartPositionExtrusion, localEndPositionExtrusion, t).normalized * 0.5f;
            // clamp to surface including extrusion
            transform.localPosition = hexa.GetExtrudedPosition(surfacePosition, worldSpace: false);

            transform.rotation = Quaternion.Lerp(startRotation, endRotation, t);

            //Start moving animation
            unitAnimator.SetBool("isMoving", true);

            yield return null;
        }

        transform.LookAt(hexa.GetTileCenter(path[0]), up);

        tileIndex = path[0];
        path.RemoveAt(0);

        if (path.Count > 0)
        {
            StartCoroutine(MoveUnit());
        }
        else
        {
            unitAnimator.SetBool("isMoving", false);
        }
    }

    public void TakeDamage(int damage)
    {
        hp = Math.Max(0, hp - damage);
        StartCoroutine(DisplayDamageText(damage));
        if (hp == 0)
        {
            StartCoroutine(Die());
        }
    }

    public IEnumerator CreateUnitButton()
    {
        while (UICanvas.instance == null || TurnManager.instance == null)
        {
            yield return null;
        }
        unitButton = UICanvas.instance.CreateUnitButton(this);
        while (!unitButton.ButtonInitialized())
        {
            yield return null;
        }
        unitButton.ChangeButtonSprite(unitButtonImages[TurnManager.instance.currentPlayer]);
        unitButton.SetVisibility(true);
    }

    public IEnumerator CreateUnitText()
    {
        while (UICanvas.instance == null)
        {
            yield return null;
        }
        unitTargetText = UICanvas.instance.CreateWorldPositionText(transform);
        unitTargetText.GetComponentInChildren<TextMeshProUGUI>().text = evasionTarget.ToString();
        SetUnitTargetTextVisibility(false);
        unitTargetText.GetComponentInChildren<TextMeshProUGUI>().raycastTarget = false;

        unitDamageText = UICanvas.instance.CreateWorldPositionText(transform);
        unitDamageText.GetComponentInChildren<TextMeshProUGUI>().text = evasionTarget.ToString();
        unitDamageText.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
        unitDamageText.SetScreenOffset(new Vector3(0, 30, 0));
        SetUnitDamageTextVisibility(false);
        unitTargetText.GetComponentInChildren<TextMeshProUGUI>().raycastTarget = false;
    }

    public void SetUnitTargetTextVisibility(bool visibility)
    {
        unitTargetText.SetVisibility(visibility);
    }

    public void SetUnitDamageTextVisibility(bool visibility)
    {
        unitDamageText.SetVisibility(visibility);
    }

    IEnumerator DisplayDamageText(int damage)
    {
        SetUnitDamageTextVisibility(true);
        TextMeshProUGUI tmp = unitDamageText.GetComponent<WorldPositionElement>().UIObject.GetComponent<TextMeshProUGUI>();
        tmp.text = damage.ToString() + " DMG";

        yield return new WaitForSeconds(damageTime);

        SetUnitDamageTextVisibility(false);
    }

    //Animates death, waits, and destroys the unit.
    public IEnumerator Die()
    {
        yield return new WaitForSeconds(deathTime);
        DestroyUnit();
    }

    //Destroys the unit and its associated UI objects.
    public void DestroyUnit()
    {
        ObjectManager.instance.HandleUnitDestroyed(this);
        Destroy(unitButton.gameObject);
        Destroy(unitTargetText.gameObject);
        Destroy(unitDamageText.gameObject);
        Destroy(gameObject);
    }

    public void HandleActionPerformed()
    {
        hasActed = true;
        UICanvas.instance.UpdateUnitPanel();
        if (remainingMoves == 0)
        {
            active = false;
            UICanvas.instance.UpdateMainButton();
        }
    }
}
