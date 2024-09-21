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
        startTime = Time.time;
        Vector3 startPosition = transform.localPosition;
        Vector3 endPosition = hexa.GetTileCenter(path[0], worldSpace: false);
        unitAnimator.SetBool("isMoving", true);

        float t = 0;
        while (t < 1f)
        {
            // Compute next position
            t = (Time.time - startTime) / moveDuration;
            t = Mathf.Clamp(t, 0, 1);
            Vector3 surfacePosition = Vector3.Lerp(startPosition, endPosition, t).normalized * 0.5f;
            // clamp to surface including extrusion
            transform.localPosition = hexa.GetExtrudedPosition(surfacePosition, worldSpace: false);

            // Adjust rotation so it keeps aligned to hexasphere surface
            Vector3 lookPosition = hexa.transform.TransformPoint(endPosition);
            Vector3 up = (transform.position - hexa.transform.position).normalized;

            transform.LookAt(lookPosition, up);

            yield return null;
        }
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
