using UnityEngine;
[System.Serializable]

public class LevelTranslator
{
    public int TranslatedColor(LiquidColor color) {
        return (color) switch {
            LiquidColor.red => 1,
            LiquidColor.green => 2,
            LiquidColor.blue => 3,
            LiquidColor.yellow => 4,
            LiquidColor.pink => 5,
            LiquidColor.purple => 6,
            LiquidColor.grey => 7,
            LiquidColor.brown => 8,
            _ => 0
        };
    }
}
