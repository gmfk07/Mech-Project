using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitPanel : MonoBehaviour
{
    [SerializeField] private UnitInfo unitInfo;
    [SerializeField] private GameObject skipButtonPrefab;
    [SerializeField] private GameObject weaponButtonPrefab;
    [SerializeField] private GameObject buildButtonPrefab;
    [SerializeField] private GameObject pushReactorButtonPrefab;
    private List<GameObject> createdWeaponButtons = new List<GameObject>();
    private List<GameObject> createdBuildButtons = new List<GameObject>();
    private GameObject createdSkipButton;
    private GameObject createdPushReactorButton;

    public void CreateButtons()
    {
        BattlerUnit battlerUnit = ObjectManager.instance.selectedUnit.GetComponent<BattlerUnit>();
        BuilderUnit builderUnit = ObjectManager.instance.selectedUnit.GetComponent<BuilderUnit>();

        createdSkipButton = Instantiate(skipButtonPrefab, transform);
        createdSkipButton.GetComponent<SkipButton>().SetVisibility(true);

        if (ObjectManager.instance.selectedUnit.hasActed)
        {
            if (ObjectManager.instance.selectedUnit.rp > 0)
            {
                createdPushReactorButton = Instantiate(pushReactorButtonPrefab, transform);
                createdPushReactorButton.GetComponent<PushReactorButton>().SetVisibility(true);
            }
            return;
        }

        if (battlerUnit)
        {
            foreach (Weapon weapon in battlerUnit.weaponList)
            {
                createdWeaponButtons.Add(Instantiate(weaponButtonPrefab, transform));
                WeaponButton weaponButton = createdWeaponButtons[createdWeaponButtons.Count-1].GetComponent<WeaponButton>();
                weaponButton.Weapon = weapon;
                weaponButton.SetVisibility(true);
            }
        }

        if (builderUnit)
        {
            bool tileUnoccupied = !ObjectManager.instance.tileCitySubObjectDict.ContainsKey(builderUnit.tileIndex);
            bool withinPlayerCity = false;
            foreach (City city in NationManager.instance.nations[TurnManager.instance.currentPlayer].cities)
            {
                if (city.tilesWithinBorders.Contains(builderUnit.tileIndex))
                {
                    withinPlayerCity = true;
                }
            }
            foreach (Building building in NationManager.instance.nations[TurnManager.instance.currentPlayer].availableBuildings)
            {
                bool depositSatisfied = !building.hasRequiredDeposit || WorldGenerator.instance.deposits[builderUnit.tileIndex] == building.requiredDeposit;
                if (tileUnoccupied && withinPlayerCity && depositSatisfied)
                {
                    createdBuildButtons.Add(Instantiate(buildButtonPrefab, transform));
                    BuildButton buildButton = createdBuildButtons[createdBuildButtons.Count-1].GetComponent<BuildButton>();
                    buildButton.Building = building;
                    buildButton.SetVisibility(true);
                }
            }
        }
    }

    public void SetVisibility(bool visibility)
    {
        unitInfo.SetVisibility(visibility);
        GetComponent<Image>().enabled = visibility;
        if (visibility)
        {
            CreateButtons();
        }
        else
        {
            DestroyButtons();
        }
    }

    public void DestroyButtons()
    {
        foreach (GameObject gameObject in createdWeaponButtons)
        {
            Destroy(gameObject);
        }
        foreach (GameObject gameObject in createdBuildButtons)
        {
            Destroy(gameObject);
        }
        if (createdSkipButton)
        {
            Destroy(createdSkipButton);
        }
        if (createdPushReactorButton)
        {
            Destroy(createdPushReactorButton);
        }
    }

    public void UpdateUnitPanel()
    {
        unitInfo.UpdateText();
        DestroyButtons();
        CreateButtons();
    }
}
