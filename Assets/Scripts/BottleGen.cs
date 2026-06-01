using System.Collections.Generic;
using UnityEngine;

public class BottleGen : MonoBehaviour
{
    [Header("GenObject")]
    public GameObject bottle;
    List<Bottle> bottles = new List<Bottle>();

    [Header("Settings")]
    public int minBottleCount = 2;
    public int maxBottleCount = 16;
    public int rowCount = 3;
    public int colCount = 6;
    public float xSpacing = 2.5f;
    public float ySpacing = 8.5f;
    public float pointNemoY = 7.5f;


    private int prefCol = 6;
    private int amount;
    private int lastRowCount = 0;
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
    }

    public void AddBottle() {
        if (lastRowCount < prefCol) {
            if (genCount == maxBottleCount) {
                Debug.Log("Cannot add more bottles, max capacity reached!");
                return;
            }
            rowIndex--;

            float rowWidth = lastRowCount * xSpacing;
            float startX = Vector2.zero.x - rowWidth / 2f;
            int j = 0;
            for (int i = (prefCol * rowIndex); i < genCount; i++) {
                Vector2 newLoc = new Vector2(startX + j * xSpacing, pointNemoY - rowIndex * ySpacing);
                bottles[i].anim.Play(3, null, new Vector3(newLoc.x, newLoc.y, 0f));
                j++;
            }
            genCount++;
            lastRowCount++;

            Vector2 newBottleVec = new Vector2(startX + j * xSpacing, pointNemoY - rowIndex * ySpacing);
            GameObject newBottle = Instantiate(bottle, newBottleVec, Quaternion.identity, transform);
            newBottle.name = $"Bottle_{genCount}";
            bottles.Add(newBottle.GetComponent<Bottle>());
        }

    }

    public void ClearBottles()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void GenAmount(int amount)
    {
        if (amount < minBottleCount || amount > maxBottleCount)
        {
            Debug.LogError($"Invalid bottle count!");
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
}
