using UnityEngine;
[System.Serializable]
public class CardDefiner
{
    public int cardId;
    public string cardName;
    public string description;
    public Sprite artwork;
    public string possibleActions;

    public CardDefiner()
    {

    }
    public CardDefiner(int id ,string cardName, string description, Sprite artwork, string possibleActions)
    {
        this.cardId = id;
        this.cardName = cardName;
        this.description = description;
        this.artwork = artwork;
        this.possibleActions = possibleActions;

    }

}

