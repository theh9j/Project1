using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    private Vector3 originalPos;
    private Vector3 targetPos;

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
}
