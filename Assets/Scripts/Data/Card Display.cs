using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public CardData card;
    

    
    public TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI NameText;
    public Image ArtworkImage;
    public bool cardBack;
    public static bool cardTurnedBack;

    public void Setup(CardData card)
    {
        if (card == null) return;
        if (NameText != null) NameText.text = card.cardName ?? "";
        if (DescriptionText != null) DescriptionText.text = card.description ?? "";
        if (ArtworkImage != null) ArtworkImage.sprite = card.artwork;
    }
    void Start()
    {
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


    void Update()
    {
         cardTurnedBack = cardBack;
    }
}
