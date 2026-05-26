using UnityEngine;

public class LiquidSlotView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer liquid;
    [SerializeField] private SpriteRenderer mystery;

    public void SetLiquid(Color color, bool isMystery, int i) {
        liquid.gameObject.SetActive(true);

        liquid.color = color;
        if (i != 3) mystery.gameObject.SetActive(isMystery);
    }

    public void Clear() {
        liquid.gameObject.SetActive(false);
        mystery.gameObject.SetActive(false);
    }
}
