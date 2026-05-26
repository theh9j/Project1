using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BottleView : MonoBehaviour
{
    [SerializeField]
    private LiquidSlotView[] liquidSlots;

    private LiquidColor color;

    private readonly ColorTranslator colorTranslator = new ColorTranslator();

    public void Refresh(List<LiquidUnit> liquidUnits) {
        for (int i = 0; i < liquidSlots.Length; i++) {
            if (i >= liquidUnits.Count) {
                liquidSlots[i].Clear();
                continue;
            }

            LiquidUnit liquid = liquidUnits[i];
            Color liquidColor = colorTranslator.GetColor(liquid.colorId);
            liquidSlots[i].SetLiquid(
                liquidColor,
                liquid.isMystery,
                i
            );
        }
    }
}
