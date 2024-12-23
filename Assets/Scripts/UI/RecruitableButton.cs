using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RecruitableButton : MonoBehaviour
{
    [SerializeField] private ResourceRowList resourceRowList;
    [SerializeField] private Image image;
    private City selectedCity;
    private Recruitable recruitable;

    public void InitializeRecruitButton(Recruitable recruitable, City selectedCity)
    {
        this.recruitable = recruitable;
        this.selectedCity = selectedCity;
        image.sprite = recruitable.sprite;
        resourceRowList.resources = recruitable.costResourceList;
        resourceRowList.resourceAmounts = recruitable.costValueList;
        resourceRowList.UpdateResourceDisplay();
    }

    public void HandleRecruitButtonPressed()
    {
        selectedCity.SetRecruiting(recruitable);
    }
}
