using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using UnityEngine;

public class Unit : Object
{
    [SerializeField] float moveDuration;
    [HideInInspector] public List<int> path;
    float startTime;
    public int moveRange;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        path = new List<int>();
    }

    public bool IsMoving()
    {
        return path.Count > 0;
    }

    public IEnumerator MoveUnit() {
        startTime = Time.time;
        Vector3 startPosition = transform.localPosition;
        Vector3 endPosition = hexa.GetTileCenter(path[0], worldSpace: false);
        Debug.Log("Moving from " + startPosition + " to " + endPosition);

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
    }
}
