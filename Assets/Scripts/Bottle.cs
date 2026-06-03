using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bottle : MonoBehaviour {

    public AnimationHandler anim;
    private BottleView bottleView;
    private readonly ColorTranslator colorTranslate = new();
    public List<LiquidUnit> liquidUnits = new List<LiquidUnit>();
    public UnityEvent onBottleCompletion;


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
        onBottleCompletion?.Invoke();
        anim.Play(4, null, transform.position + Vector3.up * 2.75f);
    }

    public void RemoveConditionalLock() {
        lockColor = LiquidColor.unknown;
        isLocked = false;
        anim.Play(5, null, transform.position + Vector3.up * 10f);
    }

    public void SetLocker(LiquidColor color) {
        isLocked = true;
        lockColor = color;
        transform.GetChild(3).GetChild(0).GetComponent<SpriteRenderer>().color = colorTranslate.GetColor(color);
    }

    public int CurrentCapacity() {
        return liquidUnits.Count;
    }

    public LiquidUnit GetTopLiquid() {
        if (IsEmpty) return null;
        return liquidUnits[liquidUnits.Count - 1];
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

    public bool Pour(Bottle nextBottle) {
        
        if (!CanPourTo(nextBottle) || nextBottle.anim.IsBusy) return false;
        changes = 1;
        LiquidColor pourColor = GetTopLiquid().colorId;

        while (true) {
            if (IsEmpty) break;

            if (nextBottle.IsFull) break;

            if (GetTopLiquid().colorId != pourColor) break;

            LiquidUnit topLiquid = GetTopLiquid();

            liquidUnits.RemoveAt(liquidUnits.Count - 1);
            nextBottle.liquidUnits.Add(new LiquidUnit(topLiquid));
            changes++;

            if (!IsEmpty) {
                if (GetTopLiquid().isMystery) {
                    GetTopLiquid().DeMysterize();
                }
            }
        }
        BottleSatisfy(nextBottle);
        return true;
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
        }
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