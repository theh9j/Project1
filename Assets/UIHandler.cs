using NUnit.Framework;
using System;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class UIHandler : MonoBehaviour
{

    [SerializeField] private BottleGen bottleGen;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TMP_Text frontCoinText;
    [SerializeField] private TMP_Text backCoinText;
    [SerializeField] private UIAnimation uianim;

    private Color textColor;

    void Start() {
        Coin(PlayerPrefs.GetInt("Coins"));

        gameManager.gameOver.AddListener(CheckForEnough);

    }

    public void ReviveUsingBottle() {

        int coins = PlayerPrefs.GetInt("Coins");
        if (coins < 900) return; //It should says that they don't have enough
        coins = coins - 900;
        PlayerPrefs.SetInt("Coins", coins);

        bottleGen.AddBottle();
        gameManager.Revival();
        PlayerPrefs.Save();
        Coin(coins);
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

    private void Coin(int coin) {
        string coint = coin.ToString();
        frontCoinText.text = coint;
        backCoinText.text = coint;
    }

}
