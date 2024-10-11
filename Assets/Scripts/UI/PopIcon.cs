using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopIcon : MonoBehaviour
{
    [HideInInspector] public int popIndex;
    [HideInInspector] public City owningCity;
    [HideInInspector] public CitySubObjectPopPanel currentPopPanel;

    public void OnEndDrag()
    {
        CitySubObject currentWorkedSubObject = owningCity.GetPop(popIndex).working;
        if (currentPopPanel != null)
        {
            //Left mouse was released on top of a panel
            CitySubObject currentPopPanelSubObject = currentPopPanel.citySubObject;

            //If we're working something, stop working it
            if (currentWorkedSubObject != null)
            {
                currentWorkedSubObject.StopBeingWorked();
            }
            //Check if currentPopPanel is already occupied
            if (currentPopPanelSubObject.workerIndex != -1)
            {
                currentPopPanelSubObject.StopBeingWorked();
                //Check if this pop is currently working something
                if (currentWorkedSubObject != null)
                {
                    //Have the occupant pop swap jobs with this pop
                    currentWorkedSubObject.StartBeingWorked(currentPopPanelSubObject.workerIndex);
                    currentPopPanelSubObject.GetComponentInChildren<PopIcon>().transform.SetParent(currentWorkedSubObject.GetCitySubObjectPopPanel().transform);
                }
            }

            currentPopPanelSubObject.StartBeingWorked(popIndex);
            transform.SetParent(currentPopPanel.transform);
            transform.localPosition = Vector3.zero;
        }
        else
        {
            //Left mouse was released on empty space
            if (currentWorkedSubObject)
            {
                currentWorkedSubObject.StopBeingWorked();
                transform.SetParent(UICanvas.instance.popIconParent);
            }
            GetComponent<RectTransform>().localPosition = Vector3.zero;
        }
    }
}
