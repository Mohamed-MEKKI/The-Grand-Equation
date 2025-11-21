using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    [Header("Player Hand")]
    public Transform playerHandTransform;     // Your "HandPanel"

    [Header("Opponent Hand")]
    public Transform opponentHandTransform;   // NEW: Drag opponent panel here

    [Header("Settings")]
    public float cardSpacing = 120f;
    public float opponentYOffset = -200f;     // Top of screen offset

    public List<GameObject> playerHandCards = new List<GameObject>();
    public List<GameObject> opponentHandCards = new List<GameObject>();

    public enum SelectionType { None, EliminateOpponentHand }

    public SelectionType currentSelection = SelectionType.None;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddCardToHand(GameObject card, bool isPlayer)
    {
        if (isPlayer)
        {
            playerHandCards.Add(card);
            card.transform.SetParent(playerHandTransform, false);
        }
        else
        {
            opponentHandCards.Add(card);
            card.transform.SetParent(opponentHandTransform, false);
        }
        ArrangeHand(isPlayer);
    }

    public Vector3 GetHandPosition(bool isPlayer)
    {
        List<GameObject> hand = isPlayer ? playerHandCards : opponentHandCards;
        float totalWidth = (hand.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2;
        return new Vector3(startX + hand.Count * cardSpacing * 0.6f, 0, 0);  // Spread
    }

    public void RemoveCardFromHand(GameObject cardObj, bool isPlayer = true)
    {
        if (isPlayer) playerHandCards.Remove(cardObj);
        else opponentHandCards.Remove(cardObj);

        ArrangeHand(isPlayer);
        if (cardObj != null) Destroy(cardObj);
    }

    public void EnterEliminationMode()
    {
        currentSelection = SelectionType.EliminateOpponentHand;

        // Highlight ALL opponent cards (red glow)
        foreach (var card in opponentHandCards)
        {
            //var image = card.GetComponent<Image>();
            //image.color = new Color(1, 0.3f, 0.3f, 0.8f);  // Red tint

            // Add click handler dynamically
            var selector = card.GetComponent<CardSelector>();
            if (selector == null)
                selector = card.AddComponent<CardSelector>();
            selector.selectionType = SelectionType.EliminateOpponentHand;
        }

        Debug.Log("?? Elimination Mode: Click opponent's card!");
    }

    public void ExitEliminationMode()
    {
        currentSelection = SelectionType.None;

        // Reset opponent hand colors
        foreach (var card in opponentHandCards)
        {
            //var image = card.GetComponent<artworkImage>();
            //image.color = Color.white;

            // Remove selector
            Destroy(card.GetComponent<CardSelector>());
        }
    }

    public void ArrangeHand(bool isPlayer)
    {
        List<GameObject> cards = isPlayer ? playerHandCards : opponentHandCards;
        Transform panel = isPlayer ? playerHandTransform : opponentHandTransform;

        if (cards.Count == 0) return;

        float spacing = 160f;
        float totalWidth = (cards.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            RectTransform rt = cards[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX + i * spacing, 0);
            rt.localScale = Vector3.one;
        }
    }
    public void ClearHands()
    {
        playerHandCards.Clear();
        opponentHandCards.Clear();
    }

    public void OpponentPlayRandomCard()
    {
        if (opponentHandCards.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, opponentHandCards.Count);
        GameObject playedCard = opponentHandCards[randomIndex];

        // "Play" to field (destroy from hand)
        RemoveCardFromHand(playedCard, false);

        // TODO: Spawn on opponent field
        Debug.Log("Opponent played a card!");
    }
}
