using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor.Experimental.GraphView;
using System;
using Unity.VisualScripting;
using Sequence = DG.Tweening.Sequence;


public class UIAnimation : MonoBehaviour
{

    //Common Variables
    public GameObject gamePause;
    private Vector2 centre = new(Screen.width / 2, Screen.height / 2);
    private Func<Transform> actions;

    //GameOver Variables
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] public Transform options;
    [SerializeField] private Transform gameOverText;
    [SerializeField] private InputHandler input;

    public float goTextEndPoint = 1.9f;
    public float optionEndPoint = .4f;

    //GameWin Variables
    [SerializeField] private GameObject gameWinPanel;
    [SerializeField] private TMP_Text levelIndex;
    [SerializeField] private Button continueButton;

    [SerializeField] private TMP_Text coinWinsText;

    [SerializeField] private TMP_Text[] shuffleReTexts = new TMP_Text[2];
    [SerializeField] private GameObject shuffleSelf;

    [SerializeField] private TMP_Text[] undoReTexts = new TMP_Text[2];
    [SerializeField] private GameObject undoSelf;

    [SerializeField] private TMP_Text[] addReTexts = new TMP_Text[2];
    [SerializeField] private GameObject addSelf;

    //Warning Message
    [SerializeField] private Transform warningSelf;
    [SerializeField] private TMP_Text[] warnings = new TMP_Text[2];
    private Sequence warn;

    void Start() {
        actions += ShuffleReward;
        actions += UndoReward;
        actions += AddBottleReward;
    }

    public Sequence OpenGamePauseBG(float wait) {
        Image img = gamePause.GetComponent<Image>();

        img.DOKill();
        gamePause.SetActive(true);

        return DOTween.Sequence()
            .AppendInterval(wait).OnStart(() => { input.GamePause(); })
            .Append(
                img.DOFade(.97f, .35f)
            );
    }

    public Sequence GameContinue(float wait) {
        Image img = gamePause.GetComponent<Image>();

        img.DOKill();

        return DOTween.Sequence()
            .AppendInterval(wait)
            .Append(
                img.DOFade(0f, .5f)
            )
            .OnComplete(() => {
                gameOverPanel.SetActive(false);
                gamePause.SetActive(false);
                input.UndoModes();
            });
    }

    public void GameEnd(int level, int amount = 0) {

        OpenGamePauseBG(2.5f).OnComplete(() => {
            if (amount != 0) GameWin(level, amount); else GameOver();
        });
    }

    private void GameOver() {
        
        gameOverPanel.SetActive(true);

        gameOverText.DOMove(
            new(centre.x, centre.y * goTextEndPoint),
            .6f
        ).From(new Vector2(centre.x, Screen.height + 100)).SetEase(Ease.OutBack, 1.5f);

        gameOverText.GetComponent<CanvasGroup>().DOFade(1f, .8f);

        if (SaveManager.Instance.level < 5) {
            options.transform.GetChild(0).gameObject.SetActive(false);
        } else {
            options.transform.GetChild(0).gameObject.SetActive(true);
        }

        options.DOMove(
            new(centre.x, centre.y * optionEndPoint),
            .8f
        ).From(new Vector2(Screen.width * 2, centre.y * optionEndPoint)).SetEase(Ease.OutSine);

        options.GetComponent<CanvasGroup>().DOFade(1, .8f);
    }

    private void GameWin(int level, int amount) {
        continueButton.interactable = false;
        gameWinPanel.SetActive(true);
        string levelt = $"Level {level}";

        levelIndex.text = levelt;
        levelIndex.transform.GetChild(0).GetComponent<TMP_Text>().text = levelt;
        levelIndex.transform.GetChild(1).GetComponent<TMP_Text>().text = levelt;

        coinWinsText.text = "+" + amount.ToString();
        coinWinsText.transform.GetChild(0).GetComponent<TMP_Text>().text = "+" + amount.ToString();


        gameWinPanel.transform.DOMove(
            centre,
            .3f
            ).From(new Vector2(Screen.width * 2, centre.y)).SetEase(Ease.OutSine)
            .OnComplete(() => {
                UpdateRewards();
            });
    }

    public void Revived() {

        gameOverText.DOMove(
            new(centre.x, Screen.height + 100),
            .3f
        );
        gameOverText.GetComponent<CanvasGroup>().DOFade(0, .8f);

        options.DOMove(
            new(Screen.width * 2, centre.y * optionEndPoint),
            .4f
        ).SetEase(Ease.InSine);

        options.GetComponent<CanvasGroup>().DOFade(0, .8f);

        GameContinue(.5f);
    }

    public void NextLevel() {
        gameWinPanel.transform.DOMove(
            new(centre.x * 5, centre.y),
            .6f
            ).OnComplete(() => {
                gameWinPanel.SetActive(false);
            });

        
        GameContinue(.5f);
    }


    public void DisplayCost(GameObject cost, GameObject notif, bool undo) { 
        if (undo) {
            (notif, cost) = (cost, notif);
        }

        DOTween.Sequence()
            .Append(
                cost.transform.GetComponent<Image>().DOFade(0f, .5f)
                    .OnComplete(
                    () => {
                        cost.SetActive(false);
                    })
            )
            .AppendInterval(.4f).OnComplete(() => {
                notif.SetActive(true);
                notif.transform.GetComponent<Image>().DOFade(1, .5f);
            });
    }

    private void UpdateRewards() {
        Sequence seq = DOTween.Sequence();
        foreach (Delegate d in actions.GetInvocationList()) {
            Func<Transform> action = (Func<Transform>)d;
            if (action() != null) {
                seq.Append(
                    action().DOMove(
                        action().position,
                        .4f
                        ).From(new Vector2(action().position.x, action().position.y * .8f))
                        .OnStart(() => {
                            action().gameObject.SetActive(true);
                        })
                        .SetEase(Ease.OutBack, 1.5f)
                    );

                seq.Join(
                        action().GetComponent<CanvasGroup>().DOFade(1f, .4f).From(0)
                    );

            }
            seq.OnComplete(() => {
                continueButton.interactable = true;
            });
        }
    }

    private Transform ShuffleReward() {
        if (SaveManager.Instance.shufflesReward > 0) {
            foreach (var reward in shuffleReTexts) {
                reward.text = "+" + SaveManager.Instance.shufflesReward.ToString();
            }
            return shuffleSelf.transform;
        } else {
            shuffleSelf.SetActive(false);
            return null;
        }
    }

    private Transform UndoReward() {
        if (SaveManager.Instance.undosReward > 0) {
            foreach (var reward in undoReTexts) {
                reward.text = "+" + SaveManager.Instance.undosReward.ToString();
            }
            return undoSelf.transform;
        } else {
            undoSelf.SetActive(false);
            return null;
        }
    }

    private Transform AddBottleReward() {
        if (SaveManager.Instance.addBottlesReward > 0) {
            foreach (var reward in addReTexts) {
                reward.text = "+" + SaveManager.Instance.addBottlesReward.ToString();
            }
            return addSelf.transform;
        } else {
            addSelf.SetActive(false);
            return null;
        }
    }

    

    public void PopupConfirmation(Transform popup) {
        Vector2 popupPos = new(centre.x, centre.y * .3f);
        popup.DOMove(
            popupPos,
            .35f
            ).From(
            new Vector2(centre.x, centre.y * -.3f)
            )
            .SetEase(Ease.OutBack, 1.5f)
            .OnStart(() => {
                gamePause.SetActive(true);
                popup.gameObject.SetActive(true);
            });

        gamePause.GetComponent<Image>().DOFade(
                .97f,
                .35f
                );
    }

    public void PopupClose(Transform popup) {
        Vector2 popupPos = new(centre.x, centre.y * -.3f);
        gamePause.GetComponent<Image>().DOFade(
                0f,
                .35f
                );

        popup.DOMove(
            popupPos,
            .35f
            ).SetEase(Ease.OutQuad, 2f)
            .OnComplete(() => {
                popup.gameObject.SetActive(false);
                gamePause.SetActive(false);
            });
    }

    public void WarningMessage(string message) {
        warn?.Kill();

        foreach (TMP_Text warning in warnings) {
            warning.text = message;
        }

        warn = DOTween.Sequence();

        warn.Append(
            warningSelf.DOMove(
                new(centre.x, centre.y * goTextEndPoint),
                .3f
                ).From(new Vector2(centre.x, Screen.height + 100))
                .SetEase(Ease.OutBack, 1.5f)
                .OnStart(() => {
                    warningSelf.gameObject.SetActive(true);
                }));

        warn.Join(
                warningSelf.GetComponent<CanvasGroup>().DOFade(1f, .5f).From(0));

        warn.AppendInterval(2f);

        warn.Append(
            warningSelf.DOMove(
                new(centre.x, Screen.height + 100),
                .3f
                ).SetEase(Ease.InBack, 2.5f)
                );

        warn.Join(
            warningSelf.GetComponent<CanvasGroup>().DOFade(0f, .5f).From(1).OnComplete(() => {
                warningSelf.gameObject.SetActive(false);
                warn = null;
            })
            );
    }
}
