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
    [HideInInspector] public bool active = true;
    [HideInInspector] public int hp;
    public int maxHp;
    [HideInInspector] public int rp;
    public int maxRp;
    public int evasionTarget;
    public Nation owningNation;
    protected WorldPositionButton unitButton;
    protected WorldPositionElement unitText;
    [SerializeField] protected List<Sprite> unitButtonImages;
    public string unitName;
    [SerializeField] private Animator unitAnimator;
    [SerializeField] private Transform legsTransform;

    protected void Awake()
    {
        hp = maxHp;
        rp = maxRp;
        path = new List<int>();
        RefreshMoves();
        StartCoroutine(CreateUnitButton());
        StartCoroutine(CreateUnitText());
    }

    public void RefreshMoves()
    {
        remainingMoves = moveRange;
        active = true;
    }

    public bool IsMoving()
    {
        return path.Count > 0;
    }

    public IEnumerator MoveUnit() {
        Vector3 localStartPositionNoExtrusion = hexa.GetTileCenter(tileIndex, worldSpace: false, includeExtrusion: false);
        Vector3 globalStartPositionNoExtrusion = hexa.transform.TransformPoint(localStartPositionNoExtrusion);
        Vector3 localEndPositionNoExtrusion = hexa.GetTileCenter(path[0], worldSpace: false, includeExtrusion: false);
        Vector3 globalEndPositionNoExtrusion = hexa.transform.TransformPoint(localEndPositionNoExtrusion);
        Vector3 localEndPositionExtrusion = hexa.GetTileCenter(path[0], worldSpace: false);
        Vector3 globalEndPositionExtrusion = hexa.transform.TransformPoint(localEndPositionExtrusion);

        float rightDot = Vector3.Dot(legsTransform.right, globalEndPositionNoExtrusion - globalStartPositionNoExtrusion);
        float forwardDot = Vector3.Dot(legsTransform.forward.normalized, (globalEndPositionNoExtrusion - globalStartPositionNoExtrusion).normalized);
        float maxRightDot = 0.01f;

        while (Mathf.Abs(rightDot) > maxRightDot || forwardDot < 0)
        {
            rightDot = Vector3.Dot(legsTransform.right, globalEndPositionNoExtrusion - globalStartPositionNoExtrusion);
            forwardDot = Vector3.Dot(legsTransform.forward.normalized, (globalEndPositionNoExtrusion - globalStartPositionNoExtrusion).normalized);
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
            yield return null;
        }

        unitAnimator.SetBool("turnLeft", false);
        unitAnimator.SetBool("turnRight", false);

        startTime = Time.time;
        float t = 0;
        while (t < 1f)
        {
            // Compute next position
            t = (Time.time - startTime) / moveDuration;
            t = Mathf.Clamp(t, 0, 1);
            Vector3 surfacePosition = Vector3.Lerp(localStartPositionNoExtrusion, localEndPositionExtrusion, t).normalized * 0.5f;
            // clamp to surface including extrusion
            transform.localPosition = hexa.GetExtrudedPosition(surfacePosition, worldSpace: false);

            /*Vector3 up = (legsTransform.position - hexa.transform.position).normalized;
            legsTransform.LookAt(globalEndPositionExtrusion, up);*/

            unitAnimator.SetBool("isMoving", true);

            yield return null;
        }
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
        if (hp == 0)
        {
            Destroy(this);
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
        unitText = UICanvas.instance.CreateWorldPositionText(transform);
        unitText.GetComponentInChildren<TextMeshProUGUI>().text = evasionTarget.ToString();
        SetUnitTextVisibility(false);
        unitText.GetComponentInChildren<TextMeshProUGUI>().raycastTarget = false;
    }

    public void SetUnitTextVisibility(bool visibility)
    {
        unitText.SetVisibility(visibility);
    }
}
