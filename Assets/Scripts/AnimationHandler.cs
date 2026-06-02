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
    private Vector3 capPos;

    [SerializeField] private Transform bottleCap;
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

        sortingGroup = transform.GetComponent<SortingGroup>();
        originalSortingOrder = sortingGroup.sortingOrder;
    }


    void Update() {

        if (currentBottle.Completion) {
            if (bottleCap.position == capPos) return;

            if (Vector3.Distance(bottleCap.position, capPos) < 0.001f) {
                bottleCap.position = capPos;
                return;
            } 
            bottleCap.position = Vector3.Lerp(bottleCap.position, capPos, Time.deltaTime * 5);
        }

        if (Vector3.Distance(visual.position, originalPos) < .001f) {
            transform.position = originalPos;
            visual.position = originalPos;
        }

        if (visual.position == targetPos) {
            return;
        } else if (Vector3.Distance(visual.position, targetPos) < .001f) {
            visual.position = targetPos;
            visual.rotation = targetRot;
            return;
        }
        visual.position = Vector3.Lerp(visual.position, targetPos, Time.deltaTime * 5);
        visual.rotation = Quaternion.Lerp(visual.rotation, targetRot, Time.deltaTime * 5);


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
            default:
                break;
        }
    }

    private IEnumerator Cap(Vector3 newPos) {
        yield return new WaitForSeconds(pourDuration * currentBottle.LastChanges);
        Color cap = bottleCap.GetComponent<SpriteRenderer>().color;
        cap.a = 0;
        bottleCap.GetComponent<SpriteRenderer>().color = cap;
        capPos = newPos;
        bottleCap.gameObject.SetActive(true);
        for (int i = 0; i < 5; i++) {
            yield return new WaitForSeconds(0.1f);
            cap.a += 0.2f;
            bottleCap.GetComponent<SpriteRenderer>().color = cap;
        }
    }

    private IEnumerator TopPosition(bool isIt, bool isPour = false) {
        if (isIt) {
            sortingGroup.sortingOrder = 1000;
        } else {
            if (isPour) yield return new WaitForSeconds(pourDuration + Time.deltaTime * 5);
            sortingGroup.sortingOrder = originalSortingOrder;
        }
    }

    private IEnumerator AddBottle(Vector3 newPos) {
        yield return new WaitForSeconds(0.01f); // To prevent runs before Start()
        targetPos = newPos;
        originalPos = newPos;
    }

    private IEnumerator ShakeRoutine() {
        IsBusy = true;

        for (int i = 0; i < 3; i++) {
            visual.rotation = Quaternion.Euler(0, 0, shakeAngle);
            yield return new WaitForSeconds(shakeDuration);

            visual.rotation = Quaternion.Euler(0, 0, -shakeAngle);
            yield return new WaitForSeconds(shakeDuration);
        }

        visual.rotation = originalRotation;

        IsBusy = false;
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
        IsBusy = true;

        Transform bottleIndex = nextBottle.transform;
        newPos = bottleIndex.position;
        newRot = bottleIndex.rotation;

        newPos.y = newPos.y + pourHeiOffset;
        if (originalPos.x > newPos.x) {
            RightPour();
        } else if (originalPos.x < newPos.x) {
            LeftPour();
        } else {
            if (originalPos.x >= 0) {
                targetPos = originalPos + Vector3.left * 2f;
                LeftPour(); 
            } else {
                targetPos = originalPos + Vector3.right * 2f;
                RightPour();
            }
        }

        targetRot = newRot;
        targetPos = newPos;
        

        yield return new WaitForSeconds(pourDuration * currentBottle.LastChanges);
        currentBottle.RefreshView();
        nextBottle.RefreshView();
        yield return new WaitForSeconds(0.2f);
        newRot = Quaternion.identity;
        newPos = Vector3.zero;

        targetRot = originalRotation;
        SelectedHover(false);
        IsBusy = false;
    }

    public bool IsBusy {
        get { return isBusy; }
        private set { isBusy = value; }
    }

}
