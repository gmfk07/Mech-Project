using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Recruitable", order = 3)]
public class Recruitable : ScriptableObject
{
    public string recruitableName;
    public Sprite sprite;
    public GameObject unitPrefab;
    public List<Resource> costResourceList;
    public List<int> costValueList;
}
