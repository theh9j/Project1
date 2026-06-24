using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BottleGen bottleGen;
    [SerializeField] private LevelCreator levelCreator;
    [SerializeField] private Tutorial tutor;
    [SerializeField] private float timeScaler = 1f;
    public bool play = true;
    public bool tutorial = true;
    public UnityEvent revive;
    public UnityEvent<int, int> gameOver;
    public UnityEvent nextStep;
    private Bottle from;
    private Dictionary<LiquidColor, List<Bottle>> conditionalBottles = new();
    private Stack<PourData> record;
    private PourData move; 
    
    
    public int currentLevel = 0;
    
    void Awake() {
        Screen.orientation = ScreenOrientation.Portrait;
    }

    void Start() {

        bottleGen.newBot.AddListener((Bottle newBottle) => {
            newBottle.aBottleCovered.AddListener(ConditionalBottleRecord);
            newBottle.onBottlePour.AddListener(CheckForImpossibility);
        });

        CameraOrtho(20);
        if (play) OnGameStart(false, true);
    }

    void Update() {
        Time.timeScale = timeScaler;
    }

    public void ContinueToNextLevel() {
        OnGameStart(false, false);
    }

    private bool OnCompletion() {
        int completedLevel = SaveManager.Instance.level;
        int coinReward = SaveManager.Instance.coinsReward;

        SaveManager.Instance.coins += SaveManager.Instance.coinsReward;
        SaveManager.Instance.shuffle += SaveManager.Instance.shufflesReward;
        SaveManager.Instance.undo += SaveManager.Instance.undosReward;
        SaveManager.Instance.addBottle += SaveManager.Instance.addBottlesReward;

        SaveManager.Instance.level++;

        levelCreator.DeleteSavedLayout();
        SaveManager.Instance.SaveData(false);

        conditionalBottles.Clear();

        gameOver?.Invoke(completedLevel, coinReward);

        Debug.Log("Game Completed!");
        return true;
    }

    public void CameraOrtho(float target) {
        Camera.main
            .DOOrthoSize(target, .3f)
            .SetEase(Ease.InOutSine, 1f);
    }

    private void CheckForImpossibility(bool comp) {
        if (comp) comp = CheckForComplete();
        if (comp) return;

        if (!LevelProcessor.CheckForPossibleMoves(bottleGen.DictionaryToSingularBottleConverter())) {
            gameOver?.Invoke(SaveManager.Instance.level, 0);
        }
    }

    private bool IsStillPlayable() {
        List<Bottle> bottles = bottleGen.DictionaryToSingularBottleConverter();

        foreach (Bottle bottle in bottles) {
            if (bottle == null) continue;
            if (bottle.IsEmpty && !bottle.isLocked) return true;
        }

        foreach (Bottle start in bottles) {
            if (!CanPourOut(start)) continue;

            foreach (Bottle end in bottles) {
                if (CanPourIn(start, end)) return true;
            }
        }
        return false;
    }

    private bool CanPourOut(Bottle bottle) {
        if (bottle == null) return false;
        if (bottle.IsEmpty) return false;
        if (bottle.isLocked) return false;
        if (bottle.Completion) return false;

        return true;
    }

    private bool CanPourIn(Bottle start, Bottle end) {
        if ((start == null) || (end == null)) return false;
        if (start == end) return false;

        if (end.isLocked) return false;
        if (end.Completion) return false;
        if (end.IsFull) return false;

        LiquidUnit startTop = start.GetTopLiquid();
        LiquidUnit endTop = end.GetTopLiquid();

        if (startTop == null || endTop == null) return false;
        return startTop.colorId == endTop.colorId;

    }

    public void Revival() {
        revive?.Invoke();
    }

    public void ADGameStart(bool rand) {
        OnGameStart(rand, false);
    }

    public void OnGameStart(bool rand, bool byLayout) {
        record = new Stack<PourData>();
        CameraOrtho(20);
        conditionalBottles.Clear();

        levelCreator.LoadLevel(rand, byLayout);
        tutor.CheckForTutorial(tutorial);
    }

    public bool BottleAvailable(Bottle currentBottle) {
        if (currentBottle == null) return false;
        if (currentBottle.isOccupied) return false;
        if (currentBottle.anim != null && currentBottle.anim.IsBusy) return false;
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

    public bool ShuffleBottle(Bottle bottle) {
        if (bottle == null) return false;
        if (bottle.IsEmpty) return false;
        if (bottle.Completion) return false;
        if (bottle.isLocked) return false;

        PourData shuffled = bottle.Shuffle(bottleGen.DictionaryToSingularBottleConverter());
        record.Push(shuffled);  
        return true;
    }

    public void TryPour(Bottle to) {
        if (to == null) return;

        if (SaveManager.Instance.level == 0 && tutor.firstEver) {
            if (TutorialTryPour(to)) nextStep?.Invoke();
            return;
        }

        if (to.isOccupied) return;

        if (from == null) {
            if (to.anim.IsUnavailable) return;
            if (to.IsEmpty || to.Completion || to.isLocked) {
                to.anim.Play(1);
                return;
            }

            from = to;
            from.anim.SelectedHover(true);
            return;
        }

        if (from == to) {
            from.anim.SelectedHover(false);
            from = null;
            return;
        }

        move = from.Pour(to);

        if (move != null) {
            record.Push(move);

            Bottle pourFrom = from;
            Bottle pourTo = to;

            pourFrom.anim.SelectedHover(false);
            pourFrom.isOccupied = true;

            from = null;

            pourFrom.anim.Play(2, pourTo, default, () =>
            {
                pourFrom.isOccupied = false;
            });

            return;
        }

        to.anim.Play(1);
    }


    private bool TutorialTryPour(Bottle to) {
        List<Bottle> bottles = bottleGen.DictionaryToSingularBottleConverter();
        if (from == null && to == bottles[0]) {
            from = to;
            from.anim.SelectedHover(true);
            to.isOccupied = false;
            return true;
        }
        if (from != null && to == bottles[2]) {
            move = from.Pour(to);
            if (move != null) {
                record.Push(move);
                from.anim.Play(2, to);
                from = null;
                tutor.firstEver = false;
                return true;
            } else {
                to.anim.Play(1);
            }
        }
        return false;
    }

    public bool Undo() {
        if (record.Count == 0) return false;

        PourData move = record.Pop();

        if (move.shuffle != null) {
            for (int i = 0; i < move.prior.Count; i++) {
                move.shuffle.liquidUnits[i] = move.prior[i];
            }
            move.shuffle.RefreshView();
            return true;
        }

        for (int i = move.movedLiquids.Count -1; i >= 0; i--) {
            LiquidUnit liquid = move.to.RemoveTopLiquid();
            move.from.liquidUnits.Add(liquid);
        }

        if (record.Count > 0) {
            if (record.Peek().deconditionalize.Count > 0) {
                PourData cond = record.Pop();
                foreach (Bottle bottle in cond.deconditionalize) {
                    bottle.SetLocker(cond.colorFinishes);
                }
            }
        }


        move.from.RefreshView();
        move.to.RefreshView();
        return true;
    }

    private void TryRemoveConditioner(Bottle completedBottle) {
        LiquidColor bottleColor = completedBottle.GetTopLiquid().colorId;

        if (conditionalBottles.TryGetValue(bottleColor, out List<Bottle> satisfyBottles)) {
            move = new();
            foreach (Bottle bottle in satisfyBottles) {
                bottle.RemoveConditionalLock();
                move.deconditionalize.Add(bottle);
            }
            move.colorFinishes = bottleColor;
            record.Push(move);
        }
    }
}
