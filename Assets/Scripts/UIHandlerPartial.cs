using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class UIHandler : MonoBehaviour
{
    [SerializeField] private Transform popup;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button watchAdButton;
    [SerializeField] private Button cancelButton;

    private Action onBuy;
    private Action onWatchAd;
    private Action onCancel;

    void Awake() {
        buyButton.onClick.AddListener(() => {
            uianim.PopupClose(popup.transform);
            onBuy?.Invoke();
            ClearAction();
        });

        watchAdButton.onClick.AddListener(() => {
            uianim.PopupClose(popup.transform);
            onWatchAd?.Invoke();
            ClearAction();
        });

        cancelButton.onClick.AddListener(() => {
            uianim.PopupClose(popup.transform);
            onCancel?.Invoke();
            ClearAction();
        });

    }

    private void ConfirmPopup(int price, Action onBuy, Action onWatchAd, Action onCancel = null) {
        this.onBuy = onBuy;
        this.onWatchAd = onWatchAd;
        this.onCancel = onCancel;

        buyButton.transform.GetChild(0).GetComponent<TMP_Text>().text = $"<sprite=0>{price}";
        buyButton.interactable = SaveManager.Instance.coins > price;
        uianim.PopupConfirmation(popup.transform);
    }


    private bool WatchAd() { //This can be expanded for whether an ad has played and the user has finished watching it.
        Debug.Log("Watching advert");

        return true;
    }
    
    private void ClearAction() {
        onBuy = null;
        onWatchAd = null;
        onCancel = null;
    }

    public void AddBottle() {
        if (bottleGen.genCount == bottleGen.maxBottleCount) {
            uianim.WarningMessage("Max bottles reached!");
            return;
        }

        if (AnyBottleBusy())
            return;

        if (SaveManager.Instance.addBottle > 0) {
            SaveManager.Instance.addBottle--;
            AddBottlePipeline();
            return;
        }

        ConfirmPopup(
            price.bottlePrice,
            onBuy: () => {
                if (SaveManager.Instance.coins < price.bottlePrice) return;
                SaveManager.Instance.coins -= price.bottlePrice;
                AddBottlePipeline();
            },
            onWatchAd: () => {
                if (!WatchAd()) return;
                AddBottlePipeline();
            }
        );
    }

    private bool AnyBottleBusy() {
        foreach (Bottle bottle in bottleGen.DictionaryToSingularBottleConverter()) {
            if (bottle != null && bottle.anim.IsBusy)
                return true;
        }

        return false;
    }

    public void Shuffle() {
        if (SaveManager.Instance.shuffle > 0) ShufflePipeline();

        if (SaveManager.Instance.shuffle == 0)
            ConfirmPopup(price.shufflePrice,
            onBuy: () => {
                if (SaveManager.Instance.coins < price.shufflePrice) return;
                ShufflePipeline();
            },
            onWatchAd: () => {
                if (!WatchAd()) return;
                SaveManager.Instance.shuffle++;
                ShufflePipeline();
            });
    }

    public void Undo() {
        bool turn;
        foreach (Bottle bottle in bottleGen.DictionaryToSingularBottleConverter()) {
            if (bottle.anim.IsBusy) return;
        }

        if (SaveManager.Instance.undo > 0) {
            turn = gameManager.Undo();
            if (turn) SaveManager.Instance.undo--;
            UndoPipeline(turn);
        }

        if (SaveManager.Instance.undo == 0) {
            ConfirmPopup(price.undoPrice,
                onBuy: () => {
                    if (SaveManager.Instance.coins < price.undoPrice) return;
                    turn = gameManager.Undo();
                    if (turn)
                        SaveManager.Instance.coins -= price.undoPrice;
                    UndoPipeline(turn);
                },
                onWatchAd: () => {
                    if (!WatchAd()) return;
                    turn = gameManager.Undo();
                    if (!turn) SaveManager.Instance.undo++;
                    UndoPipeline(turn);
                }
                );
        }

    }
}