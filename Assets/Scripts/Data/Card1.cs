using UnityEngine;

[System.Serializable]
public class RoleAbility
{
    public string abilityName;      // e.g., "Stop the Assassin"
    public AbilityType type;        // Block, Reveal, Steal, etc.
    public int value;               // Coins, targets, etc.
    public string target;           // "Player", "All", "Self"
}

public enum AbilityType
{
    BlockAssassination,     // General
    PredictRole,            // General
    PeekOtherCard,          // National Guard
    StealCoins,             // Thief
    TaxAllPlayers,          // Fiscalité
    SwapCards,           // Deputy
    Assassinate,             // Assassin
    EliminateCard,           //ALL
    GetCoins    //Boss
}
[System.Serializable]
public class CardDefiner
{
    public int cardId;
    public string cardName;
    public string description;
    public Sprite artwork;
    public string possibleActions;
    [Header("Abilities")]
    public RoleAbility[] abilities = new RoleAbility[1];  // 1 main ability per role
    internal bool canBeChallenged;

    public CardDefiner()
    {

    }
    public CardDefiner(int id ,string cardName, string description, Sprite artwork, string possibleActions, bool canBeChallenged)
    {
        this.cardId = id;
        this.cardName = cardName;
        this.description = description;
        this.artwork = artwork;
        this.possibleActions = possibleActions;
        this.canBeChallenged = canBeChallenged;

    }

}

