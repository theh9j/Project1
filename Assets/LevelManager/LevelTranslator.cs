using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
[System.Serializable]

public class LevelTranslator
{
    public int TranslatedColor(LiquidColor color) {
        return (color) switch {
            LiquidColor.red => 0,
            LiquidColor.green => 1,
            LiquidColor.blue => 2,
            LiquidColor.yellow => 3,
            LiquidColor.pink => 4,
            LiquidColor.purple => 5,
            LiquidColor.grey => 6,
            LiquidColor.brown => 7,
            _ => 8
        };
    }

    public Dictionary<int, LiquidColor> Randomizer() {
        Dictionary<int, LiquidColor> decoder = new();
        List<LiquidColor> availableColors = new List<LiquidColor>((LiquidColor[])Enum.GetValues(typeof(LiquidColor)));
        availableColors.Remove(LiquidColor.unknown);
        int colorRange = availableColors.Count;

        for (int i = 0; i < colorRange; i++) {
            int randomIndex = Random.Range(0, availableColors.Count);

            LiquidColor color = availableColors[randomIndex];
            decoder[i] = color;
            availableColors.RemoveAt(randomIndex);
        }

        return decoder;
    }
}
