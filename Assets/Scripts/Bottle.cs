using System.Collections.Generic;
using System.Linq;
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

    public void BottleInit(List<LiquidUnit> initialLiquids) { //DEPRECATED
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

    public bool Distinction() {
        int distinctColors = liquidUnits.Select(x => x.colorId)
            .Distinct() 
            .Count();
        return distinctColors > 1;
    }

    public LiquidUnit GetTopLiquid() {
        if (IsEmpty) return null;
        return liquidUnits[^1];
    }

    public PourData Shuffle() {
        if (!Distinction()) return null;

        PourData shuffled = new() { shuffle = this };
        for (int i = 0; i < liquidUnits.Count; i++) {
            shuffled.prior.Add(liquidUnits[i]);
        }

        List<LiquidUnit> original = liquidUnits.Select(
            x => new LiquidUnit( x.colorId, x.isMystery )).ToList();

        int attempts = 0;

        do {
            Swap(liquidUnits);
            attempts++;
        }
        while (
            SameOrder(original, liquidUnits) && attempts < 50
        );

        if (GetTopLiquid().isMystery) {
            GetTopLiquid().DeMysterize();
        }

        RefreshView();
        onBottlePour?.Invoke(false);
        return shuffled;
    }

    private void Swap(List<LiquidUnit> list) {
        for (int i = list.Count - 1; i > 0; i--) {
            int random = Random.Range(0, i + 1);

            (list[i], list[random]) = (list[random], list[i]);
        }
    }

    private bool SameOrder(List<LiquidUnit> a, List<LiquidUnit> b) {
        if (a.Count != b.Count) return false;

        for (int i = 0; i < a.Count; i++) {
            if (a[i].colorId != b[i].colorId) return false;
            if (a[i].isMystery != b[i].isMystery) return false;
        }

        return true;

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