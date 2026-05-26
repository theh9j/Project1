using UnityEngine;
[System.Serializable]

public class LiquidUnit
{
    public LiquidColor colorId;
    public bool isMystery;

    public LiquidUnit(LiquidUnit liquidUnit) {
        colorId = liquidUnit.colorId;
        isMystery = liquidUnit.isMystery;
    }

    public LiquidUnit(LiquidColor colorId, bool isMystery = false)
    {
        this.colorId = colorId;
        this.isMystery = isMystery;
    }

    public void DeMysterize()
    {
        isMystery = false;
    }
}
