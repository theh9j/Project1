using NUnit.Framework;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.Multiplayer.Center.Common;
using UnityEngine;

public class UIHandler : MonoBehaviour
{

    public LevelDesigner levelDesigner;
    public BottleGen bottleGen;

    [SerializeField]
    public GameObject[] colorBases = new GameObject[4];
    public TMP_Text adminHandler;
    public TMP_InputField bottleGenInput;

    private TMP_InputField[] colors;
    private TMP_Text[] mys;
    

    public bool admin = true;
    private bool selected = false;
    private Bottle bottle;
    private void Start() {
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
        if (result <= 0) {
            bottleGen.ClearBottles();
            return; 
        }
        bottleGen.ClearBottles();
        bottleGen.GenAmount(result);
    }

    public void SetBottle(int i) {
        if (mys[i].text == "True") {
            mys[i].text = "False";
        } else {
            mys[i].text = "True";
        }
    }

    public void NewDataForBottle() {
        if (!admin) return;
        if (Selection) {
            levelDesigner.SetColors(bottle, colors, mys);
            Debug.Log(colors.ToString());

        }
    }

    public void BottleSelectedChangeColor(Bottle currentbottle = null) {
        
        Selection = !Selection;
        if (selected) {
            bottle = currentbottle;
            List<LiquidUnit> liquids = bottle.liquidUnits;

            for (int i = 0; i < liquids.Count; i++) {
                LiquidUnit liquid = liquids[i];

                switch (liquid.colorId) {
                    case LiquidColor.Red:
                        colors[i].text = "Red";
                        break;
                    case LiquidColor.Green:
                        colors[i].text = "Green";
                        break;
                    case LiquidColor.Blue:
                        colors[i].text = "Blue";
                        break;
                    case LiquidColor.Yellow:
                        colors[i].text = "Yellow";
                        break;
                    case LiquidColor.Pink:
                        colors[i].text = "Pink";
                        break;
                    case LiquidColor.Purple:
                        colors[i].text = "Purple";
                        break;
                    case LiquidColor.Grey:
                        colors[i].text = "Grey";
                        break;
                    case LiquidColor.Brown:
                        colors[i].text = "Brown";
                        break;
                    case LiquidColor.Unknown:
                        colors[i].text = "Unknown";
                        break;
                    default:
                        colors[i].text = "Invalid/Empty";
                        break;
                }

                if (liquid.isMystery) {
                    mys[i].text = "True";

                } else {
                    mys[i].text = "False";
                }
            }
        } else {
            bottle = null;
            for (int i = 0; i < colors.Length; i++) {
                colors[i].text = $"Color{i}";
            }
        }

        
    }

    public void SetAdmin() {
        if (!admin) {
            adminHandler.text = "Handler Active";
        } else {
            adminHandler.text = "Handler Inactive";
        }
        admin = !admin;
    }

    public bool Selection {
        get { return selected; }
        private set { selected = value; }
    }
}
