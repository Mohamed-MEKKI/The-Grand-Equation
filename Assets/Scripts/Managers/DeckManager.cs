using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    public GameObject CardToHand;           // Your card prefab
    public List<CardDefiner> cardsInDeck = new List<CardDefiner>();

    [Header("Deck Settings")]
    public int copiesPerRole = 3;

    [Header("Hand Size")]
    public int startingHandSize = 3;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        BuildAndShuffleDeck(); 
    }

    void BuildAndShuffleDeck()
    {
        cardsInDeck.Clear();

        // Build full deck (skip ID 0)
        for (int i = 1; i < CardDatabase.cardList.Count; i++)
        {
            for (int c = 0; c < copiesPerRole; c++)
            {
                cardsInDeck.Add(CardDatabase.cardList[i]);
            }
        }

        // Shuffle
        for (int i = cardsInDeck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = cardsInDeck[i];
            cardsInDeck[i] = cardsInDeck[j];
            cardsInDeck[j] = temp;
        }

        Debug.Log($"DECK READY: {cardsInDeck.Count} cards");
    }

    public void DrawToHand(bool isPlayer)
    {
        if (cardsInDeck.Count == 0)
        {
            Debug.Log("DECK EMPTY!");
            return;
        }

        CardDefiner cardDef = cardsInDeck[0];
        cardsInDeck.RemoveAt(0);

        Transform parent = isPlayer
            ? HandManager.Instance.playerHandTransform
            : HandManager.Instance.opponentHandTransform;

        GameObject cardObj = Instantiate(CardToHand, parent);
        cardObj.GetComponent<CardDisplay>().Setup(cardDef);
        cardObj.GetComponent<CardDisplay>().SetFaceUp(isPlayer);

        HandManager.Instance.AddCardToHand(cardObj, isPlayer);

        Debug.Log($"Drew card to {(isPlayer ? "PLAYER" : "OPPONENT")} hand. Deck left: {cardsInDeck.Count}");
    }

    // CALL THIS ONCE AT GAME START
    [ContextMenu("Deal Starting Hands")]
    public void DealStartingHands()
    {
        StartCoroutine(DealHandsCoroutine());
    }

    public IEnumerator DealHandsCoroutine()
    {
        HandManager.Instance.ClearHands();  // ← CRITICAL

        for (int i = 0; i < startingHandSize; i++)
        {
            DrawToHand(true);
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < startingHandSize; i++)
        {
            DrawToHand(false);   // ← OPPONENT
            yield return new WaitForSeconds(0.4f);
        }
    }
    public void StartGame()
    {
        DealStartingHands();   // just forwards to the real method
    }
}