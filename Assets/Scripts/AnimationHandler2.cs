using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public partial class AnimationHandler : MonoBehaviour
{
    void Start() {
        capPos = bottleCap.transform.position;
        originalPos = visual.position;
        targetPos = originalPos;
        originalRotation = Quaternion.identity;
        targetRot = originalRotation;

        sortingGroup = transform.GetComponent<SortingGroup>();
        originalSortingOrder = sortingGroup.sortingOrder;
    }


    void Update() {

        if (currentBottle.Completion) {
            if (ConSim(bottleCap.position, capPos)) {
                bottleCap.position = capPos;
                return;
            }

            if (bottleCap.position != capPos) {
                bottleCap.position = Vector3.Lerp(bottleCap.position, capPos, Time.deltaTime * 5);
            }
        }

        if (ConSim(visual.position, originalPos)) {
            transform.position = originalPos;
            visual.position = originalPos;
        }

        if (visual.position == targetPos) {
            return;
        } else if (ConSim(visual.position, targetPos)) {
            visual.position = targetPos;
            visual.rotation = targetRot;
            return;
        }
        visual.position = Vector3.Lerp(visual.position, targetPos, Time.deltaTime * 5);
        visual.rotation = Quaternion.Lerp(visual.rotation, targetRot, Time.deltaTime * 5);

    }

    private IEnumerator TopPosition(bool isIt, bool isPour = false) {
        if (isIt) {
            sortingGroup.sortingOrder = 1000;
        } else {
            if (isPour) yield return new WaitForSeconds(pourDuration + Time.deltaTime * 5);
            sortingGroup.sortingOrder = originalSortingOrder;
        }
    }

    public void SelectedHover(bool hover, bool wasPour = false) {
        if (hover) {
            targetPos = originalPos + Vector3.up * 1.2f;
            StartCoroutine(TopPosition(hover));
        } else {
            targetPos = originalPos;
            StartCoroutine(TopPosition(hover, wasPour));
        }
    }

    public void Play(int action, Bottle nextBottle = null, Vector3 newPos = new Vector3()) {
        if (IsBusy) return;

        switch (action) {
            case 1:
                StartCoroutine(ShakeRoutine());
                break;
            case 2:
                StartCoroutine(PourRoutine(nextBottle));
                break;
            case 3:
                StartCoroutine(AddBottle(newPos));
                break;
            case 4:
                StartCoroutine(Cap(newPos));
                break;
            case 5:
                StartCoroutine(RemoveCover(newPos));
                break;
            default:
                break;
        }
    }
}
