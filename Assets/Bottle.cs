using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Bottle : MonoBehaviour 
{
    public bool isFilled = false;
    public bool isEmpty = false;
    public bool isLocked = false;


    public int maxCapacity = 4;
    public List<LiquidUnit> liquidUnits = new List<LiquidUnit>();

    public void Fill()
    {
        isFilled = true;
    }

    public void Pour(Bottle nextBottle)
    {
        if (isFilled || isLocked) return;

    }

}
