using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class UIHandler : MonoBehaviour
{

    [SerializeField] private BottleGen bottleGen;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private InputHandler inputs;
    public Price price;

    [SerializeField] private TMP_Text frontCoinText;
    [SerializeField] private TMP_Text backCoinText;

    [SerializeField] private GameObject[] toolNotification = new GameObject[3];
    [SerializeField] private GameObject[] costDisplayer = new GameObject[3];

    private TMP_Text shuffleText;
    private TMP_Text undoText;
    private TMP_Text addText;

    private TMP_Text shufflePrice;
    private TMP_Text undoPrice;
    private TMP_Text addPrice;

    [SerializeField] private UIAnimation uianim;
    [SerializeField] private GameObject underlay;

    private Color textColor;

    void Start() {
        Coin();

        gameManager.gameOver.AddListener(CheckForEnough);
        gameManager.revive.AddListener(uianim.Revived);


        shuffleText = toolNotification[0].transform.GetChild(1).GetComponent<TMP_Text>();
        undoText = toolNotification[1].transform.GetChild(1).GetComponent<TMP_Text>();
        addText = toolNotification[2].transform.GetChild(1).GetComponent<TMP_Text>();

        shufflePrice = costDisplayer[0].transform.GetChild(2).GetComponent<TMP_Text>();
        undoPrice = costDisplayer[1].transform.GetChild(2).GetComponent<TMP_Text>();
        addPrice = costDisplayer[2].transform.GetChild(2).GetComponent<TMP_Text>();

        for (int i = 0; i < 3; i++) {
            UpdateCount(i);
        }

        
    }

    public void ReviveUsingBottle() {

        int coins = PlayerPrefs.GetInt("Coins");
        if (coins < price.bottlePrice) return; //It should says that they don't have enough
        coins = coins - price.bottlePrice;
        PlayerPrefs.SetInt("Coins", coins);

        bottleGen.AddBottle();
        gameManager.Revival();
        PlayerPrefs.Save();
        Coin();
    }

    private void CheckForEnough(int level, int amount = 0) {
        int coins = PlayerPrefs.GetInt("Coins");

        Button button = uianim.options.GetChild(0).GetComponent<Button>();
        TMP_Text buttonText = uianim.options.GetChild(0).GetChild(0).GetComponent<TMP_Text>();

        if (coins >= 900) {
            button.interactable = true;
            textColor = new Color32(65, 162, 69, 255);


        } else {
            button.interactable = false;
            textColor = new Color32(15, 56, 16, 255);
        }
        buttonText.fontMaterial.SetColor(
            ShaderUtilities.ID_OutlineColor,
            textColor
            );

        uianim.GameEnd(level, amount);
    }

    public void LevelAdvance() {
        PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + PlayerPrefs.GetInt("Reward"));
        PlayerPrefs.Save();
        Coin();
        gameManager.OnGameStart(true, true);
        uianim.NextLevel();
    }

    public void Replay() {
        gameManager.OnGameStart(true, false);
        uianim.Revived();
    }

    private void Coin() {
        string coint = PlayerPrefs.GetInt("Coins").ToString();
        frontCoinText.text = coint;
        backCoinText.text = coint;
    }

    public void AddBottle() {
        int i = 0;
        foreach (Bottle bottle in bottleGen.DictionaryToSingularBottleConverter()) {
            if (bottle.anim.IsBusy) { i++; break; }
        }
        if (PlayerPrefs.GetInt("Add") == 0 && PlayerPrefs.GetInt("Coins") < price.bottlePrice) i++;
        if (i != 0) return;
        bottleGen.AddBottle();

        if (PlayerPrefs.GetInt("Add") == 0) {
            PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - price.undoPrice);
        } else {
            PlayerPrefs.SetInt("Add", PlayerPrefs.GetInt("Add") - 1);
        }
        PlayerPrefs.Save();
        Coin();
        UpdateCount(2);
    }

    public void Shuffle() {
        if (PlayerPrefs.GetInt("Undo") == 0 && PlayerPrefs.GetInt("Coins") < price.undoPrice) return;
        foreach (Bottle bottle in inputs.bottleGen.DictionaryToSingularBottleConverter()) {
            if (bottle.IsEmpty) continue;
            if (bottle.Completion) continue;
            if (bottle.isLocked) continue;
            if (!bottle.Distinction()) continue;

            inputs.ToggleShuffleMode();
            break;
        }
    }

    public void ShuffleUpdate() {
        if (PlayerPrefs.GetInt("Shuffle") == 0) {
            PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - price.undoPrice);
        } else {
            PlayerPrefs.SetInt("Shuffle", PlayerPrefs.GetInt("Shuffle") - 1);
        }
        PlayerPrefs.Save();
        Coin();
        UpdateCount(0);
    }

    public void ShuffleUnderlay(bool type) {
        if (type) {
            if (underlay.activeSelf) return;
            underlay.SetActive(true);
            IncreaseBottleZIndex(true);
            underlay.transform.GetComponent<SpriteRenderer>().DOFade(.95f, .3f);
        } else {
            if (!underlay.activeSelf) return;
            underlay.transform.GetComponent<SpriteRenderer>().DOFade(0f, .4f).OnComplete(() => {
                underlay.SetActive(false);
                IncreaseBottleZIndex(false);
            });
        }
    }

    private void IncreaseBottleZIndex(bool a) {
        List<Bottle> objectBot = inputs.bottleGen.DictionaryToSingularBottleConverter();

        foreach (Bottle bottle in objectBot) {
            bottle.GetComponent<SortingGroup>().sortingOrder = 10;
            if (bottle.IsEmpty) continue;
            if (bottle.Completion) continue;
            if (bottle.isLocked) continue;
            if (!bottle.Distinction()) continue;

            if (a) bottle.GetComponent<SortingGroup>().sortingOrder = 15;
        }
    }

    public void Undo() {
        if (PlayerPrefs.GetInt("Undo") == 0 && PlayerPrefs.GetInt("Coins") < price.undoPrice) return;

        bool res = gameManager.Undo();
        if (!res) return;

        if (PlayerPrefs.GetInt("Undo") == 0) {
            PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - price.undoPrice);
        } else {
            PlayerPrefs.SetInt("Undo", PlayerPrefs.GetInt("Undo")-1);
        }
        PlayerPrefs.Save();
        Coin();
        UpdateCount(1);
    }

    private void UpdateCount(int type) {
        switch (type) {
            case 0:
                if (PlayerPrefs.GetInt("Shuffle") > 0) uianim.DisplayCost(costDisplayer[type], toolNotification[type], false); else uianim.DisplayCost(costDisplayer[type], toolNotification[type], true);
                shuffleText.text = PlayerPrefs.GetInt("Shuffle").ToString();
                shufflePrice.text = price.shufflePrice.ToString();
                break;
            case 1:
                if (PlayerPrefs.GetInt("Undo") > 0) uianim.DisplayCost(costDisplayer[type], toolNotification[type], false); else uianim.DisplayCost(costDisplayer[type], toolNotification[type], true);
                undoText.text = PlayerPrefs.GetInt("Undo").ToString();
                undoPrice.text = price.undoPrice.ToString();
                break;
            case 2:
                if (PlayerPrefs.GetInt("Add") > 0) uianim.DisplayCost(costDisplayer[type], toolNotification[type], false); else uianim.DisplayCost(costDisplayer[type], toolNotification[type], true);
                addText.text = PlayerPrefs.GetInt("Add").ToString();
                addPrice.text = price.bottlePrice.ToString();
                break;
        }
    }
 
}
