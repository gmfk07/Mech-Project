using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Resource", order = 1)]
public class Resource : ScriptableObject
{
    public string resourceName;
    public Sprite sprite; 
}
