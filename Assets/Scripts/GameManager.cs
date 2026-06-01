using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BottleGen bottle;
    private List<Bottle> bottles = new List<Bottle>();
    private Bottle from;
    private bool res;


    void Start() {

        //StartCoroutine(testCompletion());
        
    }

    //IEnumerator testCompletion() {
    //    yield return new WaitForSeconds(4);

    //    OnCompletion();
    //}

    //public void OnCompletion() {
    //    if (!InGame()) return;
    //    int i = 0;
    //    foreach (Bottle bot in bottles) {
    //        if (!bot.isCompleted && !bot.IsEmpty) return;
    //        i++;

    //    }

    //    Debug.Log("Game Completed!");

    //}

    public bool BottleAvailable(Bottle currentBottle) {
        if (currentBottle.isLocked || currentBottle.isCompleted) return false;
        return true;
    }

    //public bool InGame() {
    //    int bottles = bottle.genAmount;
    //    if (bottles == 0) return false;
    //    return true;
    //}

    public void TryPour(Bottle to) {
        if (to.IsOccupied) return;
        to.IsOccupied = true;

        if (from == null) {
            if (to.IsEmpty) {
                to.anim.Play(1);
                to.IsOccupied = false;
                return;
            }
            Debug.Log("Selected bottle");
            from = to;
            from.anim.SelectedHover(true);
            to.IsOccupied = false;
            return;
        } else if (from == to) {
            from.anim.SelectedHover(false);
            from = null;
            to.IsOccupied = false;
            return;
        }
        res = from.Pour(to);
        if (res) {
            from.anim.Play(2, to);
            from = null;
            Debug.Log("Pouring successful");
        } else {
            to.anim.Play(1);
        }
        to.IsOccupied = false;
    }

    
}
