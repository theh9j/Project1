using UnityEngine;

public partial class AnimationHandler : MonoBehaviour {
    [Header("Liquid Surface")]
    [SerializeField] private Transform liquidSurface;
    [SerializeField] private SpriteRenderer liquidSurfaceRenderer;

    [Header("Liquid Surface Anchors")]
    [SerializeField] private Transform surfaceBottomPoint;
    [SerializeField] private Transform surfaceTopPoint;

    [Header("Surface Position Tuning")]
    [SerializeField] private float surfaceYOffset = -.8f;
    [SerializeField] private float surfaceSideShiftAmount = 1.5f;
    [SerializeField] private float surfaceDrainShiftAmount = 1.0f;
    [SerializeField] private float surfaceFillMultiplier = 2.5f;
    [SerializeField] private float surfaceFillOffset = -.6f;

    [Header("Surface Scale Tuning")]
    [SerializeField] private float surfaceBaseWidth = 2.1f;
    [SerializeField] private float surfaceMaxWidth = 6f;
    [SerializeField] private float surfaceBaseHeight = 0.08f;

    [Header("Surface Movement Tuning")]
    [SerializeField] private float surfaceMoveSpeed = 12f;
    [SerializeField] private float surfaceScaleSpeed = 12f;
    [SerializeField] private float surfaceRotationMultiplier = 0.35f;

    private bool surfaceHasLiquid;

    public void SetLiquidSurface(Color color, float fillAmount, bool hasLiquid) {
        if (liquidSurface == null || liquidSurfaceRenderer == null)
            return;

        surfaceHasLiquid = hasLiquid;
        liquidSurface.gameObject.SetActive(hasLiquid);

        if (!hasLiquid)
            return;

        liquidSurfaceRenderer.color = color;

        UpdateLiquidSurface(true);
    }

    private void UpdateLiquidSurface(bool instant = false) {
        if (!surfaceHasLiquid) return;
        if (liquidSurface == null) return;
        if (surfaceBottomPoint == null || surfaceTopPoint == null) return;
        if (material == null || visual == null) return;

        float fill = material.GetFloat("_FillAmount");
        float tilt = material.GetFloat("_TiltAmount");
        float direction = material.GetFloat("_Direction");

        float signedTilt = tilt * direction;

        float surfaceFill = fill * surfaceFillMultiplier + surfaceFillOffset;
        surfaceFill = Mathf.Clamp01(surfaceFill);

        Vector3 baseLocalPos = Vector3.Lerp(
            surfaceBottomPoint.localPosition,
            surfaceTopPoint.localPosition,
            surfaceFill
        );

        // Small shift toward the low side, not extreme corner
        float drainFactor = 1f - surfaceFill;

        Vector3 sideShift =
            -Vector3.right * signedTilt * surfaceSideShiftAmount;

        Vector3 drainShift =
            -Vector3.right * signedTilt * drainFactor * surfaceDrainShiftAmount;

        Vector3 targetLocalPos =
            baseLocalPos +
            sideShift +
            drainShift +
            Vector3.up * surfaceYOffset;

        float targetWidth = Mathf.Lerp(
            surfaceBaseWidth,
            surfaceMaxWidth,
            tilt
        );

        Vector3 targetScale = new Vector3(
            targetWidth,
            surfaceBaseHeight,
            1f
        );

        float bottleAngle = visual.eulerAngles.z;
        if (bottleAngle > 180f)
            bottleAngle -= 360f;

        // Cancels most bottle rotation so surface stays closer to liquid edge
        Quaternion targetRot = Quaternion.Euler(
            0f,
            0f,
            -bottleAngle * surfaceRotationMultiplier
        );

        if (instant) {
            liquidSurface.localPosition = targetLocalPos;
            liquidSurface.localScale = targetScale;
            liquidSurface.localRotation = targetRot;
        } else {
            liquidSurface.localPosition = Vector3.Lerp(
                liquidSurface.localPosition,
                targetLocalPos,
                Time.deltaTime * surfaceMoveSpeed
            );

            liquidSurface.localScale = Vector3.Lerp(
                liquidSurface.localScale,
                targetScale,
                Time.deltaTime * surfaceScaleSpeed
            );

            liquidSurface.localRotation = targetRot;
        }
    }
}