using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using Application = UnityEngine.Application;

public class LevelCreator : MonoBehaviour
{
    public AdminUIHandler ui;
    public BottleGen bottleGen;
    public LevelTranslator translator;
    public GameManager gameManager;

    private LevelData levelData;
    private Dictionary<int, LiquidColor> colorTranslate;

    private string levelPath = Application.dataPath + "/LevelManager/Levels/level_";
    private string personalPath = Application.dataPath + "/Data/";

    private void SaveLevel(int result) {
        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(levelPath + result.ToString("D2"), json);
        Debug.Log("Saved");
    }

    private void SaveLayout() {
        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(personalPath + "layout", json);
    }

    public void DataProcess(bool layout = false) {
        int result;
        int reward;
        int shuffle;
        int undo;
        int addBottle;

        if (!layout) {
            if (!int.TryParse(ui.levelInput.text, out result)) return;
            if (!int.TryParse(ui.coinInput.text, out reward)) reward = 0;
            
            if (!int.TryParse(ui.shuffleInput.text, out shuffle)) shuffle = 0;
            if (!int.TryParse(ui.undoInput.text, out undo)) undo = 0;
            if (!int.TryParse(ui.addBottleInput.text, out addBottle)) addBottle = 0;
        } else {
            result = SaveManager.Instance.level;
            reward = SaveManager.Instance.coinsReward;

            shuffle = SaveManager.Instance.shufflesReward;
            undo = SaveManager.Instance.undosReward;
            addBottle = SaveManager.Instance.addBottlesReward;
        }
        

        levelData = new();
        List<Bottle> currentBottleData = bottleGen.DictionaryToSingularBottleConverter();


        levelData.levelNumber = result;
        levelData.rewards.coins = reward;

        levelData.rewards.shuffle = shuffle;
        levelData.rewards.undo = undo;
        levelData.rewards.addBottle = addBottle;


        levelData.bottleCount = currentBottleData.Count;
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
        if (!layout) SaveLevel(result);
        else SaveLayout();
    }



    //LEVEL LOADING



    public void LoadLevel(bool randomize = false, bool next = false, bool launch = false) {
        if (ui.admin) {
            SaveManager.Instance.level = int.TryParse(ui.levelInput.text, out int result) ? result : 0;
        } else {
            SaveManager.Instance.level = next ? SaveManager.Instance.level+=1 : SaveManager.Instance.level;
        }

        string json;

        if (!launch) {
            if (!File.Exists(levelPath + SaveManager.Instance.level.ToString("D2"))) return;
            json = File.ReadAllText(levelPath + SaveManager.Instance.level.ToString("D2"));
        } else {
            if (!File.Exists(personalPath + "layout")) return;
            json = File.ReadAllText(personalPath + "layout");
        }

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

        SaveManager.Instance.level = data.levelNumber;
        SaveManager.Instance.coinsReward = data.rewards.coins;

        SaveManager.Instance.shufflesReward = data.rewards.shuffle;
        SaveManager.Instance.undosReward = data.rewards.undo;
        SaveManager.Instance.addBottlesReward = data.rewards.addBottle;

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
        Debug.Log("Level Loaded");
        ui.SetLevelnReward();
    }

    

    private LiquidColor ColorDebug(int color) {
        if (colorTranslate.TryGetValue(color, out LiquidColor result)) {
            return result;
        } else {
            throw new Exception("Critical Error for Color Decode");
        }
    }

}
