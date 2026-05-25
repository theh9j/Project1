using UnityEngine;
[System.Serializable]

public class LiquidUnit
{
    public int colorId;
    public bool isMystery;

    public LiquidUnit(int colorId, bool isMystery = false)
    {
        this.colorId = colorId;
        this.isMystery = isMystery;
    }

    public void deMysterize()
    {
        isMystery = false;
    }
}
