using DG.Tweening;
using UnityEngine;

public partial class AnimationHandler : MonoBehaviour
{
    
    public void SetPourLiquid(Color color, float fillAmount) {
        material.SetColor("_Color0", color);
        material.SetFloat("_FillAmount", fillAmount);
        material.SetFloat("_TiltAmount", 0);
    }

    private void SetDirection(bool dir) {
        //TRUE MEANS POUR FROM LEFT -> RIGHT || FALSE MEANS POUR FROM RIGHT -> LEFT
        material.SetFloat("_Direction", dir ? 1f : -1f);
    }

    void Update() {
        float angle = visual.eulerAngles.z;

        if (angle > 180f) angle -= 360f;
        float tilt = Mathf.Clamp01(Mathf.Abs(angle) /90f);


        material.SetFloat("_TiltAmount", tilt);
    }
}