using UnityEngine;

[CreateAssetMenu(fileName = "SoloLevel", menuName = "Solo/Level Data")]
public class SoloLevelData : ScriptableObject
{
    public int levelNumber;
    public string name;                 // e.g. "The First Bluff"
    public string description;          // Shown in-game
    public int aiAggression;            // 0-100
    public float aiChallengeChance;     // 0.1f-0.9f
    public int playerStartingCoins;     // Handicap: 3-7
    public Sprite backgroundImage;      // Level art
}