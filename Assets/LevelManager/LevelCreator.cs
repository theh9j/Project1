using UnityEngine;

public class LevelCreator : MonoBehaviour
{
    public UIHandler ui;
    public BottleGen bottleGen;
    public LevelData levelData = new LevelData();

    public void SaveLevel() {
        string json = JsonUtility.ToJson(levelData, true);
        Debug.Log(json);
    }

    private void DataProcess() {
        if (!int.TryParse(ui.LevelInput, out int result)) return;



        levelData.levelNumber = result;
        //levelData.levelTypeCount
        //levelDAta.levelTypeIndex

        levelData.bottleCount = bottleGen.bottles.Count;
        foreach (Bottle bottle in bottleGen.bottles) {

        }
        
    }
}
