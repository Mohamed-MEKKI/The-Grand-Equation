using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DeckManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<CardData> cards = new List<CardData>();
    public List<CardData> cardsInDeck;
    public int deckSize;
    public static List<CardData> staticDeckplayerStaticDeck;
    

    public GameObject Hand;
    public GameObject CardToHand;
    public GameObject[] clones;
    
    void Start()
    {
        SchuffleCards();
        //StartRoutine(StartGame());

    }

    public void SchuffleCards()
    {
        cardsInDeck = new List<CardData>(cards);
        for (int j = 0; j < cards.Count; j++)
        {
            CardData cup = cardsInDeck[j];
            int randomOrder = Random.Range(0, cards.Count);
            cardsInDeck[j] = cardsInDeck[randomOrder];
            cardsInDeck[randomOrder] = cup;

        }
    }

    IEnumerator StartGame()
    {
        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(1);
            Instantiate(CardToHand, transform.position, transform.rotation);
        }
    }

    public void StartRoutine(GameObject hand)
    {

    }
    
    public CardData DrawCard()
    {
        if (cards.Count <= 0) return null;
 
        CardData card = cardsInDeck[0];
        cardsInDeck.RemoveAt(0);

        return card;
    }

}
