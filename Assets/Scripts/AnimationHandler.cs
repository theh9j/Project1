using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;

public partial class AnimationHandler : MonoBehaviour {
    [Header("References")]
    public Transform bottleNeck;
    [SerializeField] private Transform bottleCap;
    [SerializeField] private Transform cover;
    [SerializeField] private Transform visual;
    [SerializeField] private Bottle currentBottle;
    [SerializeField] private LiquidColorVisualData colorTranslate;
    [SerializeField] private Transform bottleCapPos;

    [Header("Liquid Shader")]
    [SerializeField] private Transform liquidRoot;
    [SerializeField] private Transform boundingBottle;
    [SerializeField] private SpriteRenderer liquidRenderer;
    [SerializeField] private SpriteRenderer boundingBottleRenderer;

    [Header("Pour Settings")]
    [SerializeField] private float pourStartAngle = 65f;
    [SerializeField] private float pourEndAngle = 95f;
    public float pourCornerOffset = 3.1f;
    public float pourHeiOffset = 4f;
    public float pourDuration = 0.35f;
    public float pourAngle = 7.5f;
    public float pourDefaultAngle = 60f;
    public float spillLenOffset = 1.25f;
    public float spillOffset = 3f;

    [Header("Liquid Width Scaling")]
    [SerializeField] private float liquidWidthPadding = 1f;
    [SerializeField] private float liquidHeightPadding = 1f;
    [SerializeField] private float liquidTopPadding = 0.08f;
    private Vector3 baseLiquidWorldSize;
    private Vector2 liquidOffsetPadding = new Vector2(0f, 0.04f);
    private Vector3 liquidBaseScale;
    private Vector3 liquidBaseOffset;
    private float baseBottleWidth;
    private float baseBottleHeight;
    private Tween shakeTween;

    private Material material;
    private Material spillMat;

    private Vector3 originalPos;
    private Quaternion originalRotation;

    private float baseProjectedWidth;

    private SortingGroup sortingGroup;
    private int originalSortingOrder;


    public bool IsBusy { get; private set; }
    public bool IsUnavailable { get; set; }

    void Awake() {
        if (liquidRenderer != null) {
            material = new Material(liquidRenderer.sharedMaterial);
            baseLiquidWorldSize = liquidRenderer.bounds.size;
            liquidRenderer.material = material;
        }
    }

    private void Start() {
        originalPos = visual.position;
        originalRotation = visual.rotation;

        sortingGroup = GetComponent<SortingGroup>();

        if (sortingGroup != null)
            originalSortingOrder = sortingGroup.sortingOrder;

        if (liquidRoot != null) {
            liquidBaseScale = liquidRoot.localScale;
            liquidBaseOffset = liquidRoot.position - boundingBottle.position;
        }

        if (boundingBottleRenderer != null) {
            Bounds b = boundingBottleRenderer.bounds;

            baseBottleWidth = b.size.x;
            baseBottleHeight = b.size.y;
        }

        dingSFX.LoadAudioData();
        pourSFX.LoadAudioData();
        downSFX.LoadAudioData();
    }

    void Update() {
        UpdateLiquidBounds();
        UpdateLiquidSurface(true);
    }

    private void UpdateLiquidBounds() {
        if (boundingBottleRenderer == null ||
            liquidRoot == null ||
            liquidRenderer == null)
            return;

        Bounds b = boundingBottleRenderer.bounds;

        liquidRoot.position =
            b.center +
            Vector3.down * (liquidTopPadding * 0.5f) +
            (Vector3)liquidOffsetPadding;

        liquidRoot.rotation = Quaternion.identity;

        float targetWidth = b.size.x * liquidWidthPadding;
        float targetHeight =
            (b.size.y - liquidTopPadding) * liquidHeightPadding;

        liquidRoot.localScale = new Vector3(
            liquidBaseScale.x * targetWidth / baseLiquidWorldSize.x,
            liquidBaseScale.y * targetHeight / baseLiquidWorldSize.y,
            liquidBaseScale.z
        );
    }

    public void SetPourLiquid(
    Color[] colors,
    float fillAmount,
    int liquidCount) {
        if (material == null)
            return;

        material.SetColor("_Color0", colors.Length > 0 ? colors[0] : Color.clear);
        material.SetColor("_Color1", colors.Length > 1 ? colors[1] : Color.clear);
        material.SetColor("_Color2", colors.Length > 2 ? colors[2] : Color.clear);
        material.SetColor("_Color3", colors.Length > 3 ? colors[3] : Color.clear);

        material.SetFloat("_FillAmount", Mathf.Clamp01(fillAmount));

        bool hasLiquid = liquidCount > 0;

        Color topColor = hasLiquid
            ? colors[liquidCount - 1]
            : Color.clear;

        Color surfaceColor = Lighten(topColor, 0.3f);

        SetLiquidSurface(surfaceColor, hasLiquid);

        UpdateLiquidSurface(true);
    }

