using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BottleGen bottle;
    private List<Bottle> bottles = new List<Bottle>();

    void Start() {
        bottles = bottle.GenAmount(10);

        StartCoroutine(testCompletion());
        
    }

    IEnumerator testCompletion() {
        yield return new WaitForSeconds(4);

        OnCompletion();
    }

    public void OnCompletion() {
        if (!InGame()) return;
        int i = 0;
        foreach (Bottle bot in bottles) {
            if (!bot.isCompleted && !bot.IsEmpty) return;
            i++;

        }

        Debug.Log("Game Completed!");

    }

    public bool InGame() {
        int bottles = bottle.genAmount;
        if (bottles == 0) return false;
        return true;
    }
}
