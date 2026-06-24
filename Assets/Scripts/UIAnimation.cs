using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;

public class UIAnimation : MonoBehaviour {
    [Header("Common")]
    public GameObject gamePause;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private InputHandler input;

    private Vector2 centre;
    private Sequence bgSeq;

    private bool settingsOpen;
    private bool deadOpen;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] public Transform options;
    [SerializeField] private Transform gameOverText;

    public float goTextEndPoint = 1.9f;
    public float optionEndPoint = .4f;

    [Header("Game Win")]
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

    private Func<Transform> rewardActions;

    [Header("Warning")]
    [SerializeField] private Transform warningSelf;
    [SerializeField] private TMP_Text[] warnings = new TMP_Text[2];
    private Sequence warn;

    private void Awake() {
        centre = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    private void Start() {
        rewardActions += ShuffleReward;
        rewardActions += UndoReward;
        rewardActions += AddBottleReward;
    }

    public Sequence RequestBackground(bool fromSettings, float wait = 0f) {
        if (fromSettings)
            settingsOpen = true;
        else
            deadOpen = true;

        Image img = gamePause.GetComponent<Image>();

        bgSeq?.Kill(false);
        img.DOKill();

        gamePause.SetActive(true);

        return bgSeq = DOTween.Sequence()
            .AppendInterval(wait)
            .AppendCallback(() => {
                input.GamePause();
            })
            .Append(
                img.DOFade(.97f, .35f)
            );
    }

    public Sequence ReleaseBackground(bool fromSettings, float wait = 0f) {
        if (fromSettings)
            settingsOpen = false;
        else
            deadOpen = false;

        Image img = gamePause.GetComponent<Image>();

        bgSeq?.Kill(false);
        img.DOKill();

        Sequence seq = DOTween.Sequence()
            .AppendInterval(wait);

        if (!settingsOpen && !deadOpen) {
            seq.Append(
                img.DOFade(0f, .35f)
            );

            seq.OnComplete(() => {
                gamePause.SetActive(false);
                input.UndoModes();
            });
        }

        return bgSeq = seq;
    }

    public void GameEnd(int level, int amount = 0) {
        RequestBackground(false, 2.5f)
            .OnComplete(() => {
                if (amount != 0)
                    GameWin(level, amount);
                else
                    GameOver();
            });
    }

    private void GameOver() {
        gameOverPanel.SetActive(true);

        gameOverText.DOMove(
            new Vector2(centre.x, centre.y * goTextEndPoint),
            .6f
        )
        .From(new Vector2(centre.x, Screen.height + 100))
        .SetEase(Ease.OutBack, 1.5f);

        gameOverText.GetComponent<CanvasGroup>()
            .DOFade(1f, .8f);

        if (SaveManager.Instance.level < 5)
            options.transform.GetChild(0).gameObject.SetActive(false);
        else
            options.transform.GetChild(0).gameObject.SetActive(true);

        options.DOMove(
            new Vector2(centre.x, centre.y * optionEndPoint),
            .8f
        )
        .From(new Vector2(Screen.width * 2, centre.y * optionEndPoint))
        .SetEase(Ease.OutSine);

        options.GetComponent<CanvasGroup>()
            .DOFade(1f, .8f);
    }

    private void GameWin(int level, int amount) {
        continueButton.interactable = false;
        gameWinPanel.SetActive(true);

        string levelText = $"Level {level}";

        levelIndex.text = levelText;
        levelIndex.transform.GetChild(0).GetComponent<TMP_Text>().text = levelText;
        levelIndex.transform.GetChild(1).GetComponent<TMP_Text>().text = levelText;

        string coinText = "+" + amount;
        coinWinsText.text = coinText;
        coinWinsText.transform.GetChild(0).GetComponent<TMP_Text>().text = coinText;

        gameWinPanel.transform.DOMove(
            centre,
            .3f
        )
        .From(new Vector2(Screen.width * 2, centre.y))
        .SetEase(Ease.OutSine)
        .OnComplete(UpdateRewards);
    }

    public void Revived() {
        gameOverText.DOMove(
            new Vector2(centre.x, Screen.height + 100),
            .3f
        );

        gameOverText.GetComponent<CanvasGroup>()
            .DOFade(0f, .8f);

        options.DOMove(
            new Vector2(Screen.width * 2, centre.y * optionEndPoint),
            .4f
        )
        .SetEase(Ease.InSine);

        options.GetComponent<CanvasGroup>()
            .DOFade(0f, .8f);

        gameOverPanel.SetActive(false);

        ReleaseBackground(false, .5f);
    }

    public void NextLevel() {
        continueButton.interactable = false;

        gameWinPanel.transform.DOMove(
            new Vector2(centre.x * 5f, centre.y),
            .6f
        )
        .OnComplete(() => {
            gameWinPanel.SetActive(false);
            ReleaseBackground(false, 0f);
        });
    }

    private void UpdateRewards() {
        Sequence seq = DOTween.Sequence();

        foreach (Delegate d in rewardActions.GetInvocationList()) {
            Func<Transform> action = (Func<Transform>)d;
            Transform reward = action();

            if (reward == null)
                continue;

            seq.Append(
                reward.DOMove(
                    reward.position,
                    .4f
                )
                .From(new Vector2(reward.position.x, reward.position.y * .8f))
                .OnStart(() => {
                    reward.gameObject.SetActive(true);
                })
                .SetEase(Ease.OutBack, 1.5f)
            );

            seq.Join(
                reward.GetComponent<CanvasGroup>()
                    .DOFade(1f, .4f)
                    .From(0f)
            );
        }

        seq.OnComplete(() => {
            continueButton.interactable = true;
        });
    }

    private Transform ShuffleReward() {
        if (SaveManager.Instance.shufflesReward <= 0) {
            shuffleSelf.SetActive(false);
            return null;
        }

        foreach (TMP_Text reward in shuffleReTexts)
            reward.text = "+" + SaveManager.Instance.shufflesReward;

        return shuffleSelf.transform;
    }

    private Transform UndoReward() {
        if (SaveManager.Instance.undosReward <= 0) {
            undoSelf.SetActive(false);
            return null;
        }

        foreach (TMP_Text reward in undoReTexts)
            reward.text = "+" + SaveManager.Instance.undosReward;

        return undoSelf.transform;
    }

    private Transform AddBottleReward() {
        if (SaveManager.Instance.addBottlesReward <= 0) {
            addSelf.SetActive(false);
            return null;
        }

        foreach (TMP_Text reward in addReTexts)
            reward.text = "+" + SaveManager.Instance.addBottlesReward;

        return addSelf.transform;
    }

    public void PopupConfirmation(Transform popup) {
        RequestBackground(true, 0f);

        Vector2 popupPos = new Vector2(centre.x, centre.y * .3f);

        popup.DOMove(
            popupPos,
            .35f
        )
        .From(new Vector2(centre.x, centre.y * -.3f))
        .SetEase(Ease.OutBack, 1.5f)
        .OnStart(() => {
            popup.gameObject.SetActive(true);
        });
    }

    public void PopupClose(Transform popup) {
        Vector2 popupPos = new Vector2(centre.x, centre.y * -.3f);

        popup.DOMove(
            popupPos,
            .35f
        )
        .SetEase(Ease.OutQuad)
        .OnComplete(() => {
            popup.gameObject.SetActive(false);
            ReleaseBackground(true, 0f);
        });
    }

    public void WarningMessage(string message) {
        warn?.Kill(false);

        foreach (TMP_Text warning in warnings)
            warning.text = message;

        CanvasGroup group = warningSelf.GetComponent<CanvasGroup>();

        warn = DOTween.Sequence();

        warn.Append(
            warningSelf.DOMove(
                new Vector2(centre.x, centre.y * goTextEndPoint),
                .3f
            )
            .From(new Vector2(centre.x, Screen.height + 100))
            .SetEase(Ease.OutBack, 1.5f)
            .OnStart(() => {
                warningSelf.gameObject.SetActive(true);
            })
        );

        warn.Join(
            group.DOFade(1f, .5f)
                .From(0f)
        );

        warn.AppendInterval(2f);

        warn.Append(
            warningSelf.DOMove(
                new Vector2(centre.x, Screen.height + 100),
                .3f
            )
            .SetEase(Ease.InBack, 2.5f)
        );

        warn.Join(
            group.DOFade(0f, .5f)
        );

        warn.OnComplete(() => {
            warningSelf.gameObject.SetActive(false);
            warn = null;
        });
    }

    public void DisplayCost(GameObject cost, GameObject notif, bool undo) {
        if (undo)
            (notif, cost) = (cost, notif);

        DOTween.Sequence()
            .Append(
                cost.transform.GetComponent<Image>()
                    .DOFade(0f, .1f)
            )
            .AppendCallback(() => {
                cost.SetActive(false);
                notif.SetActive(true);
            })
            .Append(
                notif.transform.GetComponent<Image>()
                    .DOFade(1f, .1f)
            );
    }
}