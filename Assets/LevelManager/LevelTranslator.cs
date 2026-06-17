using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
[System.Serializable]

public class LevelTranslator
{
    public int TranslatedColor(LiquidColor color) {

        LiquidColor[] availableColors = (LiquidColor[])Enum.GetValues(typeof(LiquidColor));
        for (int i = 0; i < Enum.GetValues(typeof(LiquidColor)).Length; i++) {
            if (color == availableColors[i]) {
                return i;
            }
        } 
        return Enum.GetValues(typeof(LiquidColor)).Length;

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
