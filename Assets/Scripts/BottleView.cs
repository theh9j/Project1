using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BottleView : MonoBehaviour
{
    [SerializeField]
    private LiquidSlotView[] liquidSlots;

    private readonly ColorTranslator colorTranslator = new ColorTranslator();

    public void Refresh(List<LiquidUnit> liquidUnits) {
        for (int i = 0; i < liquidSlots.Length; i++) {
            if (i >= liquidUnits.Count) {
                liquidSlots[i].Clear();
                continue;
            }

            LiquidUnit liquid = liquidUnits[i];

            bool isTopLiquid = i == liquidUnits.Count - 1;
            bool hasLiquidAbove = i < liquidUnits.Count - 1;

            if (liquid.isMystery &&
               (isTopLiquid ||
               (hasLiquidAbove && liquid.colorId == liquidUnits[liquidSlots.Length-1].colorId && !liquidUnits[i+1].isMystery))) {
                liquid.DeMysterize();
            }

            Color liquidColor = colorTranslator.GetColor(liquid.colorId);
            liquidSlots[i].SetLiquid(
                liquidColor,
                liquid.isMystery,
                i
            );
        }
    }
}
