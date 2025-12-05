using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

    public void AddCardToHand(GameObject cardObj, bool isPlayer)
    {
        if (isPlayer)
        {
            playerHandCards.Add(cardObj);
            cardObj.transform.SetParent(playerHandTransform, false);
        }
        else
        {
            opponentHandCards.Add(cardObj);
            cardObj.transform.SetParent(opponentHandTransform, false);
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
        var handCards = isPlayer ? playerHandCards : opponentHandCards;
        var handTransform = isPlayer ? playerHandTransform : opponentHandTransform;

        if (handCards.Count == 0) return;

        float totalWidth = (handCards.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < handCards.Count; i++)
        {
            RectTransform rt = handCards[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX + i * cardSpacing, 0f);
            rt.localScale = Vector3.one;
        }
    }
    public void ClearHands()
    {
        foreach (var card in playerHandCards) if (card) Destroy(card);
        foreach (var card in opponentHandCards) if (card) Destroy(card);
        playerHandCards.Clear();
        opponentHandCards.Clear();
    }
    public void OpponentPlayRandomCard()
    {
        if (opponentHandCards.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, opponentHandCards.Count);
        GameObject playedCard = opponentHandCards[randomIndex];

        // Remove from hand (reveals role)
        var display = playedCard.GetComponent<CardDisplay>();
        display.SetFaceUp(true);  // Reveal opponent's role

        RemoveCardFromHand(playedCard, false);

        // Trigger opponent's ability
        if (display.card != null)
        {
            foreach (var ability in display.card.abilities)
            {
                RoleAbilityManager.Instance.ExecuteAbility(ability);
            }
        }

        Debug.Log($"Opponent played: {display.card?.cardName}");
    }
}
