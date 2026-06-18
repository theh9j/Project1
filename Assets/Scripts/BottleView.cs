using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BottleView : MonoBehaviour
{
    [SerializeField]
    private LiquidSlotView[] liquidSlots;
    [SerializeField] private AnimationHandler anim;
    [SerializeField] private LiquidColorVisualData colorTranslator;

    public void Refresh(List<LiquidUnit> liquidUnits) {
        float fillAmount = 0.25f;
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
               (hasLiquidAbove && liquid.colorId == liquidUnits[liquidUnits.Count-1].colorId && !liquidUnits[i+1].isMystery))) {
                liquid.DeMysterize();
            }

            Color liquidColor = colorTranslator.GetColor(liquid.colorId);
            liquidSlots[i].SetLiquid(
                liquidColor,
                liquid.isMystery,
                i
            );
        }
        Color a = Color.red;
        anim.SetPourLiquid(a, fillAmount * liquidUnits.Count);
    }
}
