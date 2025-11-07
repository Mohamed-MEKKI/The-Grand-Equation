using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DeckManager : MonoBehaviour
{
    [Header("Deck Setup")]
    public List<CardData> cards = new List<CardData>();  // Drag 30+ cards here!

    [Header("Player Deck")]
    public List<CardData> cardsInDeck = new List<CardData>();

    [Header("References")]
    public GameObject Hand;  // HandPanel
    public GameObject CardToHand;  // Card prefab

    public int deckSize;
    public static List<CardData> staticDeckplayerStaticDeck;

    void Start()
    {
        SetupDeck();   // NEW: Populate + Shuffle
        StartCoroutine(StartGame());
    }


    void SetupDeck()
    {
        if (cards.Count == 0)
        {
            Debug.LogError("🚫 DeckManager: Add cards to 'Cards' list in Inspector!");
            return;
        }

        cardsInDeck.Clear();
        cardsInDeck.AddRange(cards);  // Copy all cards
        SchuffleCards();

        deckSize = cardsInDeck.Count;
        staticDeckplayerStaticDeck = new List<CardData>(cardsInDeck);

        Debug.Log($"✅ Deck shuffled! {deckSize} cards ready.");
    }

    public void SchuffleCards()
    {
        for (int j = 0; j < cardsInDeck.Count; j++)
        {
            CardData temp = cardsInDeck[j];
            int randomIndex = Random.Range(j, cardsInDeck.Count);
            cardsInDeck[j] = cardsInDeck[randomIndex];
            cardsInDeck[randomIndex] = temp;
        }
    }
    public IEnumerator StartGame()
    {
        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(1);
            Instantiate(CardToHand, transform.position, transform.rotation);
        }
    }
}