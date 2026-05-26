using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BottleView : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer[] liquidSlots;
    private LiquidColor color;

    public void Refresh(List<LiquidUnit> liquidUnits) {
        for (int i = 0; i < liquidSlots.Length; i++) {
            if (i >= liquidUnits.Count) {
                liquidSlots[i].gameObject.SetActive(false);
            }


        }
    }
}
