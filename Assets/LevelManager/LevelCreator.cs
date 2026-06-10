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
        if (!int.TryParse(ui.levelInput.text, out int result)) return;
        if (!int.TryParse(ui.rewardInput.text, out int reward)) reward = 0;
        levelData = new();
        List<Bottle> currentBottleData = bottleGen.DictionaryToSingularBottleConverter();


        levelData.levelNumber = result;
        levelData.rewards = reward;
        levelData.bottleCount = currentBottleData.Count;
        Debug.Log(currentBottleData.Count);
        for (int i = 0; i < currentBottleData.Count; i++) {

            BottleData bottleData = new() {
                isLocked = currentBottleData[i].isLocked,
                lockCondition = translator.TranslatedColor(currentBottleData[i].lockColor)
            };

            for (int j = 0; j < currentBottleData[i].liquidUnits.Count; j++) {

                LiquidData liquidData = new() {
                    colorId = translator.TranslatedColor(currentBottleData[i].liquidUnits[j].colorId),
                    isMystery = currentBottleData[i].liquidUnits[j].isMystery
                };
                bottleData.liquids.Add(liquidData);

            }
            levelData.bottles.Add(bottleData);
        }

        SaveLevel(result);
    }

    public void LoadLevel(bool randomize = false) {
        int result;
        if (ui.admin) {
            if (!int.TryParse(ui.levelInput.text, out result)) {
                return;
            }
        } else {
            result = PlayerPrefs.GetInt("Level")+1;
        }
        
        if (!File.Exists(path + result.ToString("D2"))) return;

        string json = File.ReadAllText(path + result.ToString("D2"));
        LevelData data = JsonUtility.FromJson<LevelData>(json);

        LoadData(data, randomize);
    }

    private void LoadData(LevelData data, bool randomize) {
        if (randomize) {
            colorTranslate = translator.Randomizer();
        } else {
            colorTranslate = new();
            for (int i = 0; i < Enum.GetValues(typeof(LiquidColor)).Length; i++) {
                colorTranslate[i] = ((LiquidColor[])Enum.GetValues(typeof(LiquidColor)))[i];
            }
        }

        PlayerPrefs.SetInt("Level", data.levelNumber);
        PlayerPrefs.SetInt("Reward", data.rewards);
        bottleGen.GenAmount(data.bottleCount);

        List<Bottle> bottleList = bottleGen.DictionaryToSingularBottleConverter();

        for (int i = 0; i < data.bottleCount; i++) {

            if (data.bottles[i].isLocked) {
                bottleList[i].SetLocker(ColorDebug(data.bottles[i].lockCondition), true);
            } 
            

            for (int j = 0; j < data.bottles[i].liquids.Count; j++) {

                LiquidUnit pendingLiquid = new(
                    ColorDebug(data.bottles[i].liquids[j].colorId),
                    data.bottles[i].liquids[j].isMystery
                    );
                bottleList[i].liquidUnits.Add(pendingLiquid);
            }

        }
        PlayerPrefs.Save();
    }

    private LiquidColor ColorDebug(int color) {
        if (colorTranslate.TryGetValue(color, out LiquidColor result)) {
            return result;
        } else {
            throw new Exception("Critical Error for Color Decode");
        }
    }

}
