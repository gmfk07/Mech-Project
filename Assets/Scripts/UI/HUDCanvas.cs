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
    [SerializeField] GameObject citySubObjectButtonPrefab;
    [SerializeField] Image cityPanel;
    [SerializeField] TextMeshProUGUI cityNameText;
    [SerializeField] TextMeshProUGUI cityPopulationText;
    [HideInInspector] City selectedCity;

    void Start()
    {
        instance = this;
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

    public WorldPositionButton CreateCitySubObjectButton(CitySubObject citySubObject)
    {
        GameObject citySubObjectButton = Instantiate(citySubObjectButtonPrefab, transform);
        citySubObjectButton.GetComponent<WorldPositionButton>().targetTransform = citySubObject.transform;
        citySubObjectButton.GetComponentInChildren<Button>().onClick.AddListener(citySubObject.HandleClicked);
        return citySubObjectButton.GetComponent<WorldPositionButton>(); 
    }

    public void SetSelectedCity(City selectedCity)
    {
        this.selectedCity = selectedCity;
        cityNameText.text = selectedCity.cityName;
    }

    public void SetCityPanelVisible(bool visibility)
    {
        if (visibility)
        {
            ObjectManager.instance.selectedUnit = null;
        }
        cityPanel.enabled = visibility;
        cityNameText.enabled = visibility;
        cityPopulationText.enabled = visibility;
        if (selectedCity != null)
        {
            foreach (CitySubObject citySubObject in selectedCity.citySubObjects)
            {
                citySubObject.SetCitySubObjectButtonActive(visibility);
            }
        }
    }

    public void SetCityPopulation(int availableCitizens, int totalCitizens)
    {
        cityPopulationText.text = "Pop: " + availableCitizens.ToString() + "/" + totalCitizens.ToString();
    }
}
