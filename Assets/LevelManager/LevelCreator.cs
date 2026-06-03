using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelCreator : MonoBehaviour
{
    public AdminUIHandler ui;
    public BottleGen bottleGen;
    public LevelData levelData;
    public LevelTranslator translator;

    private string path = Application.dataPath + "/LevelManager/Levels/level_";

    private void SaveLevel(int result) {
        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(path + result.ToString(), json);

    }

    public void DataProcess() {
        if (!int.TryParse(ui.LevelInput, out int result)) return;
        levelData = new();
        List<Bottle> currentBottleData = bottleGen.bottles;


        levelData.levelNumber = result;
        levelData.bottleCount = currentBottleData.Count;
        Debug.Log(currentBottleData.Count);
        for (int i = 0; i < currentBottleData.Count; i++) {

            BottleData bottleData = new();
            bottleData.isLocked = currentBottleData[i].isLocked;
            bottleData.lockCondition = translator.TranslatedColor(currentBottleData[i].lockColor);

            for (int j = 0; j < currentBottleData[i].liquidUnits.Count; j++) {

                LiquidData liquidData = new();
                liquidData.colorId = translator.TranslatedColor(currentBottleData[i].liquidUnits[j].colorId);
                liquidData.isMystery = currentBottleData[i].liquidUnits[j].isMystery;
                bottleData.liquids.Add(liquidData);

            }
            levelData.bottles.Add(bottleData);
        }

        SaveLevel(result);
    }
}
