using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelCreator : MonoBehaviour {
    [Header("References")]
    [SerializeField] private AdminUIHandler adminui;
    [SerializeField] private UIHandler ui;
    [SerializeField] private BottleGen bottleGen;
    [SerializeField] private LevelTranslator translator;

    private LevelData levelData;
    private Dictionary<int, LiquidColor> colorTranslate;

    private string LevelResourcePath(int level) {
        return "Levels/level_" + level.ToString("D2");
    }

    private string PlayerSavePath {
        get {
            return Path.Combine(
                Application.persistentDataPath,
                "save.json"
            );
        }
    }

    public void DataProcess(bool layout = false) {
        levelData = BuildCurrentLevelData(layout);

        if (levelData == null)
            return;

        if (layout)
            SaveLayout();
        else
            SaveLevel();
    }

    private LevelData BuildCurrentLevelData(bool layout) {
        int levelNumber;
        int coins;
        int shuffle;
        int undo;
        int addBottle;

        if (layout) {
            levelNumber = SaveManager.Instance.level;
            coins = SaveManager.Instance.coinsReward;
            shuffle = SaveManager.Instance.shufflesReward;
            undo = SaveManager.Instance.undosReward;
            addBottle = SaveManager.Instance.addBottlesReward;
        } else {
            if (!int.TryParse(adminui.levelInput.text, out levelNumber))
                return null;

            if (!int.TryParse(adminui.coinInput.text, out coins))
                coins = 0;

            if (!int.TryParse(adminui.shuffleInput.text, out shuffle))
                shuffle = 0;

            if (!int.TryParse(adminui.undoInput.text, out undo))
                undo = 0;

            if (!int.TryParse(adminui.addBottleInput.text, out addBottle))
                addBottle = 0;
        }

        LevelData data = new LevelData();

        data.levelNumber = levelNumber;

        data.rewards.coins = coins;
        data.rewards.shuffle = shuffle;
        data.rewards.undo = undo;
        data.rewards.addBottle = addBottle;

        List<Bottle> bottles =
            bottleGen.DictionaryToSingularBottleConverter();

        data.bottleCount = bottles.Count;

        foreach (Bottle bottle in bottles) {
            BottleData bottleData = new BottleData {
                isLocked = bottle.isLocked,
                lockCondition = translator.TranslatedColor(bottle.lockColor)
            };

            foreach (LiquidUnit unit in bottle.liquidUnits) {
                LiquidData liquidData = new LiquidData {
                    colorId = translator.TranslatedColor(unit.colorId),
                    isMystery = unit.isMystery
                };

                bottleData.liquids.Add(liquidData);
            }

            data.bottles.Add(bottleData);
        }

        return data;
    }

    private void SaveLevel() {
    #if UNITY_EDITOR
        string path =
            Path.Combine(
                Application.dataPath,
                "Resources/Levels/level_" +
                levelData.levelNumber.ToString("D2") +
                ".json"
            );

        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(path, json);

        UnityEditor.AssetDatabase.Refresh();

        Debug.Log("Editor level saved: " + path);
    #else
            Debug.LogWarning("SaveLevel is editor-only. Runtime should use SaveLayout.");
    #endif
    }

    private void SaveLayout() {
        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(PlayerSavePath, json);

        Debug.Log("Player layout saved: " + PlayerSavePath);
    }

    public void CheckForSafeSave() {
        LevelData currentData = BuildCurrentLevelData(true);

        if (currentData == null)
            return;

        if (IsLevelCompleted(currentData)) {
            DeleteSavedLayout();
            return;
        }

        levelData = currentData;
        SaveLayout();
    }

    public void DeleteSavedLayout() {
        if (File.Exists(PlayerSavePath))
            File.Delete(PlayerSavePath);
    }

    public void LoadLevel(bool randomize = false, bool launch = false) {
        if (adminui.admin) {
            SaveManager.Instance.level =
                int.TryParse(adminui.levelInput.text, out int adminLevel)
                    ? adminLevel
                    : 0;

            LoadDefaultLevel(SaveManager.Instance.level, randomize);
            return;
        }

        if (launch && TryLoadSavedLayout())
            return;

        LoadDefaultLevel(SaveManager.Instance.level, randomize);
    }

    private bool TryLoadSavedLayout() {
        if (!File.Exists(PlayerSavePath))
            return false;

        string json = File.ReadAllText(PlayerSavePath);
        LevelData savedData = JsonUtility.FromJson<LevelData>(json);

        if (savedData == null)
            return false;

        if (savedData.levelNumber != SaveManager.Instance.level)
            return false;

        if (IsLevelCompleted(savedData)) {
            DeleteSavedLayout();
            SaveManager.Instance.level += 1;
            return false;
        }

        LoadData(savedData, false);
        return true;
    }

    private void LoadDefaultLevel(int level, bool randomize) {
        TextAsset file =
            Resources.Load<TextAsset>(
                LevelResourcePath(level)
            );

        if (file == null) {
            Debug.LogWarning(
                "Level file not found: " +
                LevelResourcePath(level)
            );
            return;
        }

        LevelData data =
            JsonUtility.FromJson<LevelData>(file.text);

        LoadData(data, randomize);
    }

    private void LoadData(LevelData data, bool randomize) {
        PrepareColorTranslation(randomize);

        SaveManager.Instance.level = data.levelNumber;

        SaveManager.Instance.coinsReward = data.rewards.coins;
        SaveManager.Instance.shufflesReward = data.rewards.shuffle;
        SaveManager.Instance.undosReward = data.rewards.undo;
        SaveManager.Instance.addBottlesReward = data.rewards.addBottle;

        bottleGen.GenAmount(data.bottleCount);

        List<Bottle> bottleList =
            bottleGen.DictionaryToSingularBottleConverter();

        for (int i = 0; i < data.bottleCount; i++) {
            Bottle bottle = bottleList[i];
            BottleData bottleData = data.bottles[i];

            bottle.liquidUnits.Clear();

            if (bottleData.isLocked) {
                bottle.SetLocker(
                    ColorDebug(bottleData.lockCondition),
                    true
                );
            }

            for (int j = 0; j < bottleData.liquids.Count; j++) {
                LiquidData liquidData =
                    bottleData.liquids[j];

                LiquidUnit unit = new LiquidUnit(
                    ColorDebug(liquidData.colorId),
                    liquidData.isMystery
                );

                bottle.liquidUnits.Add(unit);
            }

            bottle.RefreshView();
            bottle.CheckCompleteOnLoad();
        }

        Debug.Log("Level Loaded: " + data.levelNumber);

        adminui.SetLevelnReward();
        ui.BaseUpd();
    }

    private void PrepareColorTranslation(bool randomize) {
        if (randomize) {
            colorTranslate = translator.Randomizer();
            return;
        }

        colorTranslate = new Dictionary<int, LiquidColor>();

        LiquidColor[] colors =
            (LiquidColor[])Enum.GetValues(typeof(LiquidColor));

        for (int i = 0; i < colors.Length; i++) {
            colorTranslate[i] = colors[i];
        }
    }

    private LiquidColor ColorDebug(int color) {
        if (colorTranslate.TryGetValue(
            color,
            out LiquidColor result)) {
            return result;
        }

        throw new Exception(
            "Critical Error for Color Decode: " + color
        );
    }

    private bool IsLevelCompleted(LevelData data) {
        foreach (BottleData bottle in data.bottles) {
            if (bottle.liquids.Count == 0)
                continue;

            if (bottle.liquids.Count < 4)
                return false;

            int color = bottle.liquids[0].colorId;

            foreach (LiquidData liquid in bottle.liquids) {
                if (liquid.colorId != color)
                    return false;
            }
        }

        return true;
    }
}