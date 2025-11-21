using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Deck Setup")]
    public List<CardDefiner> cards = new List<CardDefiner>();  // Drag 30+ cards here!

    [Header("Player Deck")]
    public List<CardDefiner> cardsInDeck = new List<CardDefiner>();

    [Header("Auto Deck Settings")]
    public int cardsPerRole = 3;           // How many copies of each role (Coup standard = 3)
    public int totalRolesInDeck = 15;       // 5 roles × 3 copies = 15 cards

    [Header("References")]
    public GameObject Hand;  // HandPanel
    public GameObject CardToHand;  // Card prefab

    public int deckSize;
    public static List<CardDefiner> staticDeckplayerStaticDeck;
    public List<CardDefiner> fullDeck = new List<CardDefiner>();   // Full 15-card deck

    void Start()
    {
        SetupDeck();   // NEW: Populate + Shuffle
        StartCoroutine(StartGame());
    }

    private void Awake()
    {
        Instance = this;
        BuildFullDeckAutomatically();   // ← AUTOMATIC!
        ShuffleAndPrepareDrawPile();
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
        staticDeckplayerStaticDeck = new List<CardDefiner>(cardsInDeck);

        Debug.Log($"✅ Deck shuffled! {deckSize} cards ready.");
    }

    void BuildFullDeckAutomatically()
    {
        fullDeck.Clear();

        // Loop through ALL roles in CardDatabase (skip ID 0 = placeholder)
        for (int i = 1; i < CardDatabase.cardList.Count; i++)
        {
            CardDefiner role = CardDatabase.cardList[i];

            // Add X copies of this role
            for (int copy = 0; copy < cardsPerRole; copy++)
            {
                fullDeck.Add(role);
            }
        }

        Debug.Log($"Deck built automatically: {fullDeck.Count} cards (should be {totalRolesInDeck})");
    }

    void ShuffleAndPrepareDrawPile()
    {
        cardsInDeck = new List<CardDefiner>(fullDeck);

        // Fisher-Yates shuffle
        for (int i = cardsInDeck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CardDefiner temp = cardsInDeck[i];
            cardsInDeck[i] = cardsInDeck[j];
            cardsInDeck[j] = temp;
        }

        Debug.Log($"Deck shuffled! {cardsInDeck.Count} cards ready to draw.");
    }

    // Optional: Rebuild deck anytime
    [ContextMenu("Rebuild & Shuffle Deck")]
    public void RebuildDeck()
    {
        BuildFullDeckAutomatically();
        ShuffleAndPrepareDrawPile();
    }

    public void SchuffleCards()
    {
        for (int j = 0; j < cardsInDeck.Count; j++)
        {
            CardDefiner temp = cardsInDeck[j];
            int randomIndex = Random.Range(j, cardsInDeck.Count);
            cardsInDeck[j] = cardsInDeck[randomIndex];
            cardsInDeck[randomIndex] = temp;
        }
    }

    public void DrawToHand(bool isPlayer)
    {
        if (cardsInDeck.Count == 0) return;

        CardDefiner cardDef = cardsInDeck[0];
        cardsInDeck.RemoveAt(0);

        Transform parent = isPlayer
            ? HandManager.Instance.playerHandTransform
            : HandManager.Instance.opponentHandTransform;

        GameObject cardObj = Instantiate(CardToHand, parent);

        cardObj.GetComponent<CardDisplay>().Setup(cardDef);
        cardObj.GetComponent<CardDisplay>().SetFaceUp(isPlayer); // opponent = hidden

        HandManager.Instance.AddCardToHand(cardObj, isPlayer);
    }

    public IEnumerator StartGame()
    {
        // Player hand
        for (int i = 0; i < 5; i++)
        {
            DrawToHand(true);  // true = player
            yield return new WaitForSeconds(0.6f);
        }

        yield return new WaitForSeconds(1f);

        // OPPONENT HAND – NOW WORKS!
        for (int i = 0; i < 5; i++)
        {
            DrawToHand(false);  // false = opponent
            yield return new WaitForSeconds(0.4f);
        }
    }
    IEnumerator AnimateDraw(GameObject card, Transform handParent, bool isPlayer)
    {
        RectTransform rt = card.GetComponent<RectTransform>();
        Vector3 startPos = rt.localPosition;

        // Calculate final position in hand
        Vector3 finalPos = HandManager.Instance.GetHandPosition(isPlayer);

        float duration = 0.5f;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            rt.localPosition = Vector3.Lerp(startPos, finalPos, t / duration);
            yield return null;
        }

        rt.localPosition = finalPos;

        // Flip face up (reveal)
        yield return new WaitForSeconds(0.1f);
        card.GetComponent<CardDisplay>().SetFaceUp(true);
    }
}