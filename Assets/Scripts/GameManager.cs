using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BottleGen bottleGen;
    [SerializeField] private UIAnimation uianim;
    public UnityEvent revive;
    public UnityEvent<int, int> gameOver;
    private Bottle from;
    private Dictionary<LiquidColor, List<Bottle>> conditionalBottles = new();
    public int currentLevel = 0;


    void Awake() {

        PlayerPrefs.SetInt("Coins", 800);
        PlayerPrefs.Save();


    }

    void Start() {

        bottleGen.newBot.AddListener((Bottle newBottle) => {
            newBottle.aBottleCovered.AddListener(ConditionalBottleRecord);
            newBottle.onBottlePour.AddListener(CheckForImpossibility);
        });
    }

    private bool OnCompletion() {
        conditionalBottles.Clear();
        gameOver?.Invoke(currentLevel, 60);

        Debug.Log("Game Completed!");
        return true;
    }

    private void CheckForImpossibility(bool comp) {
        if (comp) comp = CheckForComplete();
        if (comp) return;

        List<Bottle> currentBottles = bottleGen.DictionaryToSingularBottleConverter();
        HashSet<LiquidColor> compare = new();
        bool imp = true;
        foreach (Bottle bottle in currentBottles) {
            if (!compare.Add(bottle.GetTopLiquid().colorId) || bottle.IsEmpty) {
                imp = false;
            }
        }
        if (imp) {
            gameOver?.Invoke(currentLevel, 100); //Amount Subject to Change
        }
    }

    public void Revival() {


        revive?.Invoke();
    }

    public void OnGameStart() {
        conditionalBottles.Clear();
    }

    public bool BottleAvailable(Bottle currentBottle) {
        if (currentBottle.isLocked || currentBottle.Completion) return false;
        return true;
    }

    public void ConditionalBottleRecord(Bottle bottle) {
        if (!conditionalBottles.ContainsKey(bottle.lockColor)) {
            conditionalBottles[bottle.lockColor] = new List<Bottle>();
        }

        conditionalBottles[bottle.lockColor].Add(bottle);
    }

    public bool CheckForComplete() {
        bool a = false;
        int i = 0;
        foreach (Bottle bottle in bottleGen.DictionaryToSingularBottleConverter()) {
            if (!bottle.IsEmpty && !bottle.Completion) {
                i++;
            }

            if (bottle.Completion) {
                TryRemoveConditioner(bottle);
            }
        }
        if (i == 0) a = OnCompletion();
        return a;
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
