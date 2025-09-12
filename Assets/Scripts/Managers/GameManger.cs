using UnityEngine;

public class GameManger : MonoBehaviour
{
    public HandManager handManager;
    public DeckManager deckManager;

    void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            drawCard();
        }
    }

    public void drawCard()
    {
        CardData card = deckManager.DrawCard();
        if (card != null)
        {
            handManager.AddCardToHand(card);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
