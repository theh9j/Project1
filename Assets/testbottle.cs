using System.Collections.Generic;
using UnityEngine;

public class BottleTest : MonoBehaviour {
    [SerializeField] private Bottle bottleA;
    [SerializeField] private Bottle bottleB;

    private void Start() {
        bottleA.BottleInit(new List<LiquidUnit>
        {
            new LiquidUnit(LiquidColor.Red),
            new LiquidUnit(LiquidColor.Blue),
            new LiquidUnit(LiquidColor.Blue),
            new LiquidUnit(LiquidColor.Blue)
        });

        bottleB.BottleInit(new List<LiquidUnit>
        {
            new LiquidUnit(LiquidColor.Blue)
        });

        Debug.Log("Before pour:");
        Debug.Log("Bottle A count: " + bottleA.CurrentCapacity());
        Debug.Log("Bottle B count: " + bottleB.CurrentCapacity());
        for (int i = 0; i < bottleA.CurrentCapacity(); i++) {
            Debug.Log("Bottle A unit " + i + ": " + bottleA.liquidUnits[i].colorId);
        }
        for (int i = 0; i < bottleB.CurrentCapacity(); i++) {
            Debug.Log("Bottle B unit " + i + ": " + bottleB.liquidUnits[i].colorId);
        }

        bottleA.Pour(bottleB);

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