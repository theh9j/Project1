using DG.Tweening;
using UnityEngine;

public class Spill : MonoBehaviour {
    private Bottle fromBottle;
    private Bottle toBottle;

    private SpriteRenderer render;
    private Material material;

    private bool isPouring;

    [SerializeField] private float spillWidth = .25f;

    private void Awake() {
        render = GetComponent<SpriteRenderer>();
        material = new Material(render.sharedMaterial);
        render.material = material;
    }

    public void Init(
        Bottle fromBottle,
        Bottle toBottle,
        Color color,
        bool rightSide) {
        this.fromBottle = fromBottle;
        this.toBottle = toBottle;

        material.SetColor("_Color", color);
        material.SetFloat("_Highlight", rightSide ? 1f : 0f);

        transform.localScale = new Vector3(spillWidth, 0f, 1f);
        transform.rotation = Quaternion.identity;

        isPouring = true;

        UpdatePourSpill();
    }

    private void Update() {
        if (isPouring)
            UpdatePourSpill();
    }

    private void UpdatePourSpill() {
        if (fromBottle == null || toBottle == null)
            return;

        Vector3 start = fromBottle.anim.bottleNeck.position;
        float endY = toBottle.anim.liquidSurface.position.y;

        float length = Mathf.Abs(start.y - endY);

        transform.position = new Vector3(
            start.x,
            start.y - length * 0.5f,
            start.z
        );

        transform.rotation = Quaternion.identity;

        transform.localScale = new Vector3(
            spillWidth,
            length,
            1f
        );
    }

    public void EndPourSpill() {
        isPouring = false;

        transform.DOKill();

        transform.DOScaleX(0f, .25f)
            .SetEase(Ease.InQuad)
            .OnComplete(() => {
                Destroy(gameObject);
            });
    }
}