using UnityEngine;

public class SaveManager : MonoBehaviour {
    public static SaveManager Instance;

    [SerializeField] private LevelCreator levelSave;

    public int coinSetForAdmin = 9000;
    public int startLevel = 0;
    public bool resetSaveOnStart = false;

    public int coins;
    public int level;

    public int coinsReward;
    public int shufflesReward;
    public int addBottlesReward;
    public int undosReward;

    public int shuffle;
    public int addBottle;
    public int undo;

    public bool music;
    public bool sfx;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadData();
    }

    public void LoadData() {
        if (resetSaveOnStart) {
            PlayerPrefs.DeleteAll();
        }

        if (!PlayerPrefs.HasKey("HasSave")) {
            FirstTime();
            return;
        }

        coins = PlayerPrefs.GetInt("Coins", coinSetForAdmin);
        level = PlayerPrefs.GetInt("Level", startLevel);

        coinsReward = PlayerPrefs.GetInt("CoinsReward", 60);
        shufflesReward = PlayerPrefs.GetInt("ShufflesReward", 0);
        undosReward = PlayerPrefs.GetInt("UndosReward", 0);
        addBottlesReward = PlayerPrefs.GetInt("AddBottlesReward", 20);

        shuffle = PlayerPrefs.GetInt("Shuffle", 5);
        addBottle = PlayerPrefs.GetInt("AddBottle", 5);
        undo = PlayerPrefs.GetInt("Undo", 5);

        music = PlayerPrefs.GetInt("Music", 1) != 0;
        sfx = PlayerPrefs.GetInt("SFX", 1) != 0;
    }

    private void FirstTime() {
        coins = coinSetForAdmin;
        level = startLevel;

        coinsReward = 60;
        shufflesReward = 2;
        undosReward = 2;
        addBottlesReward = 2;

        shuffle = 5;
        addBottle = 5;
        undo = 5;

        music = true;
        sfx = true;

        SaveData(false);
    }

    public void SaveData(bool saveLayout = true) {
        if (saveLayout && levelSave != null)
            levelSave.CheckForSafeSave();

        PlayerPrefs.SetInt("HasSave", 1);

        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("Level", level);

        PlayerPrefs.SetInt("Shuffle", shuffle);
        PlayerPrefs.SetInt("AddBottle", addBottle);
        PlayerPrefs.SetInt("Undo", undo);

        PlayerPrefs.SetInt("Music", music ? 1 : 0);
        PlayerPrefs.SetInt("SFX", sfx ? 1 : 0);

        PlayerPrefs.Save();
    }

    private void OnApplicationQuit() {
        SaveData();
    }

    private void OnApplicationPause(bool paused) {
        if (paused)
            SaveData();
    }
}