    public void SetPourLiquidColors(Color[] colors, int liquidCount) {
        if (material == null) return;

        material.SetColor("_Color0", colors.Length > 0 ? colors[0] : Color.clear);
        material.SetColor("_Color1", colors.Length > 1 ? colors[1] : Color.clear);
        material.SetColor("_Color2", colors.Length > 2 ? colors[2] : Color.clear);
        material.SetColor("_Color3", colors.Length > 3 ? colors[3] : Color.clear);

        bool hasLiquid = liquidCount > 0;
        Color topColor = hasLiquid
            ? colors[liquidCount - 1]
            : Color.clear;

        Color surfaceColor = Lighten(topColor, 0.3f);

        SetLiquidSurface(surfaceColor, hasLiquid);
    }

    public Tween TweenFillAmount(
    float targetFill,
    float duration,
    float? forceStart = null) {
        if (material == null) return null;

        float currentFill = forceStart ?? material.GetFloat("_FillAmount");

        if (forceStart.HasValue)
            material.SetFloat("_FillAmount", currentFill);

        return DOTween.To(
            () => currentFill,
            x => {
                currentFill = x;

                material.SetFloat("_FillAmount", x);

                UpdateLiquidSurface(true);
            },
            targetFill,
            duration
        )
        .SetEase(Ease.InQuad)
        .SetLink(gameObject);
    }

