using System.Collections;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    private Vector3 originalPos;
    private Vector3 targetPos;

    private float shakeDuration = 0.02f;
    private float shakeAngle = 2f;

    private bool isShaking = false;

    void Start() {
        originalPos = transform.position;
        targetPos = originalPos;
    }

    void Update() {

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5);
    }

    public void SelectedHover(bool hover) {
        if (hover) {
            targetPos = originalPos + Vector3.up * 2f;
        } else {
            targetPos = originalPos;
        }
    }

    public void PlayShake() {
        if (isShaking) return;

        StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine() {
        isShaking = true;

        Quaternion originalRotation = transform.rotation;

        for (int i = 0; i < 3; i++) {
            transform.rotation = Quaternion.Euler(0, 0, shakeAngle);
            yield return new WaitForSeconds(shakeDuration);

            transform.rotation = Quaternion.Euler(0, 0, -shakeAngle);
            yield return new WaitForSeconds(shakeDuration);
        }

        transform.rotation = originalRotation;

        isShaking = false;
    }
}
