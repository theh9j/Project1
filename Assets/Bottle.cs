using System.Collections.Generic;
using UnityEngine;

public class Bottle : MonoBehaviour {
    public bool isLocked = false;
    public bool isCompleted = false;

    public int maxCapacity = 4;
    public List<LiquidUnit> liquidUnits = new List<LiquidUnit>();

    public bool IsEmpty => liquidUnits.Count == 0;
    public bool IsFull => liquidUnits.Count >= maxCapacity;

    public void BottleInit(List<LiquidUnit> initialLiquids) {
        liquidUnits = new List<LiquidUnit>();

        foreach (LiquidUnit liquid in initialLiquids) {
            liquidUnits.Add(new LiquidUnit(liquid));
        }
    }

    public int CurrentCapacity() {
        return liquidUnits.Count;
    }

    public LiquidUnit GetTopLiquid() {
        if (IsEmpty) return null;
        return liquidUnits[liquidUnits.Count - 1];
    }

    public bool CanPourTo(Bottle nextBottle) {
        if (nextBottle == null) return false;

        if (isLocked || nextBottle.isLocked) return false;
        if (isCompleted || nextBottle.isCompleted) return false;
        if (IsEmpty) return false;
        if (nextBottle.IsFull) return false;

        LiquidUnit myTop = GetTopLiquid();
        LiquidUnit targetTop = nextBottle.GetTopLiquid();

        if (targetTop == null) return true;

        return myTop.colorId == targetTop.colorId;
    }

    public void Pour(Bottle nextBottle) {
        if (!CanPourTo(nextBottle)) return;

        LiquidColor pourColor = GetTopLiquid().colorId;

        while (true) {
            if (IsEmpty) break;

            if (nextBottle.IsFull) break;

            if (GetTopLiquid().colorId != pourColor) break;

            LiquidUnit topLiquid = GetTopLiquid();

            liquidUnits.RemoveAt(liquidUnits.Count - 1);
            nextBottle.liquidUnits.Add(new LiquidUnit(topLiquid));

            if (!IsEmpty) {
                if (GetTopLiquid().isMystery) {
                    GetTopLiquid().DeMysterize();
                }
            }
        }
        BottleSatisfy(nextBottle);
    }

    private void BottleSatisfy(Bottle nextbottle) {
        int i = 0;
        LiquidColor colorIndex = nextbottle.GetTopLiquid().colorId;
        foreach (LiquidUnit liquid in nextbottle.liquidUnits) {
            if (liquid.colorId == colorIndex) {
                i++;
            }
        }
        if (i == 4) {
            nextbottle.isCompleted = true;
        }
    }

}