    public void SelectedHover(bool hover) {
        if (visual == null) return;
        if (IsBusy || IsUnavailable) return;

        visual.DOKill();
        
        if (hover) {
            BringToFront(500);
            AudioManager.Instance.PlaySFX(dingSFX);
            visual.DOLocalMove(Vector3.up * 1.2f, 0.2f)
                .SetEase(Ease.OutQuad)
                .OnStart(() => {  })
                .SetLink(gameObject);
        } else {
            visual.DOLocalMove(Vector3.zero, 0.2f)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject)
                .OnComplete(() => { BringToFront(); AudioManager.Instance.PlaySFX(downSFX); });
        }
    }


    private void PlayShake() {
        if (visual == null) return;
        if (IsBusy || IsUnavailable) return;

        if (shakeTween != null && shakeTween.IsActive() && shakeTween.IsPlaying())
            return;

        visual.localRotation = Quaternion.identity;

        shakeTween = visual.DOShakeRotation(
            0.8f,
            new Vector3(0f, 0f, 5f),
            80,
            90
        )
        .SetLink(gameObject)
        .OnComplete(() => {
            visual.localRotation = Quaternion.identity;
            shakeTween = null;
        });
    }

    private void PlayPour(Bottle nextBottle, System.Action onComplete = null) {
        if (IsBusy) return;
        if (nextBottle == null || visual == null) return;

        IsBusy = true;
        nextBottle.anim.IsUnavailable = true;
        BringToFront(1000);
        visual.DOKill();

        int movedAmount = Mathf.Abs(currentBottle.changes);
        float fillDuration = pourDuration * movedAmount;

        BottleView fromView = currentBottle.GetComponent<BottleView>();
        BottleView toView = nextBottle.GetComponent<BottleView>();

        int toStartCount = nextBottle.liquidUnits.Count - movedAmount;
        int toEndCount = nextBottle.liquidUnits.Count;

        fromView.SetMystery(0f);

        float fromEndFill =
            fromView.GetVisualFillAmount(currentBottle.liquidUnits.Count);

        float toStartFill = toStartCount <= 0
            ? toView.GetPourInStartFill(toStartCount)
            : toView.GetVisualFillAmount(toStartCount);

        float toEndFill =
            toView.GetVisualFillAmount(toEndCount);

        Vector3 targetPos = nextBottle.transform.position;
        targetPos.y += pourHeiOffset;

        float startAngle = pourStartAngle;
        float endAngle = pourEndAngle;

        if (originalPos.x > nextBottle.transform.position.x) {
            targetPos.x += pourCornerOffset;
        } else if (originalPos.x < nextBottle.transform.position.x) {
            targetPos.x -= pourCornerOffset;
            startAngle = -startAngle;
            endAngle = -endAngle;
        } else {
            if (originalPos.x >= 0) {
                targetPos.x -= pourCornerOffset;
                startAngle = -startAngle;
                endAngle = -endAngle;
            } else {
                targetPos.x += pourCornerOffset;
            }
        }

        Sequence sequence = DOTween.Sequence();
        Spill current = null;

        sequence.Append(
            visual.DOMove(targetPos, pourDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.Join(
            visual.DORotate(
                new Vector3(0f, 0f, startAngle),
                pourDuration
            )
            .SetEase(Ease.OutQuad)
        );

        sequence.AppendCallback(() =>
        {
            toView.RefreshColorsOnly(nextBottle.liquidUnits);

            current = nextBottle.anim.StartPourSpill(currentBottle, nextBottle, startAngle < 0 ? true : false);

            currentBottle.anim.TweenFillAmount(
                fromEndFill,
                fillDuration
            );

            nextBottle.anim.TweenFillAmount(
                toEndFill,
                fillDuration,
                toStartFill
            );
        });

        sequence.Append(
            visual.DORotate(
                new Vector3(0f, 0f, endAngle),
                fillDuration
            )
            .SetEase(Ease.Linear)
        );

        sequence.AppendCallback(() =>
        {
            nextBottle.anim.EndPourSpill(current);
            currentBottle.RefreshView();
            nextBottle.RefreshView();

            currentBottle.BottleSatisfy(nextBottle);
        });

        sequence.AppendInterval(0.15f);

        sequence.Append(
            visual.DOMove(originalPos, pourDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.Join(
            visual.DORotateQuaternion(originalRotation, pourDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.Append(
            visual.DOLocalMove(Vector3.zero, .2f)
            );

        sequence.SetLink(gameObject);

        sequence.OnComplete(() =>
        {
            fromView.SetMystery(1f);
            BringToFront();
            IsBusy = false;
            nextBottle.anim.IsUnavailable = false;
            onComplete?.Invoke();
        });
    }

    public void SetCap() {
        if (bottleCap == null) return;
        SpriteRenderer capRenderer = bottleCap.GetComponent<SpriteRenderer>();
        if (capRenderer == null) return;
        bottleCap.DOKill();

        bottleCap.gameObject.SetActive(true);
    }

    private void PlayCap() {
        if (bottleCap == null) return;

        bottleCap.DOKill();

        SpriteRenderer capRenderer = bottleCap.GetComponent<SpriteRenderer>();
        if (capRenderer == null) return;

        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(pourDuration * -currentBottle.changes + 0.2f);

        seq.Append(capRenderer.DOFade(1f, 0.2f).OnStart(() => {

            bottleCap.gameObject.SetActive(true);
        }
        ));

        seq.Join(
            bottleCap.DOLocalMove(Vector3.zero, 0.35f)
                .From(Vector2.up * 3f)
                .SetEase(Ease.InQuint)
        );

        seq.SetLink(gameObject);
    }

    public void PlayUnCap() {
        if (bottleCap == null) return;
        bottleCap.DOKill();

        SpriteRenderer capRenderer = bottleCap.GetComponent<SpriteRenderer>();
        if (capRenderer == null) return;

        Sequence seq = DOTween.Sequence();

        seq.Join(
            bottleCap.DOLocalMove(Vector2.up * 3f, 0.35f)
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

        seq.Append(
            cover.DOLocalMove(Vector3.zero, 0.45f)
                .SetEase(Ease.OutSine)
                .From(Vector3.up * 1.5f)
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

        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(2f);

        seq.Append(
            cover.DOLocalMove(Vector3.up * 1.5f, 0.45f)
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
        if (IsBusy) return;
        transform.DOKill();

        transform.DOMove(newPos, 0.35f)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject)
            .OnComplete(() => {
                originalPos = visual.position;
            });
    }

    public void BringToFront(int target = 0) {
        if (target == 0) target = originalSortingOrder;
        if (sortingGroup != null)
            sortingGroup.sortingOrder = target;
    }

    public void Play(int action, Bottle nextBottle = null, Vector3 newPos = default, System.Action onComplete = null) {
        if (action == 1) {
            PlayShake();
            onComplete?.Invoke();
            return;
        }

        if (IsBusy) return;

        switch (action) {
            case 2:
                PlayPour(nextBottle, onComplete);
                break;

            case 3:
                MoveBottleRoot(newPos);
                onComplete?.Invoke();
                break;

            case 4:
                PlayCap();
                onComplete?.Invoke();
                break;

            case 5:
                RemoveCover();
                onComplete?.Invoke();
                break;
        }
    }

    private void OnDestroy() {
        if (material != null)
            Destroy(material);
    }
}