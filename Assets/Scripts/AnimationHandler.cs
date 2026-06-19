using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;

public partial class AnimationHandler : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Transform bottleCap;
    [SerializeField] private Transform cover;
    [SerializeField] private Transform visual;
    [SerializeField] private Transform spill;
    [SerializeField] private Bottle currentBottle;
    [SerializeField] private LiquidColorVisualData colorTranslate;
    [SerializeField] private SpriteRenderer sprite;
    private Material material;

    [Header("Pour Settings")]
    public float pourCornerOffset = 3.1f;
    public float pourHeiOffset = 4f;
    public float pourDuration = 0.35f;
    public float pourAngle = 7.5f;
    public float pourDefaultAngle = 69f;
    public float spillLenOffset = 2f;
    public float spillOffset = 1f;

    private Vector3 originalPos;
    private Quaternion originalRotation;

    private SortingGroup sortingGroup;
    private int originalSortingOrder;


    public bool IsBusy { get; private set; }

    private void Start() {
        material = new Material(sprite.sharedMaterial);
        sprite.material = material;

        originalPos = visual.position;
        originalRotation = visual.rotation;

        sortingGroup = GetComponent<SortingGroup>();

        if (sortingGroup != null)
            originalSortingOrder = sortingGroup.sortingOrder;

    }

    private void Update() {
        if (material == null || visual == null) return;

        float angle = visual.eulerAngles.z;

        if (angle > 180f)
            angle -= 360f;

        float tilt = Mathf.Clamp01(Mathf.Abs(angle) / 90f);

        material.SetFloat("_TiltAmount", tilt);
        UpdateLiquidSurface();
    }

    public void SetPourLiquid(Color[] colors, float fillAmount, int liquidCount) {
        if (material == null) return;

        material.SetColor("_Color0", colors.Length > 0 ? colors[0] : Color.clear);
        material.SetColor("_Color1", colors.Length > 1 ? colors[1] : Color.clear);
        material.SetColor("_Color2", colors.Length > 2 ? colors[2] : Color.clear);
        material.SetColor("_Color3", colors.Length > 3 ? colors[3] : Color.clear);

        material.SetFloat("_FillAmount", Mathf.Clamp01(fillAmount));
        material.SetFloat("_TiltAmount", 0f);

        bool hasLiquid = liquidCount > 0;
        Color topColor = hasLiquid ? colors[liquidCount - 1] : Color.clear;

        SetLiquidSurface(topColor, fillAmount, hasLiquid);
    }

    public void SetPourLiquidColors(Color[] colors, int liquidCount) {
        if (material == null) return;

        material.SetColor("_Color0", colors.Length > 0 ? colors[0] : Color.clear);
        material.SetColor("_Color1", colors.Length > 1 ? colors[1] : Color.clear);
        material.SetColor("_Color2", colors.Length > 2 ? colors[2] : Color.clear);
        material.SetColor("_Color3", colors.Length > 3 ? colors[3] : Color.clear);

        bool hasLiquid = liquidCount > 0;
        Color topColor = hasLiquid ? colors[liquidCount - 1] : Color.clear;

        SetLiquidSurface(topColor, material.GetFloat("_FillAmount"), hasLiquid);
    }

    private void SetDirection(bool dir) {
        if (material == null) return;

        // true = left -> right, false = right -> left
        material.SetFloat("_Direction", dir ? 1f : -1f);
    }

    public void SelectedHover(bool hover) {
        if (visual == null) return;

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
            0.8f,
            new Vector3(0f, 0f, 5f),
            80,
            90
        )
        .SetLink(gameObject)
        .OnComplete(() => {
            visual.localRotation = Quaternion.identity;
        });
    }

    private void Spill(Bottle targetBottle, float angle) {
        if (targetBottle == null || targetBottle.anim == null) return;
        if (targetBottle.anim.spill == null) return;

        Transform spillObj = targetBottle.anim.spill;
        Transform spillParent = spillObj.parent;

        if (spillParent == null) return;

        SpriteRenderer spillRenderer = spillObj.GetComponent<SpriteRenderer>();

        if (spillRenderer != null &&
            colorTranslate != null &&
            targetBottle.GetTopLiquid() != null) {
            spillRenderer.color =
                colorTranslate.GetColor(targetBottle.GetTopLiquid().colorId);
        }

        Vector3 originalSpillParentPos = spillParent.position;
        float targetY = spillOffset + 3 * spillLenOffset;

        spillParent.gameObject.SetActive(true);
        spillParent.localScale = new Vector3(0.2f, 0f, 1f);

        if (angle < 0) {
            spillObj.localPosition = new Vector3(0.5f, -0.5f, 0f);
            spillParent.DOMove(originalSpillParentPos + Vector3.left * 0.2f, 0f);
        } else {
            spillObj.localPosition = new Vector3(-0.5f, -0.5f, 0f);
            spillParent.DOMove(originalSpillParentPos + Vector3.right * 0.2f, 0f);
        }

        Sequence seq = DOTween.Sequence();

        seq.Append(
            spillParent.DOScaleY(
                targetY,
                currentBottle.changes * pourDuration * 0.25f
            )
        );

        seq.AppendInterval(currentBottle.changes * pourDuration * 0.5f);

        seq.Append(
            spillParent.DOScaleX(
                0f,
                currentBottle.changes * pourDuration * 0.25f
            )
        );

        seq.SetLink(gameObject);

        seq.OnComplete(() => {
            spillParent.localScale = new Vector3(0.2f, 0f, 1f);

            spillObj.localPosition = angle < 0
                ? new Vector3(0.5f, -0.5f, 0f)
                : new Vector3(-0.5f, -0.5f, 0f);

            spillParent.position = originalSpillParentPos;
            spillParent.gameObject.SetActive(false);
        });
    }

    private void PlayPour(Bottle nextBottle) {
        if (nextBottle == null || visual == null) return;

        IsBusy = true;
        BringToFront();
        visual.DOKill();

        int movedAmount = Mathf.Abs(currentBottle.changes);
        float fillDuration = pourDuration * movedAmount;

        BottleView fromView = currentBottle.GetComponent<BottleView>();
        BottleView toView = nextBottle.GetComponent<BottleView>();

        float fromEndFill = fromView.GetVisualFillAmount(currentBottle.liquidUnits.Count);

        int toStartCount = nextBottle.liquidUnits.Count - movedAmount;
        int toEndCount = nextBottle.liquidUnits.Count;

        float toStartFill = toStartCount <= 0
            ? toView.GetPourInStartFill(toStartCount)
            : toView.GetVisualFillAmount(toStartCount);

        float toEndFill = toView.GetVisualFillAmount(toEndCount);

        Vector3 targetPos = nextBottle.transform.position;
        targetPos.y += pourHeiOffset + currentBottle.changes * 0.1f;

        float angle = currentBottle.changes * pourAngle + pourDefaultAngle;

        if (originalPos.x > nextBottle.transform.position.x) {
            targetPos.x += pourCornerOffset;
            SetDirection(true);
        } else if (originalPos.x < nextBottle.transform.position.x) {
            targetPos.x -= pourCornerOffset;
            SetDirection(false);
            angle = -angle;
        } else {
            if (originalPos.x >= 0) {
                targetPos.x -= pourCornerOffset;
                SetDirection(false);
                angle = -angle;
            } else {
                targetPos.x += pourCornerOffset;
                SetDirection(true);
            }
        }

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            visual.DOMove(targetPos, pourDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.Join(
            visual.DORotate(new Vector3(0, 0, angle), pourDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.AppendCallback(() =>
        {
            toView.RefreshColorsOnly(nextBottle.liquidUnits);

            Spill(nextBottle, angle);

            currentBottle.anim.TweenFillAmount(
                fromEndFill,
                fillDuration
            );

            nextBottle.anim.TweenFillAmount(
                toStartFill,
                toEndFill,
                fillDuration
            );
        });

        sequence.AppendInterval(fillDuration);

        sequence.AppendCallback(() =>
        {
            currentBottle.RefreshView();
            nextBottle.RefreshView();

            currentBottle.BottleSatisfy(nextBottle);
        });

        sequence.AppendInterval(0.2f);

        sequence.Append(
            visual.DOMove(originalPos, pourDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.Join(
            visual.DORotateQuaternion(originalRotation, pourDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.SetLink(gameObject);

        sequence.OnComplete(() =>
        {
            RestoreSorting();
            IsBusy = false;
        });
    }

    private void PlayCap(Vector3 finalPos) {
        if (bottleCap == null) return;

        bottleCap.DOKill();

        SpriteRenderer capRenderer = bottleCap.GetComponent<SpriteRenderer>();
        if (capRenderer == null) return;

        Vector3 startPos = finalPos + Vector3.up * 1.5f;

        bottleCap.position = startPos;
        bottleCap.gameObject.SetActive(true);

        Color color = capRenderer.color;
        color.a = 0f;
        capRenderer.color = color;

        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(pourDuration * -currentBottle.changes + 0.75f);

        seq.Append(capRenderer.DOFade(1f, 0.1f));

        seq.Join(
            bottleCap.DOMove(finalPos, 0.35f)
                .SetEase(Ease.InQuad)
        );

        seq.SetLink(gameObject);
    }

    public void PlayUnCap() {
        if (bottleCap == null) return;

        SpriteRenderer capRenderer = bottleCap.GetComponent<SpriteRenderer>();
        if (capRenderer == null) return;

        Vector2 capCurrent = bottleCap.position;

        Sequence seq = DOTween.Sequence();

        seq.Join(
            bottleCap.DOMove(capCurrent + Vector2.up * 5f, 0.35f)
                .SetEase(Ease.OutQuad)
        );

        seq.Join(capRenderer.DOFade(0f, 0.1f));

        seq.SetLink(gameObject);

        seq.OnComplete(() => {
            bottleCap.gameObject.SetActive(false);
        });
    }

    public void AddCoverQ(Color color) {
        if (cover == null) return;

        SpriteRenderer cloth = cover.GetComponent<SpriteRenderer>();
        SpriteRenderer indicator = cover.GetChild(0).GetComponent<SpriteRenderer>();

        cover.gameObject.SetActive(true);

        if (cloth != null)
            cloth.DOFade(1f, 0f).SetLink(gameObject);

        if (indicator != null) {
            indicator.color = color;
            indicator.DOFade(1f, 0f).SetLink(gameObject);
        }
    }

    public void AddCoverS(Color color) {
        if (cover == null || visual == null) return;

        SpriteRenderer cloth = cover.GetComponent<SpriteRenderer>();
        SpriteRenderer indicator = cover.GetChild(0).GetComponent<SpriteRenderer>();

        cover.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();

        Vector2 startPt = visual.position + Vector3.up * 1.5f;

        seq.Append(
            cover.DOMove(visual.position, 0.45f)
                .SetEase(Ease.OutSine)
                .From(startPt)
        );

        if (cloth != null)
            seq.Join(cloth.DOFade(1f, 0.45f));

        if (indicator != null) {
            seq.Join(
                indicator.DOFade(1f, 0.45f)
                    .OnStart(() => {
                        indicator.color = color;
                    })
            );
        }

        seq.SetLink(gameObject);
    }

    private void RemoveCover() {
        if (cover == null || visual == null) return;

        SpriteRenderer cloth = cover.GetComponent<SpriteRenderer>();
        SpriteRenderer indicator = cover.GetChild(0).GetComponent<SpriteRenderer>();

        Vector3 endPos = visual.position + Vector3.up * 1.5f;

        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(2f);

        seq.Append(
            cover.DOMove(endPos, 0.45f)
                .SetEase(Ease.OutQuad)
        );

        if (cloth != null)
            seq.Join(cloth.DOFade(0f, 0.45f));

        if (indicator != null)
            seq.Join(indicator.DOFade(0f, 0.45f));

        seq.SetLink(gameObject);

        seq.OnComplete(() => {
            cover.gameObject.SetActive(false);
        });
    }

    private void MoveBottleRoot(Vector3 newPos) {
        transform.DOKill();

        transform.DOMove(newPos, 0.35f)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject)
            .OnComplete(() => {
                originalPos = visual.position;
            });
    }

    private void BringToFront() {
        if (sortingGroup != null)
            sortingGroup.sortingOrder = 1000;
    }

    private void RestoreSorting() {
        if (sortingGroup != null)
            sortingGroup.sortingOrder = originalSortingOrder;
    }

    public float CurrentFillAmount {
        get {
            if (material == null) return 0f;
            return material.GetFloat("_FillAmount");
        }
    }

    public Tween TweenFillAmount(float targetFill, float duration) {
        float startFill =
            material.GetFloat("_FillAmount");

        return DOTween.To(
            () => startFill,
            x => {
                startFill = x;

                material.SetFloat(
                    "_FillAmount",
                    x
                );

                UpdateLiquidSurface();
            },
            targetFill,
            duration
        )
        .SetEase(Ease.Linear)
        .SetLink(gameObject);
    }

    public Tween TweenFillAmount(float startFill, float targetFill, float duration) {
        material.SetFloat(
            "_FillAmount",
            startFill
        );

        return DOTween.To(
            () => startFill,
            x => {
                startFill = x;

                material.SetFloat(
                    "_FillAmount",
                    x
                );

                UpdateLiquidSurface();
            },
            targetFill,
            duration
        )
        .SetEase(Ease.Linear)
        .SetLink(gameObject);
    }

    public void Play(int action, Bottle nextBottle = null, Vector3 newPos = default) {
        if (action == 1) {
            PlayShake();
            return;
        }

        if (IsBusy) return;

        switch (action) {
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

    private void OnDestroy() {
        if (material != null)
            Destroy(material);
    }
}