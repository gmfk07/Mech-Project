using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDCanvas : MonoBehaviour
{
    public static HUDCanvas instance;
    private Hexasphere hexa;
    [SerializeField] GameObject cityButtonPrefab;
    [SerializeField] Image cityPanel;
    [SerializeField] TextMeshProUGUI cityNameText;

    void Start()
    {
        instance = this;
        cityPanel.enabled = false;
        hexa = Hexasphere.GetInstance("Hexasphere");
        hexa.OnDragStart += (hexa) => SetCityPanelVisible(false);
        hexa.OnTileClick += (hexa, tileIndex) => SetCityPanelVisible(false);
        hexa.OnTileRightClick += (hexa, tileIndex) => SetCityPanelVisible(false);
        SetCityPanelVisible(false);
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            SetCityPanelVisible(false);
        }
    }

    public WorldPositionButton CreateCityButton(City city)
    {
        GameObject cityButton = Instantiate(cityButtonPrefab, transform);
        cityButton.GetComponent<WorldPositionButton>().targetTransform = city.transform;
        cityButton.GetComponentInChildren<Button>().onClick.AddListener(city.HandleClicked);
        return cityButton.GetComponent<WorldPositionButton>();
    }

    public void SetCityPanelVisible(bool visibility)
    {
        cityPanel.enabled = visibility;
        cityNameText.enabled = visibility;
    }

    public void SetCityNameText(string text)
    {
        cityNameText.text = text;
    }
}
