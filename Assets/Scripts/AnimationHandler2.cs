using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public partial class AnimationHandler : MonoBehaviour {
    [Header("Liquid Surface")]
    public Transform liquidSurface;
    [SerializeField] private SpriteRenderer liquidSurfaceRenderer;

    [Header("Liquid Spill")]
    [SerializeField] private Spill spill;
    [SerializeField] private Transform spillParents;
    [SerializeField] private GameObject liquidSpillPrefab;

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

    [Header("Audio")]
    [SerializeField] private AudioClip pourSFX;
    [SerializeField] private AudioClip dingSFX;
    [SerializeField] private AudioClip downSFX;

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

    public Spill StartPourSpill(Bottle fromBottle, Bottle toBottle, bool rightSide) {
        Spill newSpill = Instantiate(
            liquidSpillPrefab,
            spillParents
        ).GetComponent<Spill>();

        AudioManager.Instance.PlaySFX(pourSFX);

        newSpill.Init(
            fromBottle,
            toBottle,
            colorTranslate.GetColor(toBottle.GetTopColor()),
            rightSide
        );

        return newSpill;
    }

    public void EndPourSpill(Spill targetSpill) {
        if (targetSpill == null)
            return;

        AudioManager.Instance.StopSFX();

        targetSpill.EndPourSpill();
    }
}