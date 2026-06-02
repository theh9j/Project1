using System.Collections.Generic;
using UnityEngine;

public class BottleGen : MonoBehaviour
{
    [Header("GenObject")]
    public GameObject bottle;
    public List<Bottle> bottles = new List<Bottle>();

    [Header("Settings")]
    public int minBottleCount = 2;
    public int maxBottleCount = 16;
    public int rowCount = 3;
    public int colCount = 6;
    public float xSpacing = 2.5f;
    public float ySpacing = 8.5f;
    public float pointNemoY = 7.5f;
    public float screenOffset = 50f;


    private int prefCol = 6;
    private int amount;
    private int lastRowCount = 0;
    private float spawnX;
    private int rowIndex;
    private int genCount;

    public void GenerateBottles()
    {
        int maxSlot = rowCount * colCount;
        amount = Mathf.Min(amount, maxSlot);

        genCount = 0;
        rowIndex = 0;
        while (genCount < amount && rowIndex < rowCount)
        {
            lastRowCount = 0;
            int remaining = amount - genCount;

            prefCol = GetColumnsForBottleCount(amount);
            int bottleThisRow = Mathf.Min(remaining, prefCol);

            float rowWidth = (bottleThisRow - 1) * xSpacing;

            float startX = Vector2.zero.x - rowWidth / 2f;

            for (int i = 0; i < bottleThisRow; i++)
            {
                Vector2 spawnPoint = new Vector2(startX + i * xSpacing, pointNemoY - rowIndex * ySpacing);
                GameObject currentBot = Instantiate(bottle, spawnPoint, Quaternion.identity, transform);
                currentBot.name = $"Bottle_{genCount + 1}";
                Bottle bot = currentBot.GetComponent<Bottle>();
                bottles.Add(bot);
                genCount++;
                lastRowCount++;
            }
            rowIndex++;
        }
        rowIndex--;
    }

    public void AddBottle() {
        if (genCount == maxBottleCount+2) {
            Debug.Log("Cannot add more bottles, max capacity reached!");
            return;
        }

        float startX;
        int j;
        if (lastRowCount < colCount) {

            float rowWidth = lastRowCount * xSpacing;
            startX = Vector2.zero.x - rowWidth / 2f;
            j = 0;
            for (int i = (prefCol * rowIndex); i < genCount; i++) {
                Vector2 newLoc = new Vector2(startX + j * xSpacing, pointNemoY - rowIndex * ySpacing);
                bottles[i].anim.Play(3, null, new Vector3(newLoc.x, newLoc.y, 0f));
                j++;
            }
            lastRowCount++;
        } else {
            startX = Vector2.zero.x;
            j = 0;
            rowIndex++;
            lastRowCount = 1;
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

        bottles.Add(newBottle.GetComponent<Bottle>());
    }


    public void ClearBottles()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
            bottles = new List<Bottle>();
        }
    }

    public void GenAmount(int amount)
    {
        if (amount < minBottleCount || amount > maxBottleCount)
        {
            Debug.LogError($"Invalid bottle count generation!");
            return;
        }
        this.amount = amount;
        GenerateBottles();
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
