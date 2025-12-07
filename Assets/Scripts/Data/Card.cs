using UnityEngine;

[CreateAssetMenu(fileName ="New Card", menuName ="Card")]
public class CardData : ScriptableObject
{
    public int cardID;
    public string cardName;
    public string description;
    public Sprite artwork;
    public string possibleActions;
    public bool canBeChallenged;

}

