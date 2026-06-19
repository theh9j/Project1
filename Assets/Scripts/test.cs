using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField] Bottle bottle1;
    [SerializeField] Bottle bottle2;

    void Start() {
        bottle1.liquidUnits.Add(new LiquidUnit(LiquidColor.yellow));
        bottle1.liquidUnits.Add(new LiquidUnit(LiquidColor.yellow));
        bottle1.liquidUnits.Add(new LiquidUnit(LiquidColor.yellow));

        bottle1.RefreshView();
        bottle2.RefreshView();
    }

    
}
