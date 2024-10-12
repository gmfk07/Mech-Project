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
    protected Animator unitAnimator;

    protected void Awake()
    {
        hp = maxHp;
        rp = maxRp;
        unitAnimator = GetComponent<Animator>();
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
        if (hp == 0)
        {
            DestroyUnit();
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

    //Destroys the unit and its associated UI objects.
    public void DestroyUnit()
    {
        ObjectManager.instance.HandleUnitDestroyed(this);
        Destroy(unitButton.gameObject);
        Destroy(unitText.gameObject);
        Destroy(gameObject);
    }
}
