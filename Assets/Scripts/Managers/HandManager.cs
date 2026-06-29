using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    [Header("Player Hand")]
    public RectTransform playerHandTransform;

    [Header("Opponent Hand")]
    public RectTransform opponentHandTransform;

    [Header("Settings")]
    public float cardSpacing = 120f;

    [Header("Multiplayer (pass the device)")]
    [SerializeField] private float multiplayerCardSpacingOverride = 0f;

    [Header("Ability Feedback")]
    [SerializeField] private float abilityFlipDuration = 1.5f;

    public List<GameObject> playerHandCards = new List<GameObject>();
    public List<GameObject> opponentHandCards = new List<GameObject>();

    public enum SelectionType { None, EliminateOpponentHand, SwapPlayerCard }
    public SelectionType currentSelection = SelectionType.None;

    // Cached reusable lists — avoids per-call allocation
    private readonly List<GameObject> _swapTemp = new List<GameObject>();
    private readonly List<GameObject> _scratchList = new List<GameObject>();
    private readonly List<GameObject> _revealSnap = new List<GameObject>();
    private readonly List<GameObject> _flashSnap = new List<GameObject>();
    private readonly List<GameObject> _clearPlayer = new List<GameObject>();
    private readonly List<GameObject> _clearOpponent = new List<GameObject>();

    // Cached coroutine references — prevents orphaned coroutines
    private Coroutine _revealCoroutine;
    private Coroutine _flashCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddCardToHand(GameObject cardObj, bool isPlayer)
    {
        if (cardObj == null) { Debug.LogError("AddCardToHand: cardObj is null!"); return; }
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

    private float GetActiveCardSpacing()
    {
        if (multiplayerCardSpacingOverride > 0f &&
            GameManager.Instance != null &&
            GameManager.Instance.IsMultiplayer)
            return multiplayerCardSpacingOverride;
        return cardSpacing;
    }

    /// <summary>Returns a reusable scratch list of non-null cards. Do not store reference between frames.</summary>
    public List<GameObject> GetValidCards(bool isPlayer)
    {
        _scratchList.Clear();
        var hand = isPlayer ? playerHandCards : opponentHandCards;
        foreach (var c in hand)
            if (c != null) _scratchList.Add(c);
        return _scratchList;
    }

    public void SwapHandPositions()
    {
        if (playerHandTransform == null || opponentHandTransform == null)
        {
            Debug.LogWarning("SwapHandPositions: hand transforms not assigned.");
            return;
        }

        foreach (var card in playerHandCards)
            if (card != null) card.transform.SetParent(opponentHandTransform, false);

        foreach (var card in opponentHandCards)
            if (card != null) card.transform.SetParent(playerHandTransform, false);

        // Swap lists without allocating a new List
        _swapTemp.Clear();
        _swapTemp.AddRange(playerHandCards);
        playerHandCards.Clear();
        playerHandCards.AddRange(opponentHandCards);
        opponentHandCards.Clear();
        opponentHandCards.AddRange(_swapTemp);
        _swapTemp.Clear();

        ArrangeHand(true);
        ArrangeHand(false);
    }

    public void RemoveCardFromHand(GameObject cardObj, bool isPlayer = true)
    {
        if (cardObj == null) return;
        if (isPlayer) playerHandCards.Remove(cardObj);
        else opponentHandCards.Remove(cardObj);
        ArrangeHand(isPlayer);
        Destroy(cardObj);
    }

    public void EnterSwapMode()
    {
        currentSelection = SelectionType.SwapPlayerCard;
        foreach (var card in playerHandCards)
        {
            if (card == null) continue;
            var selector = card.GetComponent<CardSelector>();
            if (selector == null) selector = card.AddComponent<CardSelector>();
            selector.selectionType = SelectionType.SwapPlayerCard;
        }
        GameEventLog.AppendGlobal("Select a card in your hand to swap.");
    }

    public void ExitSwapMode()
    {
        currentSelection = SelectionType.None;
        foreach (var card in playerHandCards)
        {
            if (card == null) continue;
            var selector = card.GetComponent<CardSelector>();
            if (selector != null) Destroy(selector);
        }
    }

    public void EnterEliminationMode()
    {
        currentSelection = SelectionType.EliminateOpponentHand;
        foreach (var card in opponentHandCards)
        {
            if (card == null) continue;
            var selector = card.GetComponent<CardSelector>();
            if (selector == null) selector = card.AddComponent<CardSelector>();
            selector.selectionType = SelectionType.EliminateOpponentHand;
        }
    }

    public void ExitEliminationMode()
    {
        currentSelection = SelectionType.None;
        foreach (var card in opponentHandCards)
        {
            if (card == null) continue;
            var selector = card.GetComponent<CardSelector>();
            if (selector != null) Destroy(selector);
        }
    }

    public void ArrangeHand(bool isPlayer)
    {
        var handCards = isPlayer ? playerHandCards : opponentHandCards;
        var handTransform = isPlayer ? playerHandTransform : opponentHandTransform;

        if (handCards.Count == 0) return;

        float spacing = GetActiveCardSpacing();
        float totalWidth = (handCards.Count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < handCards.Count; i++)
        {
            if (handCards[i] == null) continue;
            var rt = handCards[i].GetComponent<RectTransform>();
            if (rt == null) continue;
            rt.anchoredPosition = new Vector2(startX + i * spacing, 0f);
            rt.localScale = Vector3.one;
        }
    }

    public void ClearHands()
    {
        // Snapshot into cached lists before clearing to avoid stale refs
        _clearPlayer.Clear();
        _clearOpponent.Clear();
        _clearPlayer.AddRange(playerHandCards);
        _clearOpponent.AddRange(opponentHandCards);
        playerHandCards.Clear();
        opponentHandCards.Clear();
        foreach (var card in _clearPlayer) if (card) Destroy(card);
        foreach (var card in _clearOpponent) if (card) Destroy(card);
        _clearPlayer.Clear();
        _clearOpponent.Clear();
    }

    public void FlipAllCardsDown()
    {
        foreach (var card in playerHandCards)
            card?.GetComponent<CardDisplay>()?.SetFaceUp(false);
        foreach (var card in opponentHandCards)
            card?.GetComponent<CardDisplay>()?.SetFaceUp(false);
    }

    public void RevealPlayerHandAnimated()
    {
        if (_revealCoroutine != null) StopCoroutine(_revealCoroutine);
        _revealCoroutine = StartCoroutine(RevealPlayerHandCoroutine());
    }

    private IEnumerator RevealPlayerHandCoroutine()
    {
        // Snapshot into cached list — avoids allocation
        _revealSnap.Clear();
        _revealSnap.AddRange(playerHandCards);

        foreach (var card in _revealSnap)
        {
            if (card == null) continue;
            var display = card.GetComponent<CardDisplay>();
            if (display == null) continue;
            var flip = card.GetComponent<CardFlip>();
            if (flip != null) flip.StartFlip(true, display);
            else display.SetFaceUp(true);
            yield return new WaitForSeconds(0.1f);
        }
        _revealSnap.Clear();
    }

    public void RevealOneCardOnly(bool isPlayer)
    {
        var hand = isPlayer ? playerHandCards : opponentHandCards;
        if (hand.Count == 0) return;

        int revealIndex = Random.Range(0, hand.Count);
        for (int i = 0; i < hand.Count; i++)
        {
            if (hand[i] == null) continue;
            hand[i].GetComponent<CardDisplay>()?.SetFaceUp(i == revealIndex);
        }
    }

    public GameObject GetRandomCard(bool isPlayer)
    {
        var hand = isPlayer ? playerHandCards : opponentHandCards;
        if (hand.Count == 0) return null;
        return hand[UnityEngine.Random.Range(0, hand.Count)];
    }

    public void FlashCardsDown(bool isPlayer)
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsMultiplayer) return;
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashCardsDownCoroutine(isPlayer, abilityFlipDuration));
    }

    private IEnumerator FlashCardsDownCoroutine(bool isPlayer, float duration)
    {
        // Snapshot into cached list — avoids allocation
        _flashSnap.Clear();
        _flashSnap.AddRange(isPlayer ? playerHandCards : opponentHandCards);

        foreach (var card in _flashSnap)
            card?.GetComponent<CardDisplay>()?.SetFaceUp(false);

        yield return new WaitForSecondsRealtime(duration);

        if (isPlayer)
        {
            foreach (var card in _flashSnap)
                if (card != null && playerHandCards.Contains(card))
                    card.GetComponent<CardDisplay>()?.SetFaceUp(true);
        }
        _flashSnap.Clear();
    }

    public void OpponentPlayRandomCard()
    {
        if (opponentHandCards.Count == 0)
        {
            Debug.LogWarning("Opponent has no cards to play!");
            return;
        }

        int randomIndex = Random.Range(0, opponentHandCards.Count);
        GameObject played = opponentHandCards[randomIndex];

        if (played == null)
        {
            Debug.LogError("Selected opponent card is null!");
            opponentHandCards.RemoveAt(randomIndex);
            return;
        }

        var display = played.GetComponent<CardDisplay>();
        if (display == null) { Debug.LogError("Opponent card missing CardDisplay!"); RemoveCardFromHand(played, false); return; }
        if (display.card == null) { Debug.LogError("Opponent card missing card data!"); RemoveCardFromHand(played, false); return; }

        display.SetFaceUp(false);
        GameEventLog.AppendGlobal("Opponent plays: " + display.card.cardName + ".");

        if (GameManager.Instance != null)
            GameManager.Instance.BeginOpponentClaim(display.card, played);
        else
        {
            var abilities = display.card.abilities;
            if (abilities != null && RoleAbilityManager.Instance != null)
                foreach (var ability in abilities)
                    if (ability != null)
                        RoleAbilityManager.Instance.ExecuteAbility(ability, false);
        }
    }
}