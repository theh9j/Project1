using System.Collections.Generic;
using UnityEngine;

public class BottleView : MonoBehaviour {
    [Header("References")]
    [SerializeField] private AnimationHandler anim;
    [SerializeField] private LiquidColorVisualData colorTranslator;

    [Header("Visual Fill Amounts")]
    [SerializeField]
    private float[] fillAmounts =
    {
        0f,    // 0 liquids
        0.25f, // 1 liquid
        0.50f, // 2 liquids
        0.75f, // 3 liquids
        1.00f  // 4 liquids
    };

    [Header("Pour Into Empty")]
    [SerializeField] private float emptyPourStartFill = 0.05f;

    private void Awake() {
        if (anim == null)
            anim = GetComponent<AnimationHandler>();
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

    public void RefreshColorsOnly(List<LiquidUnit> liquidUnits) {
        RevealMystery(liquidUnits);

        Color[] colors = BuildColors(liquidUnits);

        anim.SetPourLiquidColors(
            colors,
            liquidUnits.Count
        );
    }

    public float GetVisualFillAmount(int liquidCount) {
        liquidCount = Mathf.Clamp(
            liquidCount,
            0,
            fillAmounts.Length - 1
        );

        return fillAmounts[liquidCount];
    }

    public float GetPourInStartFill(int liquidCount) {
        if (liquidCount <= 0)
            return emptyPourStartFill;

        return GetVisualFillAmount(liquidCount);
    }

    private Color[] BuildColors(List<LiquidUnit> liquidUnits) {
        Color[] colors = new Color[4];

        for (int i = 0; i < colors.Length; i++) {
            if (i < liquidUnits.Count) {
                colors[i] = colorTranslator.GetColor(
                    liquidUnits[i].colorId
                );
            } else {
                colors[i] = Color.clear;
            }
        }

        return colors;
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

            if (!above.isMystery &&
                above.colorId == liquid.colorId) {
                liquid.DeMysterize();
            }
        }
    }
}