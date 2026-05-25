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

    public float xSpacing = 5f;
    public float ySpacing = 2.5f;

    public int genAmount = 5;


    public List<Bottle> GenerateBottles(int amount)
    {
        int maxSlot = rowCount * colCount;
        amount = Mathf.Min(amount, maxSlot);

        int genCount = 0;
        int rowIndex = 0;
        while (genCount < amount && rowIndex < rowCount)
        {
            int remaining = amount - genCount;

            int prefCol = GetColumnsForBottleCount(amount);
            int bottleThisRow = Mathf.Min(remaining, prefCol);

            float rowWidth = (bottleThisRow - 1) * xSpacing;

            float startX = Vector2.zero.x - rowWidth / 2f;

            for (int i = 0; i < bottleThisRow; i++)
            {
                Vector2 spawnPoint = new Vector2(startX + i * xSpacing, Vector2.zero.y - rowIndex * ySpacing);
                GameObject currentBot = Instantiate(bottle, spawnPoint, Quaternion.identity, transform);
                currentBot.name = $"Bottle_{genCount + 1}";
                Bottle bot = currentBot.GetComponent<Bottle>();
                bottles.Add(bot);
                genCount++;
            }
            rowIndex++;

        }
        return bottles;
    }

    public void ClearBottles()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public List<Bottle> GenAmount(int amount)
    {
        if (amount < minBottleCount || amount > maxBottleCount)
        {
            Debug.LogError($"Invalid bottle count!");
            return null;
        }
        bottles = GenerateBottles(amount);
        return bottles;
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
