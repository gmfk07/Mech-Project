using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;
using UnityEngine;

public class Object : MonoBehaviour
{
    [HideInInspector] public int tileIndex;

    protected Hexasphere hexa;

    virtual protected void Start()
    {
        hexa = Hexasphere.GetInstance("Hexasphere");
        AlignToSurface();
    }

    void AlignToSurface() {
        transform.LookAt(hexa.transform.position);
        transform.Rotate(-90, 0, 0, Space.Self);
    }
}
