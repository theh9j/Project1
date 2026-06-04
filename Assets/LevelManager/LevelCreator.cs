using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelCreator : MonoBehaviour
{
    public AdminUIHandler ui;
    public BottleGen bottleGen;
    public LevelTranslator translator;
    public GameManager gameManager;

    private LevelData levelData;
    private Dictionary<int, LiquidColor> colorTranslate;

    private string path = Application.dataPath + "/LevelManager/Levels/level_";

    private void SaveLevel(int result) {
        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(path + result.ToString("D2"), json);
    }

    public void DataProcess() {
        if (!int.TryParse(ui.LevelInput, out int result)) return;
        levelData = new();
        List<Bottle> currentBottleData = bottleGen.DictionaryToSingularBottleConverter();


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

    public void LoadLevel(bool randomize = false) {
        if (!int.TryParse(ui.LevelInput, out int result)) return;
        if (!File.Exists(path + result.ToString("D2"))) return;

        string json = File.ReadAllText(path + result.ToString("D2"));
        LevelData data = JsonUtility.FromJson<LevelData>(json);

        LoadData(data, randomize);
    }

    private void LoadData(LevelData data, bool randomize) {
        gameManager.OnGameStart();
        if (randomize) {
            colorTranslate = translator.Randomizer();
        } else {
            colorTranslate = new();
            for (int i = 0; i < Enum.GetValues(typeof(LiquidColor)).Length; i++) {
                colorTranslate[i] = ((LiquidColor[])Enum.GetValues(typeof(LiquidColor)))[i];
            }
        }

        ui.SetLevel(data.levelNumber.ToString());
        bottleGen.GenAmount(data.bottleCount);

        List<Bottle> bottleList = bottleGen.DictionaryToSingularBottleConverter();

        for (int i = 0; i < data.bottleCount; i++) {

            if (data.bottles[i].isLocked) {
                bottleList[i].SetLocker(ColorDebug(data.bottles[i].lockCondition));
            } 
            

            for (int j = 0; j < data.bottles[i].liquids.Count; j++) {

                LiquidUnit pendingLiquid = new(
                    ColorDebug(data.bottles[i].liquids[j].colorId),
                    data.bottles[i].liquids[j].isMystery
                    );
                bottleList[i].liquidUnits.Add(pendingLiquid);


            }

        }
    }

    private LiquidColor ColorDebug(int color) {
        if (colorTranslate.TryGetValue(color, out LiquidColor result)) {
            return result;
        } else {
            throw new Exception("Critical Error for Color Decode");
        }
    }

}
