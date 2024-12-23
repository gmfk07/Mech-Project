using TMPro;
using Unity.Profiling;
using UnityEngine;

public class RecruitmentPanel : MonoBehaviour
{
    City selectedCity;
    [SerializeField] TextMeshProUGUI recruitmentText;
    [SerializeField] RecruitableButtonRow recruitableButtonRow;

    public void Start()
    {
        recruitmentText.text = "";
    }

    public void SetSelectedCity(City selectedCity)
    {
        this.selectedCity = selectedCity;
        recruitableButtonRow.SetSelectedCity(selectedCity);
    }

    public void SetVisible(bool visibility)
    {
        recruitmentText.enabled = visibility;
        if (visibility)
        {
            recruitableButtonRow.InitializeRecruitables(NationManager.instance.nations[TurnManager.instance.currentPlayer].availableRecruitables);
        }
        else
        {
            recruitableButtonRow.ClearRecruitables();
        }
    }

    public void UpdateRecruitmentText()
    {
        recruitmentText.text = "Recruiting " + selectedCity.recruiting.name;
    }
}
