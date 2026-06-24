using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;

public class Settings : MonoBehaviour {
    [Header("Buttons")]
    [SerializeField] private Button setting;

    [SerializeField] private Button audioButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button replayButton;

    [Header("References")]
    [SerializeField] private UIAnimation anim;
    [SerializeField] private GameManager gameManager;

    private readonly Vector2[] OGcoords = new Vector2[3];

    private Sequence seq;

    private bool settingActive;
    private bool settingBusy;

    private void Start() {
        OGcoords[0] = audioButton.transform.parent.position;
        OGcoords[1] = musicButton.transform.parent.position;
        OGcoords[2] = replayButton.transform.parent.position;

        ButtonUnavailable(musicButton.transform, !SaveManager.Instance.music);
        ButtonUnavailable(audioButton.transform, !SaveManager.Instance.sfx);

        setting.onClick.RemoveAllListeners();
        setting.onClick.AddListener(ToggleSettings);

        replayButton.onClick.RemoveAllListeners();
        replayButton.onClick.AddListener(Replay);

        audioButton.onClick.RemoveAllListeners();
        audioButton.onClick.AddListener(SFX);

        musicButton.onClick.RemoveAllListeners();
        musicButton.onClick.AddListener(Music);
    }

    private void ToggleSettings() {
        if (settingBusy)
            return;

        settingBusy = true;

        if (!settingActive)
            Open();
        else
            Close();
    }

    private void Open() {
        seq?.Kill(false);
        seq = DOTween.Sequence();

        List<Transform> buttons = GetButtonTransforms();

        seq.Append(
            anim.RequestBackground(true, .2f)
        );

        for (int i = 0; i < buttons.Count; i++) {
            int index = i;

            seq.AppendCallback(() => {
                buttons[index].gameObject.SetActive(true);
            });

            seq.Append(
                buttons[index]
                    .DOMove(
                        OGcoords[index],
                        .15f
                    )
                    .From(
                        new Vector2(
                            Screen.width + 150,
                            buttons[index].position.y
                        )
                    )
                    .SetEase(Ease.OutBack, 3f)
            );
        }

        seq.OnComplete(() => {
            settingActive = true;
            settingBusy = false;
        });
    }

    private void Close() {
        seq?.Kill(false);
        seq = DOTween.Sequence();

        List<Transform> buttons = GetButtonTransforms();

        for (int i = buttons.Count - 1; i >= 0; i--) {
            int index = i;

            seq.Append(
                buttons[index]
                    .DOMove(
                        new Vector2(
                            Screen.width + 150,
                            buttons[index].position.y
                        ),
                        .15f
                    )
                    .SetEase(Ease.InBack, 3f)
            );

            seq.AppendCallback(() => {
                buttons[index].gameObject.SetActive(false);
            });
        }

        seq.Append(
            anim.ReleaseBackground(true, .2f)
        );

        seq.OnComplete(() => {
            settingActive = false;
            settingBusy = false;
        });
    }

    private List<Transform> GetButtonTransforms() {
        return new List<Transform>
        {
            audioButton.transform,
            musicButton.transform,
            replayButton.transform
        };
    }

    private void Replay() {
        if (settingBusy)
            return;

        settingBusy = true;

        seq?.Kill(false);

        gameManager.OnGameStart(true, false);

        Close();
    }

    private void SFX() {
        bool isMuted = AudioManager.Instance.ToggleMuteSFX();

        ButtonUnavailable(
            audioButton.transform,
            isMuted
        );
    }

    private void Music() {
        bool isMuted = AudioManager.Instance.ToggleMuteBG();

        ButtonUnavailable(
            musicButton.transform,
            isMuted
        );
    }

    private void ButtonUnavailable(Transform button, bool unavailable) {
        Transform slash = button.Find("Slash");

        if (slash == null)
            return;

        slash.gameObject.SetActive(unavailable);
    }

    private void OnDisable() {
        seq?.Kill(false);
        seq = null;

        settingBusy = false;
    }
}