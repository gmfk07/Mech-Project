using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HexasphereGrid;
using System;

public class WorldPositionButton : WorldPositionElement
{
    private Button button;
    
    protected override void PostStart()
    {
        button = GetComponentInChildren<Button>();
    }

    //Is the button initialized and ready to be interacted with?
    public bool ButtonInitialized()
    {
        return button != null;
    }

    public void ChangeButtonSprite(Sprite newSprite)
    {
        button.image.sprite = newSprite;
    }
}
