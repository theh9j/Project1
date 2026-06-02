using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BottleData
{
    public List<LiquidData> liquids = new();

    public bool isLocked;
    public int lockCondition;
}