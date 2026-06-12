using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    public int coinSetForAdmin = 9000;

    public int coins;
    public int level;

    //Rewards
    public int coinsReward;
    public int shufflesReward;
    public int addBottlesReward;
    public int undosReward;

    //Boosters
    public int shuffle;
    public int addBottle;
    public int undo;

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
        coinsReward = PlayerPrefs.GetInt("CoinsReward"); //STC


        shuffle = PlayerPrefs.GetInt("Shuffle");
        addBottle = PlayerPrefs.GetInt("Add");
        undo = PlayerPrefs.GetInt("Undo");
    }

    private void FirstTime() {
        coins = coinSetForAdmin;
        level = 0;

        //Rewards
        coinsReward = 60;
        shufflesReward = 2;
        addBottlesReward = 2;

        //Boosters
        shuffle = 5;
        addBottle = 5;
        undo = 5;
    }

    public void SaveData() {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("Level", level);
        PlayerPrefs.SetInt("CoinsReward", coinsReward);

        PlayerPrefs.SetInt("Shuffle", shuffle);
        PlayerPrefs.SetInt("Add", addBottle);
        PlayerPrefs.SetInt("Undo", undo);

        PlayerPrefs.SetInt("FirstLaunch", 1);

        PlayerPrefs.Save();
    }

    void OnApplicationQuit() {
        SaveData();
    }

    void OnApplicationPause(bool a) {
        if (a) SaveData();
    }
}
