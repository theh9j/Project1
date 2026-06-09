using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class PourData
{
    public Bottle from;
    public Bottle to;
    public List<LiquidUnit> movedLiquids = new();
}