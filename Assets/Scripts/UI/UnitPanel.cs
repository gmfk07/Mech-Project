using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitPanel : MonoBehaviour
{
    [SerializeField] private UnitInfo unitInfo;
    [SerializeField] private GameObject skipButtonPrefab;
    [SerializeField] private GameObject weaponButtonPrefab;
    private List<GameObject> createdWeaponButtons = new List<GameObject>();
    private GameObject createdSkipButton;

    public void CreateButtons()
    {
        BattlerUnit battlerUnit = ObjectManager.instance.selectedUnit.GetComponent<BattlerUnit>();

        createdSkipButton = Instantiate(skipButtonPrefab, transform);
        createdSkipButton.GetComponent<SkipButton>().SetVisibility(true);

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
        if (createdSkipButton)
        {
            Destroy(createdSkipButton);
        }
    }

    public void UpdateUnitInfo()
    {
        unitInfo.UpdateText();
    }
}
