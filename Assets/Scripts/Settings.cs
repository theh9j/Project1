using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

public class Settings : MonoBehaviour
{
    [SerializeField] private Button setting;

    [SerializeField] private Button audioButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button replayButton;

    [SerializeField] private Transform slash;

    [SerializeField] private UIAnimation anim;
    [SerializeField] private GameManager gameManager;

    private Vector2 centre = new(Screen.width / 2, Screen.height / 2);
    private Sequence seq;

    private bool settingActive = false;
    private bool settingBusy = false;
    private bool test;

    void Start() {
        setting.onClick.AddListener(() => {
            if (!settingActive) Open(); else Close();
        });

        replayButton.onClick.AddListener(() => {
            Replay();
        });

        audioButton.onClick.AddListener(() => {
            Audio();
        });

        musicButton.onClick.AddListener(() => {
            Music();
        });
    }

    private void ButtonUnavailable(Transform button, bool avail) {
        GameObject slash = button.Find("Slash").gameObject;
        if (avail) {
            slash.SetActive(true);
        } else {
            slash.SetActive(false);
        }
    }

    private void Open() {
        if (settingBusy) return;
        seq = DOTween.Sequence();
        settingBusy = true;
        List<Transform> buttons = new() { audioButton.transform, musicButton.transform, replayButton.transform };
        anim.OpenGamePauseBG(.2f);

        foreach (Transform button in buttons) {
            seq.Append(
                button.DOMove(
                    new Vector2(button.position.x, button.position.y),
                    .15f
                ).From(new Vector2(Screen.width + 150, button.position.y))
                .OnStart(() => {
                    button.gameObject.SetActive(true);
                })
                .SetEase(Ease.OutBack, 3f)
                );

            seq.AppendInterval(.05f);
        }
        settingActive = true;
        seq.OnComplete(() => {
            settingBusy = false;
        });
    }

    private void Close() {
        if (settingBusy) return;
        seq = DOTween.Sequence();
        settingBusy = true;
        List<Transform> buttons = new() { replayButton.transform, musicButton.transform, audioButton.transform };

        foreach (Transform button in buttons) {
            Vector2 originalCoord = button.position;
            seq.Append(
                button.DOMove(
                    new Vector2(Screen.width + 150, button.position.y),
                    .15f
                ).OnComplete(() => {
                    button.gameObject.SetActive(false);
                    button.position = originalCoord;
                })
                .SetEase(Ease.InBack, 3f));

        }
        anim.GameContinue();
        settingActive = false;
        seq.OnComplete(() => {
            settingBusy = false;
        });

    }

    private void Replay() {
        gameManager.OnGameStart(true, false, false);
        Close();
    }

    private void Audio() {
        //Audio Processing
        test = !test;
        ButtonUnavailable(audioButton.transform, test);
    }

    private void Music() {
        //Music Processing
        ButtonUnavailable(musicButton.transform, true);
    }

}
