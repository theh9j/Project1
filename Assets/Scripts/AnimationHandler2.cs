using UnityEngine;

public partial class AnimationHandler : MonoBehaviour {
    [Header("Liquid Surface")]
    [SerializeField] private Transform liquidSurface;
    [SerializeField] private SpriteRenderer liquidSurfaceRenderer;

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
}