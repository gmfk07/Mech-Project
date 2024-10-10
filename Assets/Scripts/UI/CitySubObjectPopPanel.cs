using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitySubObjectPopPanel : MonoBehaviour
{
    [HideInInspector] public CitySubObject citySubObject;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "PopIcon" && citySubObject.workerIndex == -1)
        {
            PopIcon popIcon = other.GetComponent<PopIcon>();
            popIcon.currentPopPanel = this;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "PopIcon" && citySubObject.workerIndex == other.GetComponent<PopIcon>().popIndex)
        {
            PopIcon popIcon = other.GetComponent<PopIcon>();
            popIcon.currentPopPanel = null;
        }
    }
}
