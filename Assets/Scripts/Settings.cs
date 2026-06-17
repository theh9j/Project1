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

    [SerializeField] private UIAnimation anim;
    [SerializeField] private GameManager gameManager;

    private Vector2 centre = new(Screen.width / 2, Screen.height / 2);
    private Vector2[] OGcoords = new Vector2[3];
    private Sequence seq;

    private bool settingActive = false;
    private bool settingBusy = false;

    void Start() {
        OGcoords[0] = audioButton.transform.parent.position;
        OGcoords[1] = musicButton.transform.parent.position;
        OGcoords[2] = replayButton.transform.parent.position;

        setting.onClick.AddListener(() => {
            if (settingBusy) return;
            settingBusy = true;
            if (!settingActive) Open(); else Close();
        });

        replayButton.onClick.AddListener(() => {
            Replay();
        });

        audioButton.onClick.AddListener(() => {
            SFX();
        });

        musicButton.onClick.AddListener(() => {
            Music();
        });
    }

    private void Open() {
        seq = DOTween.Sequence();
        List<Transform> buttons = new() { audioButton.transform, musicButton.transform, replayButton.transform };

        seq.AppendCallback(() => { anim.OpenGamePauseBG(.2f); });

        for (int i = 0; i < buttons.Count; i++) {
            int index = i;

            seq.Append(
                buttons[index].DOMove(
                    OGcoords[index],
                    .15f
                ).From(new Vector2(Screen.width + 150, buttons[index].position.y))
                .OnStart(() => {
                    buttons[index].gameObject.SetActive(true);
                })
                .SetEase(Ease.OutBack, 3f)
                );

        }
        seq.AppendInterval(.5f);
        seq.OnComplete(() => {
            settingBusy = false;
            settingActive = true;
        });
    }

    private void Close() {
        seq = DOTween.Sequence();
        List<Transform> buttons = new() { audioButton.transform, musicButton.transform, replayButton.transform };

        for (int i = buttons.Count-1; i >= 0; i--) {
            int index = i;
            seq.Append(
                buttons[index].DOMove(
                    new Vector2(Screen.width + 150, buttons[index].position.y),
                    .15f
                ).OnComplete(() => {
                    buttons[index].gameObject.SetActive(false);
                })
                .SetEase(Ease.InBack, 3f));
        }
        seq.AppendCallback(() => { anim.GameContinue(.2f); });
        seq.AppendInterval(.5f);
        seq.OnComplete(() => {
            settingBusy = false;
            settingActive = false;
        });

    }

    private void Replay() {
        gameManager.OnGameStart(true, false, false);
        Close();
    }

    private void SFX() {
        bool isMuted = AudioManager.Instance.ToggleMuteSFX();
        ButtonUnavailable(audioButton.transform, isMuted);
    }

    private void Music() {
        bool isMuted = AudioManager.Instance.ToggleMuteBG();
        ButtonUnavailable(musicButton.transform, isMuted);
    }

    private void ButtonUnavailable(Transform button, bool avail) {
        GameObject slash = button.Find("Slash").gameObject;
        if (avail) {
            slash.SetActive(true);
        } else {
            slash.SetActive(false);
        }
    }
}
