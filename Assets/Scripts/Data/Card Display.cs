using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


[RequireComponent(typeof(CardFlip))]
public class CardDisplay : MonoBehaviour
{
    // -------------------------------------------------
    // 1. PUBLIC FIELDS (assign in the prefab)
    // -------------------------------------------------
    public CardData card;                 // set by DeckManager when the card is drawn
    public DeckManager deckManager;       // drag the DeckManager object here

    [Header("UI References")]
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;
    public Image ArtworkImage;

    public Sprite cardBackSprite;         // <-- drag your back image here

    // -------------------------------------------------
    // 2. INTERNAL STATE
    // -------------------------------------------------
    private bool _isFaceUp = false;       // true = front, false = back
    private CardFlip _flip;               // the flip animation component

    // -------------------------------------------------
    // 3. UNITY CALLBACKS
    // -------------------------------------------------
    private void Awake()
    {
        _flip = GetComponent<CardFlip>();
    }

    private void Start()
    {
        // Cards that start in the deck should be face-down
        SetFaceUp(false);
    }

    // -------------------------------------------------
    // 4. PUBLIC API (called from DeckManager / HandManager)
    // -------------------------------------------------
    public void Setup(CardData data)
    {
        card = data;
        SetFaceUp(false);                 // start as back
    }

    public void SetFaceUp(bool faceUp)
    {
        if (_isFaceUp == faceUp) return;  // already correct

        _isFaceUp = faceUp;

        if (faceUp)
        {
            // ---- SHOW FRONT ----
            if (ArtworkImage != null) ArtworkImage.sprite = card.artwork;
            if (NameText != null) NameText.text = card.cardName ?? "";
            if (DescriptionText != null) DescriptionText.text = card.description ?? "";

            // make texts visible
            SetTextsActive(true);
        }
        else
        {
            // ---- SHOW BACK ----
            if (ArtworkImage != null) ArtworkImage.sprite = cardBackSprite;

            // hide all texts while the back is visible
            SetTextsActive(false);
        }
    }

    // -------------------------------------------------
    // 5. FLIP WITH ANIMATION
    // -------------------------------------------------
    public void FlipTo(bool faceUp)
    {
        if (_isFaceUp == faceUp) return;

        // start the coroutine on the flip component
        _flip.StartFlip(faceUp, this);
    }

    // -------------------------------------------------
    // 6. HELPERS
    // -------------------------------------------------
    private void SetTextsActive(bool active)
    {
        if (NameText != null) NameText.gameObject.SetActive(active);
        if (DescriptionText != null) DescriptionText.gameObject.SetActive(active);
    }
}