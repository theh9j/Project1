using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public partial class AnimationHandler : MonoBehaviour
{
    private Vector3 originalPos;
    private Vector3 targetPos;
    private Quaternion originalRotation;
    private Quaternion targetRot;

    private Vector3 newPos;
    private Quaternion newRot;
    private Vector3 capPos;

    [SerializeField] private Transform bottleCap;
    [SerializeField] private Transform cover;
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

    private IEnumerator RemoveCover(Vector3 newPos) {
        Color cloth = cover.GetComponent<SpriteRenderer>().color;
        for (int i = 0; i < 5; i++) {
            yield return new WaitForSeconds(0.1f);
            cloth.a -= .2f;
            cover.GetComponent<SpriteRenderer>().color = cloth;
        }
        cover.gameObject.SetActive(false);
    }

    private IEnumerator Cap(Vector3 newPos) {
        yield return new WaitForSeconds(pourDuration * transform.GetComponent<Bottle>().changes + .5f);
        Color cap = bottleCap.GetComponent<SpriteRenderer>().color;
        cap.a = 0;
        bottleCap.GetComponent<SpriteRenderer>().color = cap;
        bottleCap.gameObject.SetActive(true);
        capPos = newPos;
        for (int i = 0; i < 5; i++) {
            yield return new WaitForSeconds(0.1f);
            cap.a += 0.2f;
            bottleCap.GetComponent<SpriteRenderer>().color = cap;
        }
    }

    private IEnumerator AddBottle(Vector3 newPos) {
        yield return new WaitForEndOfFrame(); // To prevent runs before Start()
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

        yield return new WaitForSeconds(pourDuration * currentBottle.changes);
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

    private bool ConSim(Vector3 a, Vector3 b) {
        if (Vector3.Distance(a, b) < .005f) return true;
        return false;
    }

}
