using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    [SerializeField] private LevelCreator levelSave;

    public int coinSetForAdmin = 9000;
    public int startLevel = 0;

    [HideInInspector] public int coins;
    [HideInInspector] public int level;

    //Rewards
    [HideInInspector] public int coinsReward;
    [HideInInspector] public int shufflesReward;
    [HideInInspector] public int addBottlesReward;
    [HideInInspector] public int undosReward;

    //Boosters
    [HideInInspector] public int shuffle;
    [HideInInspector] public int addBottle;
    [HideInInspector] public int undo;

    [HideInInspector] public bool music;
    [HideInInspector] public bool sfx;

    void Awake() {
        if (Instance != null) {
            Destroy(Instance);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadData();
    }

    public void LoadData() {
        PlayerPrefs.DeleteKey("FirstLaunch"); //For debug


        if (!PlayerPrefs.HasKey("FirstLaunch")) { FirstTime(); return; }

        coins = PlayerPrefs.GetInt("Coins"); //Base amount for first time playing
        level = PlayerPrefs.GetInt("Level");
        coinsReward = PlayerPrefs.GetInt("CoinsReward");


        shuffle = PlayerPrefs.GetInt("Shuffle");
        addBottle = PlayerPrefs.GetInt("Add");
        undo = PlayerPrefs.GetInt("Undo");

        //SETTINGS
        music = PlayerPrefs.GetInt("Music") != 0;
        sfx = PlayerPrefs.GetInt("SFX") != 0;
    }

    private void FirstTime() {
        coins = coinSetForAdmin;
        level = startLevel;

        //Rewards
        coinsReward = 60;
        shufflesReward = 2;
        addBottlesReward = 2;

        //Boosters
        shuffle = 5;
        addBottle = 5;
        undo = 5;

        //Settings
        music = true;
        sfx = true;
    }

    public void SaveData() {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("Level", level);
        PlayerPrefs.SetInt("CoinsReward", coinsReward);

        PlayerPrefs.SetInt("Shuffle", shuffle);
        PlayerPrefs.SetInt("Add", addBottle);
        PlayerPrefs.SetInt("Undo", undo);

        PlayerPrefs.SetInt("Music", music ? 1 : 0);
        PlayerPrefs.SetInt("SFX", sfx ? 1 : 0);

        PlayerPrefs.SetInt("FirstLaunch", 1);

        PlayerPrefs.Save();
        levelSave.DataProcess(true);
    }

    void OnApplicationQuit() {
        SaveData();
    }

    void OnApplicationPause(bool a) {
        if (a) SaveData();
    }
}
