using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleTest : MonoBehaviour {
    [SerializeField] private Bottle[] bottles;

    private void Awake() {
        bottles[0].BottleInit(new List<LiquidUnit>
        {
            new LiquidUnit(LiquidColor.Red),
            new LiquidUnit(LiquidColor.Blue),
            new LiquidUnit(LiquidColor.Blue),
            new LiquidUnit(LiquidColor.Blue)
        });

        bottles[1].BottleInit(new List<LiquidUnit>
        {
            new LiquidUnit(LiquidColor.Blue)
        });

        bottles[2].BottleInit(new List<LiquidUnit>
        {
            new LiquidUnit(LiquidColor.Purple, true),
            new LiquidUnit(LiquidColor.Red, true),
            new LiquidUnit(LiquidColor.Red),
            new LiquidUnit(LiquidColor.Red, true)       
        });

        bottles[3].BottleInit(new List<LiquidUnit>());

        bottles[4].BottleInit(new List<LiquidUnit>
        {
            new LiquidUnit(LiquidColor.Purple),
            new LiquidUnit(LiquidColor.Purple, true),
            new LiquidUnit(LiquidColor.Purple, true),
            new LiquidUnit(LiquidColor.Green)
        });

        bottles[5].BottleInit(new List<LiquidUnit>
        {
            new LiquidUnit(LiquidColor.Pink),
            new LiquidUnit(LiquidColor.Green, true),
            new LiquidUnit(LiquidColor.Green, true),
            new LiquidUnit(LiquidColor.Green, true)
        });



        //StartCoroutine(Test(bottleA, bottleB));
    }

    IEnumerator Test(Bottle bottleA, Bottle bottleB) {
        yield return new WaitForSeconds(6);


        PourLog(bottleA, bottleB);
    }

    private void PourLog(Bottle bottleA, Bottle bottleB) {
        

        Debug.Log("After pour:");
        Debug.Log("Bottle A count: " + bottleA.CurrentCapacity());
        Debug.Log("Bottle B count: " + bottleB.CurrentCapacity());
        for (int i = 0; i < bottleA.CurrentCapacity(); i++) {
            Debug.Log("Bottle A unit " + i + ": " + bottleA.liquidUnits[i].colorId);
        }
        for (int i = 0; i < bottleB.CurrentCapacity(); i++) {
            Debug.Log("Bottle B unit " + i + ": " + bottleB.liquidUnits[i].colorId);
        }
        Debug.Log(bottleA.isCompleted.ToString());
        Debug.Log(bottleB.isCompleted.ToString());
    }
}