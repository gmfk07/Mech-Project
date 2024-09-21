using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    protected TextMeshProUGUI buttonText;

    public void Awake()
    {
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetVisibility(bool visibility)
    {
        GetComponent<Button>().enabled = visibility;
        buttonText.enabled = visibility;
    }

    public virtual void OnButtonPressed() {}
}
