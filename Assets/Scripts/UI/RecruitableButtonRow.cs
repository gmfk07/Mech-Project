using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class RecruitableButtonRow : MonoBehaviour
{
    [SerializeField] private GameObject recruitableButtonPrefab;
    City selectedCity;

    public void SetSelectedCity(City selectedCity)
    {
        this.selectedCity = selectedCity;
    }

    public void InitializeRecruitables(List<Recruitable> recruitables)
    {
        foreach (Recruitable recruitable in recruitables)
        {
            GameObject child = Instantiate(recruitableButtonPrefab, transform);
            child.GetComponentInChildren<RecruitableButton>().InitializeRecruitButton(recruitable, selectedCity);
        }
    }

    public void ClearRecruitables()
    {
        foreach (Transform transform in transform)
        {
            Destroy(transform.gameObject);
        }
    }
}
