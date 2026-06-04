using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BottleGen : MonoBehaviour
{
    [Header("GenObject")]
    public GameObject bottle;
    public UnityEvent<Bottle> newBot;
    private Dictionary<int, List<Bottle>> bottleDict = new();

    [Header("Settings")]
    public int minBottleCount = 2;
    public int maxBottleCount = 16;
    public int rowCount = 3;
    public int colCount = 6;
    public float xSpacing = 2.5f;
    public float ySpacing = 8.5f;
    public float pointNemoY = 7.5f;
    public float screenOffset = 50f;

    int genCount;
    private float spawnX;
    private int rowIndex = 0;

    public void AddBottle(int prefCol = 0) {
        if (genCount == maxBottleCount+2 || rowIndex == rowCount) {
            Debug.Log("Cannot add more bottles, max capacity reached!");
            return;
        }

        bool singularAdd = false;
        if (prefCol == 0) {
            prefCol = colCount;
            singularAdd = true;
            if (bottleDict.Count == 0) {
                bottleDict.Add(0, new());
            }
        }

        float startX;
        int j;
        int rowAdd = rowIndex;
        if ((bottleDict[rowIndex].Count < prefCol && genCount != 0) && !singularAdd) {

            float rowWidth = bottleDict[rowIndex].Count * xSpacing;
            startX = Vector2.zero.x - rowWidth / 2f;
            j = 0;
            for (int i = 0; i < bottleDict[rowIndex].Count; i++) {
                Vector2 newLoc = new Vector2(startX + j * xSpacing, pointNemoY - rowIndex * ySpacing);
                bottleDict[rowIndex][i].anim.Play(3, null, new Vector3(newLoc.x, newLoc.y, 0f));
                j++;
            }
        } else {

            int least = rowIndex;
            int minCount = int.MaxValue;

            foreach (var pair in bottleDict) {
                if (pair.Value.Count <= minCount) {
                    minCount = pair.Value.Count;
                    least = pair.Key;
                }
            }

            if (singularAdd && bottleDict.Count != 0 && bottleDict[least].Count < colCount) {
                

                float rowWidth = bottleDict[least].Count * xSpacing;
                startX = Vector2.zero.x - rowWidth / 2f;
                j = 0;
                for (int i = 0; i < bottleDict[least].Count; i++) {
                    Vector2 newLoc = new Vector2(startX + j * xSpacing, pointNemoY - least * ySpacing);
                    bottleDict[least][i].anim.Play(3, null, new Vector3(newLoc.x, newLoc.y, 0f));
                    j++;
                }
                rowAdd = least;
            } else {
                startX = Vector2.zero.x;
                j = 0;
                if (genCount != 0) {
                    rowIndex++;
                    rowAdd = rowIndex;
                }
                if (rowIndex == rowCount) return;
            }
        }

        Vector2 newBottleVec = new(spawnX, pointNemoY - rowAdd * ySpacing);

        GameObject newBottleObj = Instantiate(
            bottle,
            newBottleVec,
            Quaternion.identity,
            transform);

        genCount++;
        newBottleObj.name = $"Bottle_{genCount}";

        Bottle newBottle = newBottleObj.GetComponent<Bottle>();

        newBottle.anim.Play(
            3,
            null,
            new Vector3(startX + j * xSpacing, newBottleVec.y, 0f));
        newBot?.Invoke(newBottle);
        if (bottleDict.Count-1 < rowIndex) {
            DictManager(newBottle);
        }
        bottleDict[rowAdd].Add(newBottle.GetComponent<Bottle>());
    }

    private void DictManager(Bottle bottle) {
        bottleDict.Add(rowIndex, new());
    }

    public void RemoveBottle(Bottle bottle) {
        //THIS IS FOR DEBUGGING ONLY, AS SUCH, THIS FEATURE IS RESTRICTED

        if (genCount == 0) return;
        if (FindBottleInDict(bottle) == 5) return;
        int row = FindBottleInDict(bottle);

        float y = bottle.transform.position.y;

        bottleDict[row].Remove(bottle);
        Destroy(bottle.gameObject);

        float rowWidth = (bottleDict[row].Count-1) * xSpacing;
        float startX = Vector2.zero.x - rowWidth / 2f;

        for (int j = 0; j < bottleDict[row].Count; j++) {

            Vector2 newLoc = new Vector2(startX + j * xSpacing, y);
            bottleDict[row][j].anim.Play(3, null, new Vector3(newLoc.x, newLoc.y, 0f));

        }
        genCount--;
    }

    private int FindBottleInDict(Bottle bottle) {
        for (int i = 0; i < bottleDict.Count; i++) {
            for (int k = 0; k < bottleDict[i].Count; k++) {
                if (bottleDict[i][k] == bottle) return i;
            }
        }

        return 5;
    }

    public List<Bottle> DictionaryToSingularBottleConverter() {
        List<Bottle> listOfBottle = new();
        for (int i = 0; i < bottleDict.Count; i++) {
            for (int k = 0; k < bottleDict[i].Count; k++) {
                listOfBottle.Add(bottleDict[i][k]);
            }
        }
        return listOfBottle;
    }

    public void ClearBottles()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        rowIndex = 0;
        genCount = 0;
        bottleDict = new() {
            {0, new() }
        };
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
