using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] private Button setting;
    [SerializeField] private Button audioButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button replayButton;

    [SerializeField] private UIAnimation anim;
    [SerializeField] private GameManager gameManager;
    private Vector2 centre;

    void Awake() {
        centre = new(Screen.width/2, Screen.height/2);
    }

    private void ButtonUnavailable(Transform button) {

    }

    private void Open() {
        Sequence seq = DOTween.Sequence();

        anim.gamePause.SetActive(true);
        List<Transform> buttons = new() { audioButton.transform, musicButton.transform, replayButton.transform };

        seq.AppendInterval(2.5f)
            .Append(
            anim.gamePause.GetComponent<Image>().DOFade(
                .97f,
                .35f
                ));

        foreach (Transform button in buttons) {
            seq.Append(
                button.DOMove(
                    new Vector2(button.position.x, button.position.y),
                    .4f
                ).From(new Vector2(Screen.width + 100, button.position.y)));

            seq.AppendInterval(.4f);
        }
    }

    private void Close() {
        anim.GameContinue();
    }

    public void Replay() {
        gameManager.OnGameStart(true, false, false);
        Close();
    }



}
