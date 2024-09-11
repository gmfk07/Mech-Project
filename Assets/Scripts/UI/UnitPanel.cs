using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitPanel : MonoBehaviour
{
    [SerializeField] private UnitInfo unitInfo;
    [SerializeField] private GameObject weaponButtonPrefab;
    private List<GameObject> createdWeaponButtons = new List<GameObject>();

    public void CreateWeaponButtons()
    {
        BattlerUnit battlerUnit = ObjectManager.instance.selectedUnit.GetComponent<BattlerUnit>();
        if (battlerUnit)
        {
            foreach (Weapon weapon in battlerUnit.weaponList)
            {
                createdWeaponButtons.Add(Instantiate(weaponButtonPrefab, transform));
                WeaponButton weaponButton = weaponButtonPrefab.GetComponent<WeaponButton>();
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
            CreateWeaponButtons();
        }
        else
        {
            DestroyWeaponButtons();
        }
    }

    public void DestroyWeaponButtons()
    {
        foreach (GameObject gameObject in createdWeaponButtons)
        {
            Destroy(gameObject);
        }
    }
}
