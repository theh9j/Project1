using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;

public class LevelDesigner : MonoBehaviour
{

    [SerializeField]
    private UIHandler ui;

    public void SetColors(Bottle bottle, TMP_InputField[] colors, TMP_Text[] mys) {
        string[] color_texts = new string[colors.Length];

        for (int i = 0; i < colors.Length; i++) {
            color_texts[i] = colors[i].text;
        }

        List<LiquidUnit> liquidUnits = bottle.liquidUnits;

        for (int i = 0; i < colors.Length; i++) {

            if (Enum.TryParse(color_texts[i], out LiquidColor color)) {
                if (i > liquidUnits.Count-1) {
                    LiquidUnit newLiquid = new LiquidUnit(color);
                    liquidUnits.Add(newLiquid);
                } else {
                    liquidUnits[i].colorId = color;
                }
                CheckAndSetMystery(liquidUnits[i], mys[i]);
            } else if (color_texts[i] == $"Color{i}") {
                continue;
            } else {
                Debug.LogWarning($"Invalid color input for color at {i}");
            }
        }

        bottle.RefreshView();
    }

    public void CheckAndSetMystery(LiquidUnit liquid, TMP_Text mys) {
        if (mys.text == "True") {
            liquid.isMystery = true;
        } else {
            liquid.isMystery = false;
        }
    }

}
