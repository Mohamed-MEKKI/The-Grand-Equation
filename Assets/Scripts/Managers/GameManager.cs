using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Turn State")]
    public bool isPlayerTurn = true;
    public int currentRound = 1;

    [Header("UI")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI turnText;

    private CardDefiner lastClaimedRole;

    private void Awake()
    {
        if (Instance == null) { Instance = this;}
        else Destroy(gameObject);
    }

    private IEnumerator Start()
    {
        if (DeckManager.Instance == null)
        {
            Debug.LogError("DeckManager.Instance is null. Cannot deal hands or start game loop.");
            yield break;
        }

        Debug.Log("Starting DealHandsCoroutine and waiting for it to complete...");
        // Ensure shuffle + initial dealing completes before starting the main game loop
        yield return StartCoroutine(DeckManager.Instance.DealHandsCoroutine());

        Debug.Log("Deal complete. Starting GameLoop...");
        // Start the main game loop and wait for it to finish (game end)
        yield return StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while (true)  // Loop until someone wins
        {
            yield return StartCoroutine(PlayerTurn());
            if (CheckWinCondition()) yield break;

            yield return StartCoroutine(OpponentTurn());
            if (CheckWinCondition()) yield break;

            currentRound++;
            if (roundText) roundText.text = $"Round {currentRound}";
        }
    }

    IEnumerator PlayerTurn()
    {
        isPlayerTurn = true;
        UpdateTurnUI();

        // Start 1-minute timer
        TurnTimer.Instance.StartTimer();

        // Wait for player to end turn (button or timer)
        yield return new WaitUntil(() => !isPlayerTurn);

        TurnTimer.Instance.StopTimer();
    }

    public void EndTurn()
    {
        if (!isPlayerTurn) return;

        Debug.Log("PLAYER ENDED TURN");

        // Stop timer
        if (TurnTimer.Instance != null)
            TurnTimer.Instance.StopTimer();

        // End player turn and start opponent turn
        isPlayerTurn = false;
        StartCoroutine(OpponentTurn());
    }

    private IEnumerator OpponentTurn()
    {
        isPlayerTurn = false;
        Debug.Log("OPPONENT TURN");

        // Opponent plays ONE card instantly
        HandManager.Instance.OpponentPlayRandomCard();

        // Tiny dramatic pause
        yield return new WaitForSeconds(1.5f);

        // Back to player — FAST!
        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        Debug.Log("YOUR TURN");

        // Restart 60-second timer
        if (TurnTimer.Instance != null)
            TurnTimer.Instance.StartTimer();
    }



    void UpdateTurnUI()
    {
        if (turnText != null)
            turnText.text = isPlayerTurn ? "Your Turn" : "Opponent's Turn";
    }

    bool CheckWinCondition()
    {
        if (HandManager.Instance.playerHandCards.Count == 0)
        {
            ShowWinScreen("OPPONENT WINS!\nYou have no cards left.");
            return true;
        }
        else if (HandManager.Instance.opponentHandCards.Count == 0)
        {
            ShowWinScreen("YOU WIN!\nOpponent has no cards left!");
            return true;
        }
        return false;
    }

    void ShowWinScreen(string message)
    {
        Debug.Log("🎉 GAME OVER: " + message);
        Time.timeScale = 0f;  // Pause game
        // TODO: Show victory panel
    }

    public void PlayCard(CardDefiner cardDef, GameObject cardObj)
    {
        // 1. Remove from hand
        HandManager.Instance.RemoveCardFromHand(cardObj, true);

        // 2. Spawn on playfield (FACE DOWN)
        GameObject playedCard = Instantiate(cardObj);
        PlayedCard played = playedCard.GetComponent<PlayedCard>();
        played.Setup(cardDef);
        RoleAbilityManager.Instance.playerRoles.Add(played);

        // 3. Trigger ability
        foreach (var ability in cardDef.abilities)
        {
            RoleAbilityManager.Instance.ExecuteAbility(ability);
        }

        Debug.Log($"✅ Played {cardDef.cardName} → {cardDef.possibleActions}");
    }

    public void ClaimRole(CardDefiner claimedRole)
    {
        Debug.Log($"Player claims: {claimedRole.cardName} → {claimedRole.possibleActions}");

        foreach (var ability in claimedRole.abilities)
            RoleAbilityManager.Instance.ExecuteAbility(ability);
    }

    // Consolidated method - calls EndTurn() for consistency
    public void EndPlayerTurn()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.isPaused) return;
        EndTurn();
    }

    public void LoseRandomCard(bool isPlayer)
    {
        HandManager.Instance.RemoveCardFromHand(
            HandManager.Instance.GetRandomCard(isPlayer), isPlayer);
    }

    public void LoseSpecificCard(CardDefiner role)
    {
        // Find and remove the specific role card
        GameObject targetCard = null;
        var hand = isPlayerTurn ? HandManager.Instance.playerHandCards : HandManager.Instance.opponentHandCards;
        foreach (var card in hand)
        {
            if (card.GetComponent<CardDisplay>().card.cardId == role.cardId)
            {
                targetCard = card;
                break;
            }
        }
        if (targetCard != null)
            HandManager.Instance.RemoveCardFromHand(targetCard, isPlayerTurn);
    }

    private IEnumerator AIDecideChallenge()
    {
        yield return new WaitForSeconds(1.5f);  // Dramatic pause

        // AI decides whether to challenge (50% chance)
        if (Random.value < 0.5f)
        {
            ChallengeIssued();
        }
        else
        {
            Debug.Log("Opponent lets it pass...");
        }
    }

    public void ChallengeIssued()
    {
        Debug.Log("OPPONENT CHALLENGES!");
        ResolveChallenge(true);
    }

    public void ResolveChallenge(bool revealClaimed)
    {
        if (!revealClaimed || lastClaimedRole == null) return;

        Debug.Log($"REVEAL: You had {lastClaimedRole.cardName}");

        // TODO: Implement proper challenge resolution logic
        // For now, assume player has the role (challenger loses)
        bool playerHasRole = true; // TODO: Check if player actually has the role in hand
        
        if (playerHasRole)
        {
            // Player had the role → Challenger (opponent) loses card
            LoseRandomCard(false);
            Debug.Log("You had the role! Opponent loses a card.");
        }
        else
        {
            // Player lied → Player loses claimed card
            LoseSpecificCard(lastClaimedRole);
            Debug.Log("You lied! You lose the claimed card.");
        }

        // Hide challenge UI
        if (ChallengeButton.Instance != null)
            ChallengeButton.Instance.HideChallenge();

        CheckWinCondition();
    }
}
