using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [Header("Player Hand")]
    public Transform playerHandTransform;     // Your "HandPanel"

    [Header("Opponent Hand")]
    public Transform opponentHandTransform;   // NEW: Drag opponent panel here
    public GameObject opponentHandPanel;      // Prefab or existing panel

    [Header("Settings")]
    public float cardSpacing = 120f;
    public float opponentYOffset = -200f;     // Top of screen offset

    private List<GameObject> playerHandCards = new List<GameObject>();
    private List<GameObject> opponentHandCards = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    // Player adds card
    public void AddCardToHand(GameObject cardObj, bool isPlayer = true)
    {
        if (isPlayer)
        {
            playerHandCards.Add(cardObj);
            cardObj.transform.SetParent(playerHandTransform);
            ArrangeHand(true);
        }
        else
        {
            opponentHandCards.Add(cardObj);
            cardObj.transform.SetParent(opponentHandTransform);
            ArrangeHand(false);
        }

        // Add hover/drag (only for player)
        if (isPlayer)
        {
            var hover = cardObj.AddComponent<CardHover>();
            var drag = cardObj.AddComponent<CardDragHandler>();
        }
    }

    public void RemoveCardFromHand(GameObject cardObj, bool isPlayer = true)
    {
        if (isPlayer) playerHandCards.Remove(cardObj);
        else opponentHandCards.Remove(cardObj);

        ArrangeHand(isPlayer);
        if (cardObj != null) Destroy(cardObj);
    }

    public void ArrangeHand(bool isPlayer)
    {
        List<GameObject> hand = isPlayer ? playerHandCards : opponentHandCards;
        Transform handTransform = isPlayer ? playerHandTransform : opponentHandTransform;

        float totalWidth = (hand.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2;

        for (int i = 0; i < hand.Count; i++)
        {
            RectTransform cardRect = hand[i].GetComponent<RectTransform>();
            cardRect.anchoredPosition = new Vector2(startX + i * cardSpacing, 0);
        }
    }

    public void OpponentPlayRandomCard()
    {
        if (opponentHandCards.Count == 0) return;

        int randomIndex = Random.Range(0, opponentHandCards.Count);
        GameObject playedCard = opponentHandCards[randomIndex];

        // "Play" to field (destroy from hand)
        RemoveCardFromHand(playedCard, false);

        // TODO: Spawn on opponent field
        Debug.Log("Opponent played a card!");
    }
}
