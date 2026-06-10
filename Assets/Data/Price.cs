using UnityEngine;
[System.Serializable]
[CreateAssetMenu(fileName = "Price", menuName = "Scriptable Objects/Price")]
public class Price : ScriptableObject
{
    public int bottlePrice = 900;
    public int shufflePrice = 300;
    public int undoPrice = 500;
}
