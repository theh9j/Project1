using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BottleGen bottleGen;
    private Bottle from;
    private Dictionary<LiquidColor, List<Bottle>> conditionalBottles = new();


    void Start() {

        bottleGen.newBot.AddListener((Bottle newBottle) => {
            newBottle.onBottleCompletion.AddListener(CheckForComplete);
        });

    }


    //IEnumerator testCompletion() {
    //    yield return new WaitForSeconds(4);

    //    OnCompletion();
    //}

    public void OnCompletion() {
        conditionalBottles.Clear();

        Debug.Log("Game Completed!");

    }

    public bool BottleAvailable(Bottle currentBottle) {
        if (currentBottle.isLocked || currentBottle.Completion) return false;
        return true;
    }

    public void ConditionalBottleRecord(List<Bottle> bottles) {
        foreach (Bottle bottle in bottles) {
            if (!bottle.isLocked) return;
            if (!conditionalBottles.ContainsKey(bottle.lockColor)) {
                conditionalBottles[bottle.lockColor] = new List<Bottle>();
            }

            conditionalBottles[bottle.lockColor].Add(bottle);
        }
    }

    public void CheckForComplete() {
        int i = 0;
        foreach (Bottle bottle in bottleGen.bottles) {
            if (!bottle.IsEmpty && !bottle.Completion) {
                i++;
            }

            if (bottle.Completion) {
                TryRemoveConditioner(bottle);
            }
        }
        if (i == 0) OnCompletion();
    }

    public void TryPour(Bottle to) {
        if (to.isOccupied) return;
        to.isOccupied = true;

        if (from == null) {
            if (to.IsEmpty || to.Completion) {
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
        bool res = from.Pour(to);
        if (res) {
            from.anim.Play(2, to);
            from = null;
        } else {
            to.anim.Play(1);
        }
        to.isOccupied = false;
    }

    private void TryRemoveConditioner(Bottle completedBottle) {
        LiquidColor bottleColor = completedBottle.GetTopLiquid().colorId;

        if (conditionalBottles.TryGetValue(bottleColor, out List<Bottle> satisfyBottles)) {
            foreach (Bottle bottle in satisfyBottles) {
                bottle.RemoveConditionalLock();
            }
        }
    }
}
