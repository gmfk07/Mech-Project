using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using UnityEngine;

public class WorldPositionElement : MonoBehaviour
{
    private Hexasphere hexa;
    [HideInInspector] public Transform targetTransform;
    [SerializeField] public GameObject UIObject;
    [SerializeField] private float height;
    //Whether or not the WPE should display graphics when it is on screen.
    private bool visibility = true;
    private Vector3 screenOffset;

    private void Start()
    {
        hexa = Hexasphere.GetInstance("Hexasphere");
        PostStart();
    }

    protected virtual void PostStart() {}

    private void Update()
    {
        var screenPoint = Camera.main.WorldToScreenPoint(targetTransform.TransformPoint(Vector3.up * height));
        GetComponent<RectTransform>().position = screenPoint + screenOffset;

        UIObject.SetActive(visibility && hexa.IsTileVisibleFromCamera(targetTransform.gameObject.GetComponent<Object>().tileIndex, Camera.main));
    }

    //Sets whether or not the WPE should display graphics when it is on screen.
    public void SetVisibility(bool visibility)
    {
        this.visibility = visibility;
    }

    public void SetScreenOffset(Vector3 screenOffset)
    {
        this.screenOffset = screenOffset;
    }
}
