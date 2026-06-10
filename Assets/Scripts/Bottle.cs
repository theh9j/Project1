using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bottle : MonoBehaviour {

    public AnimationHandler anim;
    private BottleView bottleView;
    private readonly ColorTranslator colorTranslate = new();
    public List<LiquidUnit> liquidUnits = new List<LiquidUnit>();
    public UnityEvent<bool> onBottlePour;
    public UnityEvent<Bottle> aBottleCovered;
    


    public bool isLocked = false;
    public LiquidColor lockColor;


    public int maxCapacity = 4;
    public bool isOccupied = false;
    public int changes;
    private bool isCompleted = false;

    public void BottleInit(List<LiquidUnit> initialLiquids) {
        liquidUnits = new List<LiquidUnit>();

        foreach (LiquidUnit liquid in initialLiquids) {
            liquidUnits.Add(new LiquidUnit(liquid));
        }
    }

    public void AttemptComplete() {
        Completion = true;
        onBottlePour?.Invoke(true);
        anim.Play(4, null, transform.position + Vector3.up * 2.75f);
    }

    public void RemoveConditionalLock() {
        lockColor = LiquidColor.unknown;
        isLocked = false;
        anim.Play(5, null, transform.position + Vector3.up * 10f);
    }

    public void SetLocker(LiquidColor color, bool quick = false) {
        isLocked = true;
        lockColor = color;
        Transform cover = transform.Find("Visual/Cover").transform;
        cover.gameObject.SetActive(true);
        if (quick) {
            anim.AddCoverQ(colorTranslate.GetColor(color));
        } else {
            anim.AddCoverS(colorTranslate.GetColor(color));
        }

        aBottleCovered?.Invoke(this);   
    }

    public LiquidUnit GetTopLiquid() {
        if (IsEmpty) return null;
        return liquidUnits[^1];
    }

    public PourData Shuffle() {
        PourData shuffled = new();
        if (liquidUnits.Count <= 1) return shuffled;

        shuffled.shuffle = this;
        for (int i = 0; i < liquidUnits.Count; i++) {
            shuffled.prior.Add(liquidUnits[i]);
        }


        for (int i = 0; i < liquidUnits.Count; i++) {
            int randomIndex = Random.Range(i, liquidUnits.Count);

            LiquidUnit temp = liquidUnits[i];
            liquidUnits[i] = liquidUnits[randomIndex];
            liquidUnits[randomIndex] = temp;
        }

        if (GetTopLiquid().isMystery) {
            GetTopLiquid().DeMysterize();
        }

        RefreshView();
        return shuffled;
    }

    public LiquidUnit RemoveTopLiquid() {
        if (Completion) { Completion = false; anim.PlayUnCap(); }
        LiquidUnit topLiquid = GetTopLiquid();
        liquidUnits.RemoveAt(liquidUnits.Count - 1);
        return topLiquid;
    }

    public bool CanPourTo(Bottle nextBottle) {
        if (nextBottle == null) return false;

        if (isLocked || nextBottle.isLocked) return false;
        if (isCompleted || nextBottle.isCompleted) return false;
        if (IsEmpty) return false;
        if (nextBottle.IsFull) return false;
                
        LiquidUnit myTop = GetTopLiquid();
        LiquidUnit targetTop = nextBottle.GetTopLiquid();

        if (targetTop == null) return true;

        return myTop.colorId == targetTop.colorId;
    }

    public PourData Pour(Bottle nextBottle) {
        PourData move = null;
        if (!CanPourTo(nextBottle) || nextBottle.anim.IsBusy) return move;
        changes = 1;
        LiquidColor pourColor = GetTopLiquid().colorId;
        move = new() {
            from = this,
            to = nextBottle
        };
        while (true) {
            if (IsEmpty) break;

            if (nextBottle.IsFull) break;

            if (GetTopLiquid().colorId != pourColor) break;

            LiquidUnit topLiquid = GetTopLiquid();
            
            liquidUnits.RemoveAt(liquidUnits.Count - 1);
            move.movedLiquids.Add(topLiquid);
            nextBottle.liquidUnits.Add(new LiquidUnit(topLiquid));

            changes++;

            if (!IsEmpty) {
                if (GetTopLiquid().isMystery) {
                    GetTopLiquid().DeMysterize();
                }
            }
        }
        nextBottle.changes = -changes;
        BottleSatisfy(nextBottle);
        return move;
    }

    

    private void BottleSatisfy(Bottle nextbottle) {
        int i = 0;
        LiquidColor colorIndex = nextbottle.GetTopLiquid().colorId;
        foreach (LiquidUnit liquid in nextbottle.liquidUnits) {
            if (liquid.colorId == colorIndex) {
                i++;
            }
        }
        if (i == 4) {
            nextbottle.AttemptComplete();
        } else onBottlePour?.Invoke(false);
    }

    private void Awake() {
        bottleView = GetComponent<BottleView>();
    }

    private void Start() { 
        RefreshView();
    }

    public void RefreshView() {
        bottleView.Refresh(liquidUnits);
    }

    public bool Completion {
        get { return isCompleted; }
        private set { isCompleted = value; }
    }

    public bool IsEmpty => liquidUnits.Count == 0;
    private bool IsFull => liquidUnits.Count >= maxCapacity;
}