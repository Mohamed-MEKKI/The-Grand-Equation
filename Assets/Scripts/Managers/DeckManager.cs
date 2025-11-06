using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DeckManager : MonoBehaviour
{
    public List<CardData> cards = new List<CardData>();
    public List<CardData> cardsInDeck;
    public int deckSize;
    public static List<CardData> staticDeckplayerStaticDeck;

    public GameObject Hand;  // Hand panel
    public GameObject CardToHand;  // Your card prefab
    public GameObject[] clones;

    void Start()
    {
        SchuffleCards();
        StartCoroutine(StartGame());  // Fixed: Use coroutine directly
    }

    public void SchuffleCards()
    {
        cardsInDeck = new List<CardData>(cards);
        for (int j = 0; j < cardsInDeck.Count; j++)  // Fixed: cardsInDeck.Count
        {
            CardData cup = cardsInDeck[j];
            int randomOrder = Random.Range(j, cardsInDeck.Count);  // Fixed: Fisher-Yates shuffle
            cardsInDeck[j] = cardsInDeck[randomOrder];
            cardsInDeck[randomOrder] = cup;
        }
    }

    // Fixed: Now draws AND instantiates with flip animation
    public void DrawToHand()
    {
        CardData cardData = DrawCard();
        if (cardData == null) return;

        // 1. Instantiate FACE DOWN at deck position
        GameObject newCard = Instantiate(CardToHand, transform.position, Quaternion.identity, Hand.transform);
        newCard.name = cardData.cardName;  // Nice-to-have

        // 2. Get components (your CardView + new CardFlip)
        var cardView = newCard.GetComponent<CardDisplay>();  // Your class
        //var cardFlip = newCard.GetComponent<CardFlip>();

        // 3. Start FACE DOWN
        //cardView.SetCard(cardData);  // Or whatever your method is
        //cardView.SetFaceUp(false);   // Show back

        // 4. Animate: Fly to hand position → Flip face up
        StartCoroutine(AnimateDraw(newCard, cardData));

        //GameObject newCard = Instantiate(CardToHand, transform.position, Quaternion.identity, Hand.transform);

        var display = newCard.GetComponent<CardDisplay>();
        display.deckManager = this;               // <-- give it a reference
        display.Setup(cardData);                  // <-- sets data + shows back

        // … fly animation …

        // AFTER the card lands in the hand:
        display.FlipTo(true);                     // <-- animated flip to front
        //HandManager.Instance.AddCardToHand(newCard);
    }

    IEnumerator AnimateDraw(GameObject cardObj, CardData cardData)
    {
        // Fly from deck to hand (0.5 sec)
        Vector3 startPos = cardObj.transform.localPosition;
        Vector3 handPos = GetHandPosition();  // Spread cards in hand
        float flyTime = 0.5f;
        float elapsed = 0;

        while (elapsed < flyTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flyTime;
            cardObj.transform.localPosition = Vector3.Lerp(startPos, handPos, t);
            yield return null;
        }

        // Land in hand → FLIP!
        cardObj.transform.localPosition = handPos;
        yield return new WaitForSeconds(0.1f);  // Tiny pause
        //cardFlip.FlipToFaceUp(cardData);  // Reveals the card
    }

    Vector3 GetHandPosition()
    {
        // Spread cards across hand (adjust based on your Hand panel size)
        int cardCount = Hand.transform.childCount;
        float handWidth = 800f;  // Your Hand rect width
        float cardWidth = 120f;  // Your card width
        float startX = -(cardCount * cardWidth * 0.7f) / 2;
        return new Vector3(startX + cardCount * cardWidth * 0.7f, 0, 0);
    }

    // Your original DrawCard (for other uses)
    public CardData DrawCard()
    {
        if (cardsInDeck.Count <= 0) return null;  // Fixed
        CardData card = cardsInDeck[0];
        cardsInDeck.RemoveAt(0);
        return card;
    }

    // Updated StartGame: Draw 5 cards with animation
    IEnumerator StartGame()
    {
        for (int i = 0; i < 5; i++)  // Standard hand size
        {
            DrawToHand();  // Uses new animated draw
            yield return new WaitForSeconds(0.6f);  // Staggered draw
        }
    }
}