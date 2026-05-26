using System.Collections;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    private Vector3 originalPos;
    private Vector3 targetPos;
    private Quaternion originalRotation;
    private Quaternion targetRot;
    [SerializeField] private Transform visual;

    public float pourCornerOffset = 3f;
    public float pourHeiOffset = 4f;
    public float pourDuration = 2f;
    public float pourAngle = 95f;

    private float shakeDuration = 0.02f;
    private float shakeAngle = 2f;

    private bool isBusy = false;

    void Start() {

        originalPos = visual.position;
        targetPos = originalPos;
        originalRotation = Quaternion.identity;
        targetRot = originalRotation;
    }

    void Update() {

        visual.position = Vector3.Lerp(visual.position, targetPos, Time.deltaTime * 5);
        visual.rotation = Quaternion.Lerp(visual.rotation, targetRot, Time.deltaTime * 5);
    }

    public void SelectedHover(bool hover) {
        if (hover) {
            targetPos = originalPos + Vector3.up * 2f;
        } else {
            targetPos = originalPos;
        }
    }

    public void Play(int action, Transform nextBottle = null) {
        if (isBusy) return;

        switch (action) {
            case 1:
                StartCoroutine(ShakeRoutine());
                break;
            case 2:
                StartCoroutine(PourRoutine(nextBottle));
                break;
            default:
                break;
        }
            
    }

    private IEnumerator ShakeRoutine() {
        isBusy = true;

        for (int i = 0; i < 3; i++) {
            visual.rotation = Quaternion.Euler(0, 0, shakeAngle);
            yield return new WaitForSeconds(shakeDuration);

            visual.rotation = Quaternion.Euler(0, 0, -shakeAngle);
            yield return new WaitForSeconds(shakeDuration);
        }

        visual.rotation = originalRotation;

        isBusy = false;
    }

    private IEnumerator PourRoutine(Transform nextBottle) {
        isBusy = true;
        Vector3 newPos = nextBottle.position;
        Quaternion newRot = nextBottle.rotation;

        newPos.y = newPos.y + pourHeiOffset;
        if (originalPos.x > newPos.x) {
            newPos.x = newPos.x + pourCornerOffset;
            newRot = Quaternion.Euler(0, 0, pourAngle);
        } else if (originalPos.x < newPos.x) {
            newPos.x = newPos.x - pourCornerOffset;
            newRot = Quaternion.Euler(0, 0, -pourAngle);
        } else {
            Debug.Log("Math went wrong");
        }

        targetRot = newRot;
        targetPos = newPos;
        

        yield return new WaitForSeconds(pourDuration);
        targetRot = originalRotation;
        SelectedHover(false);
    }
}
