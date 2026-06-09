using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BottleGen bottleGen;
    [SerializeField] private LevelCreator levelCreator;
    public UnityEvent revive;
    public UnityEvent<int, int> gameOver;
    private Bottle from;
    private Dictionary<LiquidColor, List<Bottle>> conditionalBottles = new();
    public Stack<PourData> record = new(); 
    
    public int currentLevel = 0;

    [Header("Inventory")]
    public int coins = 1200;
    public int shuffle = 10;
    public int undo = 10;
    public int add = 10;


    void Awake() {

        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("Shuffle", shuffle);
        PlayerPrefs.SetInt("Undo", undo);
        PlayerPrefs.SetInt("Add", add);


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
        gameOver?.Invoke(currentLevel, PlayerPrefs.GetInt("Reward"));

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
            if (bottle.IsEmpty) { imp = false; break; }
            if (!compare.Add(bottle.GetTopLiquid().colorId)) imp = false;
        }
        if (imp) {
            gameOver?.Invoke(currentLevel, 0);
        }
    }

    public void Revival() {
        revive?.Invoke();
    }

    public void OnGameStart(bool next) {
        conditionalBottles.Clear();
        if (next) currentLevel++;
        levelCreator.LoadLevel(true);
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
        PourData move = from.Pour(to);
        if (move != null) {
            record.Push(move);
            from.anim.Play(2, to);
            from = null;
        } else {
            to.anim.Play(1);
        }
        to.isOccupied = false;
    }

    public bool Undo() {
        if (record.Count == 0) return false;

        PourData move = record.Pop();
        for (int i = move.movedLiquids.Count -1; i >= 0; i--) {
            LiquidUnit liquid = move.to.RemoveTopLiquid();
            move.from.liquidUnits.Add(liquid);
        }

        move.from.RefreshView();
        move.to.RefreshView();
        return true;
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
