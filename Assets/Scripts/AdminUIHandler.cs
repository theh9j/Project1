using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class AdminUIHandler : MonoBehaviour
{

    public LevelDesigner levelDesigner;
    public BottleGen bottleGen;

    [SerializeField]
    public GameObject[] colorBases = new GameObject[4];
    public TMP_Text adminHandler;
    public TMP_InputField bottleGenInput;
    public TMP_InputField levelInput;
    public TMP_InputField lockInput;

    private TMP_InputField[] colors;
    private TMP_Text[] mys;
    

    public bool admin = true;
    private bool selected = false;
    private TMP_Text lockText;
    private Bottle bottle;

    private void Start() {
        lockText = lockInput.transform.Find("Lock/Text (TMP)").GetComponent<TMP_Text>();
        colors = new TMP_InputField[colorBases.Length];
        mys = new TMP_Text[colorBases.Length];


        for (int i = 0; i < colorBases.Length; i++) {
            colors[i] = colorBases[i].GetComponent<TMP_InputField>();
            mys[i] = colorBases[i].transform.Find("Mys/Text (TMP)").GetComponent<TMP_Text>();
        }

        if (admin) {
            adminHandler.text = "Handler Active";
        } else {
            adminHandler.text = "Handler Inactive";
        }
    }

    public void GenBottle() {
        
        bottleGenInput.text = bottleGenInput.text.Replace(" ", "");
        string gen = bottleGenInput.text;
        if (!int.TryParse(gen, out int result)) {
            Debug.Log("NaN");
            return;
        }
        bottleGen.GenAmount(result);
    }

    public void SetBottle(int i) {
        if (mys[i].text == "True") {
            mys[i].text = "False";
        } else {
            mys[i].text = "True";
        }
    }

    public void AddBottle() {
        bottleGen.AddBottle();
    }

    public void LockBottle() {
        if (!bottle) return;
        if (lockText.text == "Lock") {
            
            levelDesigner.BottleConditioning(bottle, out bool res, lockInput.text);
            if (res) lockText.text = "Unlock";
        } else {
            levelDesigner.BottleConditioning(bottle, out _);
            lockText.text = "Lock";
        }
    }

    public void RemoveLiquid(int i) {
        levelDesigner.RemoveColor(bottle, i, out List<LiquidUnit> liquidUnits);

        for (int j = 0; j < liquidUnits.Count; j++) {
            SetColor(liquidUnits[j], colors[j], mys[j]);
        }

        for (int u = liquidUnits.Count; (u >= liquidUnits.Count) && (u < colors.Length); u++) {
            colors[u].text = $"Color{u}";
        }
    }

    public void NewDataForBottle() {
        if (!admin) return;
        if (Selection) {
            levelDesigner.SetColors(bottle, colors, mys);

        }
    }
    
    public void SetLevel(string level) {
        levelInput.text = level;
    }

    public void BottleSelectedChangeColor(Bottle currentBottle = null) {
        Selection = currentBottle != null;

        if (Selection) {
            bottle = currentBottle;

            List<LiquidUnit> liquids = bottle.liquidUnits;

            for (int i = 0; i < colors.Length; i++) {
                if (i < liquids.Count) {
                    SetColor(liquids[i], colors[i], mys[i]);
                } else {
                    colors[i].text = $"Color{i}";
                }
            }

            lockText.text = bottle.isLocked ? "Unlock" : "Lock";
        } else {
            bottle = null;

            for (int i = 0; i < colors.Length; i++) {
                colors[i].text = $"Color{i}";
            }

            lockText.text = "Lock";
        }
    }

    public void SetAdmin() {
        if (!admin) {
            adminHandler.text = "Handler Active";
        } else {
            adminHandler.text = "Handler Inactive";
        }
        admin = !admin;
        Selection = false;
    }

    

    private void SetColor(LiquidUnit liquid, TMP_InputField color, TMP_Text mys) {

        switch (liquid.colorId) {
            case LiquidColor.red:
                color.text = "Red";
                break;
            case LiquidColor.green:
                color.text = "Green";
                break;
            case LiquidColor.blue:
                color.text = "Blue";
                break;
            case LiquidColor.yellow:
                color.text = "Yellow";
                break;
            case LiquidColor.pink:
                color.text = "Pink";
                break;
            case LiquidColor.purple:
                color.text = "Purple";
                break;
            case LiquidColor.grey:
                color.text = "Grey";
                break;
            case LiquidColor.brown:
                color.text = "Brown";
                break;
            case LiquidColor.unknown:
                color.text = "Unknown";
                break;
            default:
                color.text = "Invalid/Empty";
                break;
        }

        if (liquid.isMystery) {
            mys.text = "True";

        } else {
            mys.text = "False";
        }
    }

    public string LevelInput {
        get { return levelInput.text; }
    }
    public bool Selection {
        get { return selected; }
        private set { selected = value; }
    }
}
