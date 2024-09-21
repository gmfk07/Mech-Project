using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using UnityEngine;

public class WorldPositionElement : MonoBehaviour
{
    private Hexasphere hexa;
    [HideInInspector] public Transform targetTransform;
    [SerializeField] protected GameObject UIObject;
    [SerializeField] private float height;
    //Whether or not the WPE should display graphics when it is on screen.
    private bool visibility = true;

    private void Start()
    {
        hexa = Hexasphere.GetInstance("Hexasphere");
        PostStart();
    }

    protected virtual void PostStart() {}

    private void Update()
    {
        var screenPoint = Camera.main.WorldToScreenPoint(targetTransform.TransformPoint(Vector3.up * height));
        GetComponent<RectTransform>().position = screenPoint;

        UIObject.SetActive(visibility && hexa.IsTileVisibleFromCamera(targetTransform.gameObject.GetComponent<Object>().tileIndex, Camera.main));
    }

    //Sets whether or not the WPE should display graphics when it is on screen.
    public void SetVisibility(bool visibility)
    {
        this.visibility = visibility;
    }
}
