using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;

public class AnimationHandler : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Transform bottleCap;
    [SerializeField] private Transform cover;
    [SerializeField] private Transform visual;
    [SerializeField] private Bottle currentBottle;

    [Header("Pour Settings")]
    public float pourCornerOffset = 3.1f;
    public float pourHeiOffset = 4f;
    public float pourDuration = 0.35f;
    public float pourAngle = 95f;

    private Vector3 originalPos;
    private Quaternion originalRotation;

    private int originalSortingOrder;
    private SortingGroup sortingGroup;

    public bool IsBusy { get; private set; }

    void Start() {
        originalPos = visual.position;
        originalRotation = visual.rotation;

        sortingGroup = GetComponent<SortingGroup>();

        if (sortingGroup != null)
            originalSortingOrder = sortingGroup.sortingOrder;

    }

    public void SelectedHover(bool hover) {
        visual.DOKill();

        if (hover) {
            BringToFront();

            visual.DOMove(originalPos + Vector3.up * 1.2f, 0.2f)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);
        } else {
            visual.DOMove(originalPos, 0.2f)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject)
                .OnComplete(RestoreSorting);
        }
    }

    private void PlayShake() {
        if (visual == null) return;

        visual.DOKill();

        visual.DOShakeRotation(
            .8f,
            new Vector3(0f, 0f, 5f),
            80,
            90
        )
        .SetLink(gameObject)
        .OnComplete(() => {
            visual.localRotation = Quaternion.identity;
        });
    }

    private void PlayPour(Bottle nextBottle) {
        if (nextBottle == null) return;

        IsBusy = true;
        BringToFront();

        visual.DOKill();

        Vector3 targetPos = nextBottle.transform.position;
        targetPos.y += pourHeiOffset;

        float angle;

        if (originalPos.x > nextBottle.transform.position.x) {
            targetPos.x += pourCornerOffset;
            angle = pourAngle;
        } else if (originalPos.x < nextBottle.transform.position.x) {
            targetPos.x -= pourCornerOffset;
            angle = -pourAngle;
        } else {
            if (originalPos.x >= 0) {
                targetPos.x -= pourCornerOffset;
                angle = -pourAngle;
            } else {
                targetPos.x += pourCornerOffset;
                angle = pourAngle;
            }
        }

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            visual.DOMove(targetPos, pourDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject)
        );

        sequence.Join(
            visual.DORotate(new Vector3(0, 0, angle), pourDuration)
                .SetLink(gameObject)
                .SetEase(Ease.OutQuad)
        );

        sequence.AppendInterval(pourDuration * currentBottle.changes).SetLink(gameObject);

        sequence.AppendCallback(() => {
            currentBottle.RefreshView();
            nextBottle.RefreshView();
        }).SetLink(gameObject);

        sequence.AppendInterval(0.2f).SetLink(gameObject);

        sequence.Append(
            visual.DOMove(originalPos, pourDuration)
                .SetLink(gameObject)
                .SetEase(Ease.OutQuad)
        );

        sequence.Join(
            visual.DORotateQuaternion(originalRotation, pourDuration)
                .SetLink(gameObject)
                .SetEase(Ease.OutQuad)
        );

        sequence.OnComplete(() => {
            RestoreSorting();
            IsBusy = false;
        });
    }

    private void PlayCap(Vector3 finalPos) {
        if (bottleCap == null) return;

        bottleCap.DOKill();

        SpriteRenderer capRenderer = bottleCap.GetComponent<SpriteRenderer>();

        Vector3 startPos = finalPos + Vector3.up * 1.5f;

        bottleCap.position = startPos;
        bottleCap.gameObject.SetActive(true);

        Color color = capRenderer.color;
        color.a = 0f;
        capRenderer.color = color;

        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(pourDuration * -currentBottle.changes + .75f).SetLink(gameObject);

        seq.Append(capRenderer.DOFade(1f, 0.1f).SetLink(gameObject));

        seq.Join(
            bottleCap.DOMove(finalPos, 0.35f)
                .SetEase(Ease.InQuad).SetLink(gameObject)
        );
    }

    private void RemoveCover() {
        if (cover == null) return;

        cover.DOKill();

        SpriteRenderer cloth = cover.GetComponent<SpriteRenderer>();
        SpriteRenderer indicator = cover.GetChild(0).GetComponent<SpriteRenderer>();

        Vector3 endPos = cover.position + Vector3.up * 1.5f;

        Sequence seq = DOTween.Sequence();

        seq.Join(
            cover.DOMove(endPos, 0.45f)
                .SetLink(gameObject)
                .SetEase(Ease.OutQuad)
        );

        seq.Join(cloth.DOFade(0f, 0.45f).SetLink(gameObject));

        if (indicator != null)
            seq.Join(indicator.DOFade(0f, 0.45f).SetLink(gameObject));

        seq.OnComplete(() =>
        {
            cover.gameObject.SetActive(false);
        });
    }

    private void MoveBottleRoot(Vector3 newPos) {
        transform.DOKill();

        transform.DOMove(newPos, 0.35f)
            .SetLink(gameObject)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                originalPos = visual.position;
            });
    }

    private void UpdatePosition(Vector3 newPos) {
        originalPos = newPos;
    }

    private void BringToFront() {
        if (sortingGroup != null)
            sortingGroup.sortingOrder = 1000;
    }

    private void RestoreSorting() {
        if (sortingGroup != null)
            sortingGroup.sortingOrder = originalSortingOrder;
    }

    public void Play(int action, Bottle nextBottle = null, Vector3 newPos = default) {

        if (IsBusy) return;

        switch (action) {
            case 1:
                PlayShake();
                break;
            case 2:
                PlayPour(nextBottle);
                break;
            case 3:
                MoveBottleRoot(newPos);
                break;
            case 4:
                PlayCap(newPos);
                break;
            case 5:
                RemoveCover();
                break;
        }
    }
}