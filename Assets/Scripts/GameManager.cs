using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BottleGen bottleGen;
    private Bottle from;
    private bool res;


    void Start() {

        //StartCoroutine(testCompletion());
        
    }

    //IEnumerator testCompletion() {
    //    yield return new WaitForSeconds(4);

    //    OnCompletion();
    //}

    public void OnCompletion() {


        Debug.Log("Game Completed!");

    }

    public bool BottleAvailable(Bottle currentBottle) {
        if (currentBottle.isLocked || currentBottle.Completion) return false;
        return true;
    }

    public void InGame() {
        int i = 0;
        foreach (Bottle bottle in bottleGen.bottles) {
            if (!bottle.IsEmpty || !bottle.Completion) {
                i++;
            }
        }
        if (i == 0) OnCompletion();
    }

    public void TryPour(Bottle to) {
        if (to.isOccupied) return;
        to.isOccupied = true;

        if (from == null) {
            if (to.IsEmpty) {
                to.anim.Play(1);
                to.isOccupied = false;
                return;
            }
            from = to;
            from.anim.SelectedHover(true);
            to.isOccupied = false;
            return;
        } else if (from == to) {
            from.anim.SelectedHover(false);
            from = null;
            to.isOccupied = false;
            return;
        }
        res = from.Pour(to);
        if (res) {
            from.anim.Play(2, to);
            from = null;
        } else {
            to.anim.Play(1);
        }
        to.isOccupied = false;
    }

    
}
