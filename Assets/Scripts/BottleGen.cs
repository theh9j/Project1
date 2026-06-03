using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BottleGen : MonoBehaviour
{
    [Header("GenObject")]
    public GameObject bottle;
    public List<Bottle> bottles = new List<Bottle>();   
    public UnityEvent<Bottle> newBot;

    [Header("Settings")]
    public int minBottleCount = 2;
    public int maxBottleCount = 16;
    public int rowCount = 3;
    public int colCount = 6;
    public float xSpacing = 2.5f;
    public float ySpacing = 8.5f;
    public float pointNemoY = 7.5f;
    public float screenOffset = 50f;


    private int lastRowCount = 0;
    private float spawnX;
    private int rowIndex;
    private int genCount;

    public void AddBottle(int prefCol = 0) {
        if (genCount == maxBottleCount+2 || rowIndex == rowCount) {
            Debug.Log("Cannot add more bottles, max capacity reached!");
            return;
        }

        if (prefCol == 0) prefCol = colCount;


        float startX;
        int j;
        if (lastRowCount < prefCol && genCount != 0) {

            float rowWidth = lastRowCount * xSpacing;
            startX = Vector2.zero.x - rowWidth / 2f;
            j = 0;
            for (int i = genCount - lastRowCount; i < genCount; i++) {
                Vector2 newLoc = new Vector2(startX + j * xSpacing, pointNemoY - rowIndex * ySpacing);
                bottles[i].anim.Play(3, null, new Vector3(newLoc.x, newLoc.y, 0f));
                j++;
            }
            lastRowCount++;
        } else {
            startX = Vector2.zero.x;
            j = 0;
            lastRowCount = 1;
            if (genCount != 0) rowIndex++;
            if (rowIndex == rowCount) return;
        }

        Vector2 newBottleVec = new Vector2(spawnX, pointNemoY - rowIndex * ySpacing);

        GameObject newBottleTrans = Instantiate(
            bottle,
            newBottleVec,
            Quaternion.identity,
            transform);

        genCount++;
        newBottleTrans.name = $"Bottle_{genCount}";

        Bottle newBottle = newBottleTrans.GetComponent<Bottle>();

        newBottle.anim.Play(
            3,
            null,
            new Vector3(startX + j * xSpacing, newBottleVec.y, 0f));
        newBot?.Invoke(newBottle);
        bottles.Add(newBottle.GetComponent<Bottle>());
    }


    public void ClearBottles()
    {
        rowIndex = 0;
        genCount = 0;
        bottles = new List<Bottle>();
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void GenAmount(int amount)
    {
        if (amount < minBottleCount || amount > maxBottleCount)
        {
            Debug.LogError($"Invalid bottle count generation!");
            return;
        }

        ClearBottles();
        int prefCol = GetColumnsForBottleCount(amount);
        for (int i = 0; i < amount; i++) {
            AddBottle(prefCol);
        }
    }

    private int GetColumnsForBottleCount(int count)
    {
        return count switch
        {
            2 => 2,
            3 => 3,
            4 => 4,
            5 => 3,
            6 => 3, 
            7 => 4,
            8 => 4,
            9 => 5,
            10 => 5,
            11 => 6,
            _ => 6
        };
    }

    private void Start() {
        spawnX = Camera.main.ScreenToWorldPoint(
            new Vector3(Screen.width + screenOffset, 0f, Camera.main.nearClipPlane)
            ).x;
    }
}
