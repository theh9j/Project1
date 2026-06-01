using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleTest : MonoBehaviour {
    [SerializeField] private Bottle[] bottles;

    private void Awake() {
        bottles[0].BottleInit(new List<LiquidUnit>
        {
            new LiquidUnit(LiquidColor.red),
            new LiquidUnit(LiquidColor.blue),
            new LiquidUnit(LiquidColor.blue),
            new LiquidUnit(LiquidColor.blue)
        });

        bottles[1].BottleInit(new List<LiquidUnit>
        {
            new LiquidUnit(LiquidColor.blue)
        });

        bottles[2].BottleInit(new List<LiquidUnit>
        {
            new LiquidUnit(LiquidColor.purple, true),
            new LiquidUnit(LiquidColor.red, true),
            new LiquidUnit(LiquidColor.red),
            new LiquidUnit(LiquidColor.red, true)       
        });

        bottles[3].BottleInit(new List<LiquidUnit>());

        bottles[4].BottleInit(new List<LiquidUnit>
        {
            new LiquidUnit(LiquidColor.purple),
            new LiquidUnit(LiquidColor.purple, true),
            new LiquidUnit(LiquidColor.purple, true),
            new LiquidUnit(LiquidColor.green)
        });

        bottles[5].BottleInit(new List<LiquidUnit>
        {
            new LiquidUnit(LiquidColor.pink),
            new LiquidUnit(LiquidColor.green, true),
            new LiquidUnit(LiquidColor.green, true),
            new LiquidUnit(LiquidColor.green, true)
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