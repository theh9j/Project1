using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class AnimationHandler : MonoBehaviour
{
    private Vector3 originalPos;
    private Vector3 targetPos;
    private Quaternion originalRotation;
    private Quaternion targetRot;

    private Vector3 newPos;
    private Quaternion newRot;

    [SerializeField] private Transform visual;
    [SerializeField] private Bottle currentBottle;

    public float pourCornerOffset = 3.1f;
    public float pourHeiOffset = 4f;
    public float pourDuration = 0.35f;
    public float pourAngle = 95f;

    private float shakeDuration = 0.02f;
    private float shakeAngle = 2f;

    private bool isBusy = false;
    private int originalSortingOrder;
    private SortingGroup sortingGroup;

    void Start() {

        originalPos = visual.position;
        targetPos = originalPos;
        originalRotation = Quaternion.identity;
        targetRot = originalRotation;

        sortingGroup = currentBottle.GetComponent<SortingGroup>();
        originalSortingOrder = sortingGroup.sortingOrder;

        //Time.timeScale = 0.25f;
    }

    void Update() {

        visual.position = Vector3.Lerp(visual.position, targetPos, Time.deltaTime * 5);
        visual.rotation = Quaternion.Lerp(visual.rotation, targetRot, Time.deltaTime * 5);
    }

    public void SelectedHover(bool hover) {
        if (hover) {
            StartCoroutine(TopPosition(true));
            targetPos = originalPos + Vector3.up * 2f;
        } else {
            targetPos = originalPos;
            StartCoroutine(TopPosition(false));
        }
    }

    public void Play(int action, Bottle nextBottle = null, Vector3 newPos = new Vector3()) {
        if (isBusy) return;

        switch (action) {
            case 1:
                StartCoroutine(ShakeRoutine());
                break;
            case 2:
                StartCoroutine(PourRoutine(nextBottle));
                break;
            case 3:
                StartCoroutine(NewBottleRoutine(newPos));
                break;
            default:
                break;
        }
    }

    private IEnumerator TopPosition(bool isIt, bool isPour = false) {
        if (isIt) {
            sortingGroup.sortingOrder = 1000;
        } else {
            if (isPour) yield return new WaitForSeconds(pourDuration + Time.deltaTime * 5); //WIP need fixing with timing
            sortingGroup.sortingOrder = originalSortingOrder;
        }
    }

    private IEnumerator NewBottleRoutine(Vector3 newPos) {
        targetPos = newPos;
        originalPos = targetPos;
        yield return new WaitForSeconds(0.5f);
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

    private void LeftPour() {
        newPos.x = newPos.x - pourCornerOffset;
        newRot = Quaternion.Euler(0, 0, -pourAngle);
    }

    private void RightPour() {
        newPos.x = newPos.x + pourCornerOffset;
        newRot = Quaternion.Euler(0, 0, pourAngle);
    }

    private IEnumerator PourRoutine(Bottle nextBottle) {
        isBusy = true;

        Transform bottleIndex = nextBottle.transform;
        newPos = bottleIndex.position;
        newRot = bottleIndex.rotation;

        newPos.y = newPos.y + pourHeiOffset;
        if (originalPos.x > newPos.x) {
            Debug.Log("Right");
            RightPour();
        } else if (originalPos.x < newPos.x) {
            Debug.Log("Left");
            LeftPour();
        } else {
            if (originalPos.x >= 0) {
                Debug.Log("Middle-Left");
                targetPos = originalPos + Vector3.left * 2f;
                LeftPour(); 
            } else {
                Debug.Log("Middle-Right");
                targetPos = originalPos + Vector3.right * 2f;
                RightPour();
            }
        }

        targetRot = newRot;
        targetPos = newPos;
        

        yield return new WaitForSeconds(pourDuration * currentBottle.lastChanges);
        currentBottle.RefreshView();
        nextBottle.RefreshView();
        yield return new WaitForSeconds(0.2f);
        newRot = Quaternion.identity;
        newPos = Vector3.zero;

        targetRot = originalRotation;
        SelectedHover(false);
        StartCoroutine(TopPosition(false, true));
        isBusy = false;
    }
}
