using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class HandManager : MonoBehaviour
{
    [Header("UI")]
    public Transform Hand;
    public GameObject CardPrefab;

    [Header("Hand")]
    public List<CardData> cardsInHand = new List<CardData>();

    public void FillHands()
    {
        for(int i = 0; i < cardsInHand.Count; i++)
        {
            if (Hand == null) { Debug.LogError("Hand panel not assigned"); return; }
            if (CardPrefab == null) { Debug.LogError("Card prefab not assigned"); return; }

            var card = DeckManager.Instance?.DrawCard();
            if (card == null)
            {
                Debug.LogWarning("Deck empty or DeckManager missing.");
                break;
            }
            AddCardToHand(card);
        }
    } 
    public void AddCardToHand(CardData cardData)
    {
        cardsInHand.Add(cardData);

        GameObject go = Instantiate(CardPrefab, Hand, false);
        // Ensure RectTransform scale/position are preserved for UI layout
        var rt = go.GetComponent<RectTransform>();
        if (rt != null) { rt.localScale = Vector3.one; }

        CardDisplay display = go.GetComponent<CardDisplay>();
        if (display != null) display.Setup(cardData);
        else Debug.LogWarning("Card prefab missing CardDisplay component.");
    }
  
}
