using UnityEngine;
using DG.Tweening;

public class Star : MonoBehaviour {
    public Camera cam;

    [Header("Star Settings")]
    public GameObject starPrefab;
    public int starCount = 30;

    private float width;
    private float height;

    [Header("Movement")]
    public float moveRange = 0.5f;
    public float minMoveTime = 2f;
    public float maxMoveTime = 5f;

    [Header("Twinkle")]
    public float minAlpha = 0.2f;
    public float maxAlpha = 1f;
    public float minTwinkleTime = 1f;
    public float maxTwinkleTime = 3f;

    void Awake() {
        height = cam.orthographicSize * 2f;
        width = height * 2f * cam.aspect;
    }

    private void Start() {
        

        GenerateStars();
    }

    private void GenerateStars() {
        DOTween.SetTweensCapacity(300, 300);
        for (int i = 0; i < starCount; i++) {
            GameObject star = Instantiate(starPrefab, transform);

            Vector3 pos = new Vector3(
                Random.Range(-width / 2f, width / 2f),
                Random.Range(-height / 2f, height / 2f),
                0f
            );

            star.transform.localPosition = pos;

            float size = Random.Range(0.05f, 0.09f);
            star.transform.localScale = Vector3.one * size;

            SpriteRenderer sr = star.GetComponent<SpriteRenderer>();
            sr.color = new Color(20f, 20f, 20f, 1f);
            if (sr != null) {
                Color c = sr.color;
                c.a = Random.Range(minAlpha, maxAlpha);
                sr.color = c;

                sr.DOFade(
                    Random.Range(minAlpha, maxAlpha),
                    Random.Range(minTwinkleTime, maxTwinkleTime)
                )
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
            }

            AnimateMovement(star.transform);
        }
    }

    private void AnimateMovement(Transform star) {
        Vector3 target = star.localPosition + new Vector3(
            Random.Range(-moveRange, moveRange),
            Random.Range(-moveRange, moveRange),
            0f
        );

        star.DOLocalMove(
            target,
            Random.Range(minMoveTime, maxMoveTime)
        )
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy() {
        DOTween.Kill(transform);
    }
}