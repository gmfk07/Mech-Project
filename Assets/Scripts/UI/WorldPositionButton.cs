using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HexasphereGrid;
using System;

public class WorldPositionButton : MonoBehaviour
{
    Hexasphere hexa;
    [HideInInspector] public Transform targetTransform;
    private GameObject buttonObject;
    [SerializeField] private float height;
    
    private void Start()
    {
        hexa = Hexasphere.GetInstance("Hexasphere");
        buttonObject = GetComponentInChildren<Button>().gameObject;
    }

    private void Update()
    {
        var screenPoint = Camera.main.WorldToScreenPoint(targetTransform.TransformPoint(Vector3.up * height));
        GetComponent<RectTransform>().position = screenPoint;

        buttonObject.SetActive(hexa.IsTileVisibleFromCamera(targetTransform.gameObject.GetComponent<Object>().tileIndex, Camera.main));
    }
}
