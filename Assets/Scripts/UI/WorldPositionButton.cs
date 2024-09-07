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
    private Button button;
    
    private void Start()
    {
        hexa = Hexasphere.GetInstance("Hexasphere");
        button = GetComponentInChildren<Button>();
        buttonObject = button.gameObject;
    }

    private void Update()
    {
        var screenPoint = Camera.main.WorldToScreenPoint(targetTransform.TransformPoint(Vector3.up * height));
        GetComponent<RectTransform>().position = screenPoint;

        buttonObject.SetActive(hexa.IsTileVisibleFromCamera(targetTransform.gameObject.GetComponent<Object>().tileIndex, Camera.main));
    }

    //Is the button initialized and ready to be interacted with?
    public bool ButtonInitialized()
    {
        return button != null;
    }

    public void ChangeButtonSprite(Sprite newSprite)
    {
        button.image.sprite = newSprite;
    }
}
