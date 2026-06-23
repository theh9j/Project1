using DG.Tweening;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        musicSource.mute = !SaveManager.Instance.music;
        sfxSource.mute = !SaveManager.Instance.sfx;
        StartCoroutine(PlayBG(musicSource.clip));
    }

    private IEnumerator PlayBG(AudioClip clip) {
        while (true) {
            musicSource.clip = clip;
            musicSource.volume = .1f;
            musicSource.Play();

            yield return new WaitForSeconds(clip.length - .5f);

            yield return musicSource.DOFade(0f, .5f).WaitForCompletion();

            musicSource.Stop();
            musicSource.time = 0;
            musicSource.volume = .1f;

        }
    }

    public void PlaySFX(AudioClip clip) {
        sfxSource.volume = .3f;
        sfxSource.clip = clip;
        sfxSource.Play();
    }

    public void StopSFX() {
        sfxSource?.DOFade(0f, .2f).OnComplete(() => {
            sfxSource.Stop();
        });
        
    }

    public bool ToggleMuteBG() {
        musicSource.mute = !musicSource.mute;
        SaveManager.Instance.music = musicSource.mute;
        if (musicSource.mute) return true;
        return false;
    }

    public bool ToggleMuteSFX() {
        sfxSource.mute = !sfxSource.mute;
        SaveManager.Instance.sfx = sfxSource.mute;
        if (sfxSource.mute) return true;
        return false;
    }
}
