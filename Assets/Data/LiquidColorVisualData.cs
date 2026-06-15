using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LiquidColorVisualData", menuName = "Scriptable Objects/LiquidColorVisualData")]
public class LiquidColorVisualData : ScriptableObject
{
    public List<ColorEntry> colorEntry = new();

    public Color GetColor(LiquidColor color) {
        foreach (var entry in colorEntry) {
            if (entry.liquidColor == color) return entry.color;
        }
        return Color.black;
    }
}
