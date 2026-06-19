using System.Collections.Generic;
using UnityEngine;

public class BottleView : MonoBehaviour {
    [SerializeField] private AnimationHandler anim;
    [SerializeField] private LiquidColorVisualData colorTranslator;
    [SerializeField] private float bottleBottomFill = 0.55f;

    private float[] fillAmounts =
    {
        0f,   // 0 liquid
        0.6f, // 1 liquid
        0.65f, // 2 liquids
        0.7f, // 3 liquids
        0.75f  // 4 liquids
    };

    public float GetPourInStartFill(int liquidCount) {
        if (liquidCount <= 0)
            return bottleBottomFill;

        return GetVisualFillAmount(liquidCount);
    }


    public float GetVisualFillAmount(int liquidCount) {
        liquidCount = Mathf.Clamp(liquidCount, 0, fillAmounts.Length - 1);
        return fillAmounts[liquidCount];
    }

    public Color[] BuildColors(List<LiquidUnit> liquidUnits) {
        Color[] colors = new Color[4];

        for (int i = 0; i < colors.Length; i++) {
            colors[i] = i < liquidUnits.Count
                ? colorTranslator.GetColor(liquidUnits[i].colorId)
                : Color.clear;
        }

        return colors;
    }

    public void RefreshColorsOnly(List<LiquidUnit> liquidUnits) {
        RevealMystery(liquidUnits);

        Color[] colors = BuildColors(liquidUnits);

        anim.SetPourLiquidColors(
            colors,
            liquidUnits.Count
        );
    }

    public void Refresh(List<LiquidUnit> liquidUnits) {
        RevealMystery(liquidUnits);

        Color[] colors = BuildColors(liquidUnits);
        anim.SetPourLiquid(
            colors,
            GetVisualFillAmount(liquidUnits.Count),
            liquidUnits.Count
        );
    }

    private void RevealMystery(List<LiquidUnit> liquidUnits) {
        for (int i = 0; i < liquidUnits.Count; i++) {
            LiquidUnit liquid = liquidUnits[i];

            if (!liquid.isMystery)
                continue;

            bool isTop =
                i == liquidUnits.Count - 1;

            if (isTop) {
                liquid.DeMysterize();
                continue;
            }

            LiquidUnit above =
                liquidUnits[i + 1];

            if (
                !above.isMystery &&
                above.colorId ==
                liquid.colorId
            ) {
                liquid.DeMysterize();
            }
        }
    }
}