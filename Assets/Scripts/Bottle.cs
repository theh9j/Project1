using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Bottle : MonoBehaviour {
    [Header("References")]
    public AnimationHandler anim;
    [SerializeField] private BottleView bottleView;

    [Header("Liquid Data")]
    public List<LiquidUnit> liquidUnits = new List<LiquidUnit>();
    public int maxCapacity = 4;

    [Header("Events")]
    public UnityEvent<bool> onBottlePour;
    public UnityEvent<Bottle> aBottleCovered;

    [Header("Lock Data")]
    public bool isLocked = false;
    public LiquidColor lockColor;

    [Header("State")]
    public bool isOccupied = false;
    public int changes = 0;

    [SerializeField] private bool isCompleted = false;
    [SerializeField] private LiquidColorVisualData colorTranslate;

    private void Awake() {

        if (anim == null)
            anim = GetComponent<AnimationHandler>();
    }

    private void Start() {
        RefreshView();
    }

    public void RefreshView() {
        if (bottleView != null)
            bottleView.Refresh(liquidUnits);
    }

    public LiquidUnit GetTopLiquid() {
        if (IsEmpty) return null;
        return liquidUnits[^1];
    }

    public LiquidColor GetTopColor() {
        LiquidUnit top = GetTopLiquid();
        return top != null ? top.colorId : default;
    }

    public int GetAdjacentColorCount() {
        if (IsEmpty || Completion) return 5;             //Fallback
        LiquidColor c = GetTopColor();
        int u = 0;
        for (int i = liquidUnits.Count-1; i >= 0; i--) {
            if (liquidUnits[i].colorId == c) {
                u++;
                continue;
            }
            break;
        }
        return u;
    }

    public void AttemptComplete() {
        if (Completion) return;
        Completion = true;

        onBottlePour?.Invoke(true);

        if (anim != null)
            anim.Play(4);
    }

    public void RemoveConditionalLock() {
        isLocked = false;

        if (anim != null)
            anim.Play(5, null, transform.position + Vector3.up * 10f);
    }

    public void SetLocker(LiquidColor color, bool quick = false) {
        isLocked = true;
        lockColor = color;

        if (anim != null) {
            if (quick)
                anim.AddCoverQ(colorTranslate.GetColor(color));
            else
                anim.AddCoverS(colorTranslate.GetColor(color));
        }

        aBottleCovered?.Invoke(this);
    }

    public bool Distinction() {
        return liquidUnits
            .Select(x => x.colorId)
            .Distinct()
            .Count() > 1;
    }

    public PourData Shuffle(List<Bottle> allBottles) {
        if (!Distinction()) return null;

        PourData shuffled = new PourData {
            shuffle = this
        };

        for (int i = 0; i < liquidUnits.Count; i++) {
            shuffled.prior.Add(new LiquidUnit(liquidUnits[i]));
        }

        List<LiquidUnit> original = liquidUnits
            .Select(x => new LiquidUnit(x.colorId, x.isMystery))
            .ToList();

        int attempts = 0;

        do {
            SmartSwap(liquidUnits, allBottles);
            attempts++;
        }
        while (SameOrder(original, liquidUnits) && attempts < 50);

        LiquidUnit top = GetTopLiquid();

        if (top != null && top.isMystery)
            top.DeMysterize();

        RefreshView();
        onBottlePour?.Invoke(false);

        return shuffled;
    }

    private void SmartSwap(List<LiquidUnit> list, List<Bottle> allBottles) {
        LiquidUnit originalTop = GetTopLiquid();
        if (originalTop == null) {
            Swap(list);
            return;
        }

        LiquidColor originalTopColor = originalTop.colorId;

        List<LiquidColor> usefulColors = new List<LiquidColor>();

        foreach (Bottle bottle in allBottles) {
            if (bottle == null) continue;
            if (bottle == this) continue;
            if (bottle.IsEmpty) continue;
            if (bottle.isLocked) continue;
            if (bottle.Completion) continue;

            LiquidUnit otherTop = bottle.GetTopLiquid();
            if (otherTop == null) continue;

            LiquidColor otherTopColor = otherTop.colorId;

            if (otherTopColor == originalTopColor) continue;

            for (int i = 0; i < list.Count; i++) {
                if (list[i].colorId == otherTopColor &&
                    !usefulColors.Contains(otherTopColor)) {
                    usefulColors.Add(otherTopColor);
                }
            }
        }

        Swap(list);

        if (usefulColors.Count == 0) return;

        LiquidColor chosenColor =
            usefulColors[Random.Range(0, usefulColors.Count)];

        for (int i = 0; i < list.Count; i++) {
            if (list[i].colorId == chosenColor) {
                (list[i], list[^1]) = (list[^1], list[i]);
                return;
            }
        }
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
        if (IsEmpty) return null;

        if (Completion) {
            Completion = false;

            if (anim != null)
                anim.PlayUnCap();
        }

        LiquidUnit topLiquid = GetTopLiquid();
        liquidUnits.RemoveAt(liquidUnits.Count - 1);

        return topLiquid;
    }

    public void CheckCompleteOnLoad() {
        if (IsEmpty || liquidUnits.Count < maxCapacity)
            return;

        LiquidColor firstColor = liquidUnits[0].colorId;

        for (int i = 1; i < liquidUnits.Count; i++) {
            if (liquidUnits[i].colorId != firstColor)
                return;
        }

        Debug.Log("I've been played");
        Completion = true;
        anim.SetCap();
    }

    public bool CanPourTo(Bottle nextBottle) {
        if (nextBottle == null) return false;

        if (isLocked || nextBottle.isLocked) return false;
        if (Completion || nextBottle.Completion) return false;
        if (IsEmpty) return false;
        if (nextBottle.IsFull) return false;

        LiquidUnit myTop = GetTopLiquid();
        LiquidUnit targetTop = nextBottle.GetTopLiquid();

        if (myTop == null) return false;
        if (targetTop == null) return true;

        return myTop.colorId == targetTop.colorId;
    }

    public PourData Pour(Bottle nextBottle) {
        if (!CanPourTo(nextBottle)) return null;
        if (anim.IsBusy || nextBottle.anim.IsBusy) return null;

        changes = 0;

        LiquidColor pourColor = GetTopLiquid().colorId;

        PourData move = new PourData {
            from = this,
            to = nextBottle
        };

        while (true) {
            if (IsEmpty) break;
            if (nextBottle.IsFull) break;
            if (GetTopLiquid().colorId != pourColor) break;

            LiquidUnit topLiquid = RemoveTopLiquid();

            if (topLiquid == null) break;

            move.movedLiquids.Add(new LiquidUnit(topLiquid));
            nextBottle.liquidUnits.Add(new LiquidUnit(topLiquid));

            changes++;

            LiquidUnit newTop = GetTopLiquid();

            if (newTop != null && newTop.isMystery)
                newTop.DeMysterize();
        }

        nextBottle.changes = -changes;

        return move;
    }

    public void BottleSatisfy(Bottle nextBottle) {
        if (nextBottle == null || nextBottle.IsEmpty) return;

        LiquidColor colorIndex = nextBottle.GetTopLiquid().colorId;

        int count = 0;

        foreach (LiquidUnit liquid in nextBottle.liquidUnits) {
            if (liquid.colorId == colorIndex)
                count++;
        }

        if (count == maxCapacity && nextBottle.liquidUnits.Count == maxCapacity) {
            nextBottle.AttemptComplete();
        } else {
            onBottlePour?.Invoke(false);
        }
    }

    public bool Completion {
        get => isCompleted;
        private set => isCompleted = value;
    }

    public bool IsEmpty => liquidUnits.Count == 0;
    public bool IsFull => liquidUnits.Count >= maxCapacity;
}