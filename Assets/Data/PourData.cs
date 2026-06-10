using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class PourData
{
    public Bottle shuffle;
    public List<LiquidUnit> prior = new();

    public Bottle from;
    public Bottle to;
    public List<LiquidUnit> movedLiquids = new();
        
    public List<Bottle> deconditionalize = new();
    public LiquidColor colorFinishes;
}