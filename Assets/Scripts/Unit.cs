using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using UnityEngine;

public class Unit : MonoBehaviour
{
    Hexasphere hexa;
    [HideInInspector] public int tileIndex;
    [SerializeField] float moveDuration;
    public List<int> path;
    float startTime;

    // Start is called before the first frame update
    void Start()
    {
        hexa = Hexasphere.GetInstance("Hexasphere");
        AlignToSurface();
        transform.Translate(0, 0, 0);
        path = new List<int>();
    }

    void AlignToSurface() {
        transform.LookAt(hexa.transform.position);
        transform.Rotate(-90, 0, 0, Space.Self);
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
