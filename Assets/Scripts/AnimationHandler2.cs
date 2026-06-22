using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public partial class AnimationHandler : MonoBehaviour {
    [Header("Liquid Surface")]
    [SerializeField] private Transform liquidSurface;
    [SerializeField] private SpriteRenderer liquidSurfaceRenderer;

    [Header("Liquid Spill")]
    [SerializeField] private Transform spillParents;
    [SerializeField] private GameObject liquidSpillPrefab;
    [SerializeField] private float spillWidth = 0.25f;

    private Transform liquidSpill;
    private SpriteRenderer liquidSpillRenderer;

    private Transform spillStartPoint;
    private Transform spillEndSurface;

    private bool isSpilling;

    [Header("Liquid Surface Settings")]
    [SerializeField] private float surfaceYOffset = 0.02f;
    [SerializeField] private float surfaceHeight = 0.12f;
    [SerializeField] private float surfaceWidthPadding = 1f;
    [SerializeField] private float surfaceFollowSpeed = 30f;
    [SerializeField] private float liquidVisualHeight = .95f;

    public void SetLiquidSurface(Color color, bool hasLiquid) {
        if (liquidSurface == null || liquidSurfaceRenderer == null)
            return;

        liquidSurface.gameObject.SetActive(hasLiquid);

        if (!hasLiquid)
            return;

        liquidSurfaceRenderer.color = color;
        UpdateLiquidSurface(true);
    }

    private void UpdateLiquidSurface(bool instant = false) {
        if (liquidSurface == null || liquidRenderer == null || material == null)
            return;

        if (!liquidSurface.gameObject.activeSelf)
            return;

        Bounds liquidBounds = liquidRenderer.bounds;

        float fill = material.GetFloat("_FillAmount");

        float liquidTopY = Mathf.Lerp(
            liquidBounds.min.y,
            liquidBounds.max.y,
            fill * liquidVisualHeight
        );

        Vector3 targetPos = new Vector3(
            liquidBounds.center.x,
            liquidTopY + surfaceYOffset,
            liquidSurface.position.z
        );

        float targetWidth = liquidBounds.size.x * surfaceWidthPadding;

        Vector3 targetScale = new Vector3(
            targetWidth,
            surfaceHeight,
            1f
        );

        float speed = instant ? 999f : surfaceFollowSpeed;

        liquidSurface.position = targetPos;

        liquidSurface.localScale = targetScale;

        liquidSurface.rotation = Quaternion.identity;
    }

    private Color Lighten(Color c, float amount) {
        return Color.Lerp(c, Color.white, amount);
    }

    private void HighlightSpill(Color c, bool rightSide) {
        if (liquidSpillRenderer == null) return;
            
        spillMat = new Material(liquidSpillRenderer.sharedMaterial);
        spillMat.SetColor("_Color", c);
        spillMat.SetFloat("_Highlight", rightSide ? 1f : 0f);
        liquidSpillRenderer.material = spillMat;
    }

    public void StartPourSpill(Bottle fromBottle, Bottle toBottle, bool rightSide) {
        if (liquidSpill != null)
            Destroy(liquidSpill.gameObject);

        liquidSpill = Instantiate(
            liquidSpillPrefab,
            spillParents
        ).transform;

        liquidSpillRenderer =
            liquidSpill.GetComponent<SpriteRenderer>();

        spillStartPoint = fromBottle.anim.bottleNeck;
        spillEndSurface = toBottle.anim.liquidSurface;

        HighlightSpill(colorTranslate.GetColor(toBottle.GetTopColor()), rightSide);

        liquidSpillRenderer.material = spillMat;

        spillParents.position = spillStartPoint.position;
        spillParents.rotation = Quaternion.identity;
        spillParents.localScale = Vector3.one;

        liquidSpill.localPosition = Vector3.zero;
        liquidSpill.localRotation = Quaternion.identity;
        liquidSpill.localScale = new Vector3(spillWidth, 0f, 1f);

        isSpilling = true;
    }

    private void UpdatePourSpill() {
        if (liquidSpill == null)
            return;

        Vector3 start = spillStartPoint.position;
        float endY = spillEndSurface.position.y;

        float length = Mathf.Abs(start.y - endY);

        spillParents.position = start;
        spillParents.rotation = Quaternion.identity;

        liquidSpill.localPosition =
            new Vector3(0f, -length * 0.5f, 0f);

        liquidSpill.localScale =
            new Vector3(spillWidth, length, 1f);
    }

    public void EndPourSpill() {
        if (liquidSpill == null)
            return;

        isSpilling = false;

        Transform spillToDestroy = liquidSpill;

        liquidSpill = null;
        liquidSpillRenderer = null;
        spillStartPoint = null;
        spillEndSurface = null;

        spillToDestroy
            .DOScaleX(0f, 0.25f)
            .SetEase(Ease.InQuad)
            .OnComplete(() => {
                if (spillToDestroy != null)
                    Destroy(spillToDestroy.gameObject);
            });
    }
}