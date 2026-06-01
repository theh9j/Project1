using System;
using UnityEngine;
[System.Serializable]

public class ColorTranslator
{
    public Color GetColor(LiquidColor liquidColor) {
        switch (liquidColor) {
            case LiquidColor.red:
                ColorUtility.TryParseHtmlString("#FF1717", out Color red);
                return red;
            case LiquidColor.green:
                ColorUtility.TryParseHtmlString("#6BFC41", out Color green);
                return green;
            case LiquidColor.blue:
                ColorUtility.TryParseHtmlString("#4F52FF", out Color blue);
                return blue;
            case LiquidColor.yellow:
                ColorUtility.TryParseHtmlString("#F7E40F", out Color yellow);
                return yellow;
            case LiquidColor.pink:
                ColorUtility.TryParseHtmlString("#E180F2", out Color pink);
                return pink;
            case LiquidColor.purple:
                ColorUtility.TryParseHtmlString("#910CF0", out Color purple);
                return purple;
            case LiquidColor.grey:
                ColorUtility.TryParseHtmlString("#696969", out Color grey);
                return grey;
            case LiquidColor.brown:
                ColorUtility.TryParseHtmlString("#572E2E", out Color brown);
                return brown;
            case LiquidColor.unknown:
                ColorUtility.TryParseHtmlString("#000000", out Color black);
                return black;
            default:
                return Color.clear;
        }
    }
        
}
