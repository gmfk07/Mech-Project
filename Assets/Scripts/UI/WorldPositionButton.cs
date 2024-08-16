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
    
    private void Start()
    {
        hexa = Hexasphere.GetInstance("Hexasphere");
        buttonObject = GetComponentInChildren<Button>().gameObject;
    }

    private void FixedUpdate()
    {
        var screenPoint = Camera.main.WorldToScreenPoint(targetTransform.position);
        GetComponent<RectTransform>().position = screenPoint;

        buttonObject.SetActive(hexa.IsTileVisibleFromCamera(targetTransform.gameObject.GetComponent<Object>().tileIndex, Camera.main));
    }
}
