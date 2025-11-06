using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public CardData card;
    public DeckManager deckManager;

    
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI NameText;
    public Image ArtworkImage;
    public bool cardBack;
    public static bool cardTurnedBack;

    public GameObject Hand;
    public int numberOfCardsInHand;

    public void Setup(CardData card)
    {
        if (card == null) return;
        if (NameText != null) NameText.text = card.cardName ?? "";
        if (DescriptionText != null) DescriptionText.text = card.description ?? "";
        if (ArtworkImage != null) ArtworkImage.sprite = card.artwork;
    }
    void Start()
    {
        numberOfCardsInHand = deckManager.deckSize;
        if (card == null)
        {
            Debug.LogError("CardData not assigned!");
            return;
        }

        NameText.text = card.cardName;
        DescriptionText.text = card.description;
        ArtworkImage.sprite = card.artwork;

        Debug.Log($"Displaying card: {card.cardName}");
    }

    private bool GetCardBack()
    {
        return cardBack;
    }

    void Update()
    {
         Hand = GameObject.Find("HandPanel");

        if (this.transform.parent == Hand.transform.parent)
        {
            cardBack = false;
        }
        cardTurnedBack = cardBack;

        if ( this.tag == "Clone")
        {
            //displayCard[0] = deckManager.playerStaticDeck[deckManager.deckSize - 1];
            numberOfCardsInHand += 1;
            deckManager.deckSize -= 1;
            cardBack = false;
            this.tag = "false";
        }
    }
}
