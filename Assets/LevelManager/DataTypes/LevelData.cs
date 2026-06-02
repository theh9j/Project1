using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class LevelData
{
    public int levelNumber;
    public int variantIndex;

    public int bottleCount;

    public List<BottleData> bottes = new List<BottleData>();
}
