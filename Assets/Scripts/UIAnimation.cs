using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor.Experimental.GraphView;
using System;


public class UIAnimation : MonoBehaviour
{

    //Common Variables
    [SerializeField] private GameObject gameEndPanel;
    private Vector2 centre = new(Screen.width / 2, Screen.height / 2);

    //GameOver Variables
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] public Transform options;
    [SerializeField] private Transform gameOverText;

    public float goTextEndPoint = 1.9f;
    public float optionEndPoint = .4f;

    //GameWin Variables
    [SerializeField] private GameObject gameWinPanel;
    [SerializeField] private TMP_Text levelIndex;
    [SerializeField] private TMP_Text coinWinsText;




    public void GameEnd(int level, int amount = 0) {
        gameEndPanel.SetActive(true);

        DOTween.Sequence()
            .AppendInterval(2.5f)
            .Append(
            gameEndPanel.GetComponent<Image>().DOFade(
                .97f, 
                .5f
                ))
            .OnComplete(() => {
                if (amount != 0) GameWin(level, amount); else GameOver();
            });
    }

    private void GameOver() {
        gameOverPanel.SetActive(true);

        gameOverText.DOMove(
            new(centre.x, centre.y * goTextEndPoint),
            .6f
        ).From(new Vector2(centre.x, Screen.height + 100));

        gameOverText.GetComponent<CanvasGroup>().DOFade(1f, .8f);

        options.DOMove(
            new(centre.x, centre.y * optionEndPoint),
            .8f
        ).From(new Vector2(Screen.width + 100, centre.y * optionEndPoint));

        options.GetComponent<CanvasGroup>().DOFade(1, .8f);
    }

    private void GameWin(int level, int amount) {
        gameWinPanel.SetActive(true);
        string levelt = $"Level {level}";

        levelIndex.text = levelt;
        levelIndex.transform.GetChild(0).GetComponent<TMP_Text>().text = levelt;
        levelIndex.transform.GetChild(1).GetComponent<TMP_Text>().text = levelt;

        coinWinsText.text = amount.ToString();
        coinWinsText.transform.GetChild(0).GetComponent<TMP_Text>().text = amount.ToString();

        gameWinPanel.transform.DOMove(
            centre,
            .3f
            ).From(new Vector2(centre.x * 5, centre.y));

    }

    public void Revived() {

        gameOverText.DOMove(
            new(centre.x, Screen.height + 100),
            .3f
        );
        gameOverText.GetComponent<CanvasGroup>().DOFade(0, .8f);

        options.DOMove(
            new(Screen.width + 100, centre.y * optionEndPoint),
            .4f
        );

        options.GetComponent<CanvasGroup>().DOFade(0, .8f);

        GameContinue();
    }

    public void NextLevel() {
        gameWinPanel.transform.DOMove(
            new(centre.x * 5, centre.y),
            .6f
            ).OnComplete(() => {
                gameWinPanel.SetActive(false);
            });

        
        GameContinue();

    }

    private void GameContinue() {
        DOTween.Sequence()
            .AppendInterval(.5f)
            .Append(
                gameEndPanel.GetComponent<Image>().DOFade(0, .5f)
            )
            .OnComplete(() => {
                gameOverPanel.SetActive(false);
                gameEndPanel.SetActive(false);
            });
    }

    public void DisplayCost(GameObject cost, GameObject notif, bool undo) { //notif pops up, cost disappear if false
        if (undo) {
            GameObject temp = cost;
            cost = notif;
            notif = temp;
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
}
