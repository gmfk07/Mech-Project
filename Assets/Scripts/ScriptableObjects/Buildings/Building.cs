using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Building", order = 3)]
public class Building : ScriptableObject
{
    public string buildingName;
    public GameObject citySubObjectPrefab;
    public bool hasRequiredDeposit;
    public Deposit requiredDeposit;
}
