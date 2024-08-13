using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using UnityEngine;

public class BuilderUnit : Unit
{
    [SerializeField] private GameObject minePrefab;

    void Update()
    {
        if (!IsMoving() && Input.GetButtonDown("Build"))
        {
            BuildMine();
        }
    }

    void BuildMine()
    {
        Object mine = Instantiate(minePrefab).GetComponent<Object>();
        mine.tileIndex = tileIndex;

        mine.transform.SetParent(hexa.transform);
        mine.transform.position = hexa.GetTileCenter(tileIndex);
    }
}
