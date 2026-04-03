using System.Collections;
using TMPro;
using Unity.AppUI.UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    #endregion

    #region Fields
    [Header("Turn State")]
    public bool isPlayerTurn = true;
    public int currentRound = 1;

    [Header("Match State")]
    public int playerRoundWins = 0;
    public int opponentRoundWins = 0;
    [Range(3, 5)]
    public int roundsToWin = 3;
    private bool isResettingRound = false;

    [Header("UI")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI opponentScoreText;


    private CardDefiner lastClaimedRole;
    private GameObject pendingOpponentPlayedCard;
    private Coroutine opponentChallengeWindowRoutine;
    #endregion

    #region Initialization
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        roundsToWin = Mathf.Clamp(roundsToWin, 3, 5);
    }

    private void OnValidate()
    {
        roundsToWin = Mathf.Clamp(roundsToWin, 3, 5);
    }

    private IEnumerator Start()
    {
        if (DeckManager.Instance == null)
        {

            Debug.LogError("DeckManager.Instance is null. Cannot deal hands or start game loop.");
            yield break;
        }

        yield return StartCoroutine(DeckManager.Instance.DealHandsCoroutine());
        UpdateScoreUI();
        StartCoroutine(GameLoop());
    }
    #endregion

    #region Game Loop & Turn Management
    private IEnumerator GameLoop()
    {
        while (true)
        {
            CheckRoundEnd();
            yield return new WaitForSecondsRealtime(5f);
            yield return StartCoroutine(PlayerTurn());
            yield return new WaitForSecondsRealtime(5f);
            CheckRoundEnd();
            yield return new WaitForSecondsRealtime(5f);
            yield return StartCoroutine(OpponentTurn());
        }
    }

    private IEnumerator PlayerTurn()
    {
        isPlayerTurn = true;
        UpdateTurnUI();

        StartPlayerTurnTimers();

        while (isPlayerTurn)
        {
            yield return null;
        }

        // Timer is reset in EndTurn() when the player’s turn actually ends.
    }

    private IEnumerator OpponentTurn()
    {
        isPlayerTurn = false;
        ClaimMenu.Instance?.DisableClaimButton();

        UpdateTurnUI();

        yield return new WaitForSecondsRealtime(5f);

        if (HandManager.Instance != null)
            HandManager.Instance.OpponentPlayRandomCard();

        // Wait until the opponent play is fully resolved (challenge window or challenge click).
        // This prevents the turn from "moving on" while the card still looks unplayed.
        float timeout = 10f;
        while (pendingOpponentPlayedCard != null && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        // Leave the turn timer in a fresh stopped state until the next player turn starts.
        StopPlayerTurnTimers();
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        Debug.Log("YOUR TURN");
        UpdateTurnUI();

        StartPlayerTurnTimers();
    }

    public void EndTurn()
    {
        if (!isPlayerTurn) return;

        isPlayerTurn = false;
        UpdateTurnUI();
        // Full reset (duration + visuals) when the turn ends, not only pause at last value.
        StopPlayerTurnTimers();
    }

    public void EndPlayerTurn()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.isPaused) return;
        EndTurn();
        Debug.Log("Player ran out of time!");
        GameEventLog.AppendGlobal("Turn timed out: Player ended turn.");
    }

    private void UpdateTurnUI()
    {
        if (turnText != null)
            turnText.text = isPlayerTurn ? "Your Turn" : "Opponent's Turn";

        if (ClaimMenu.Instance != null)
            ClaimMenu.Instance.SetClaimButtonVisible(isPlayerTurn);
    }

    private void StartPlayerTurnTimers()
    {
        if (TurnTimer.Instance != null)
        {
            TurnTimer.Instance.ResetTimer();
            TurnTimer.Instance.StartNewTurn();
        }

        if (GameTimer.Instance != null)
            GameTimer.Instance.BeginPlayerTurn();
        else if (TurnTimer.Instance == null)
            Debug.LogWarning("GameManager: No TurnTimer or GameTimer in scene; turn countdown will not run.");
    }

    private void StopPlayerTurnTimers()
    {
        TurnTimer.Instance?.ResetTimer();
        GameTimer.Instance?.StopBetweenTurns();
    }
    #endregion

    #region Round Condition
    /// <summary>
    /// Replaces CheckWinCondition. Awards a round win and either
    /// starts the next round or ends the match.
    /// </summary>
    private bool CheckRoundEnd()
    {
        if (isResettingRound) return false;

        if (HandManager.Instance == null)
        {
            Debug.LogWarning("HandManager.Instance is null! Cannot check round end.");
            return false;
        }

        bool playerLost = HandManager.Instance.playerHandCards.Count == 0;
        bool opponentLost = HandManager.Instance.opponentHandCards.Count == 0;

        if (!playerLost && !opponentLost) return false;

        if (opponentLost)
        {
            playerRoundWins++;
            Debug.Log($"Player wins the round! ({playerRoundWins}/{roundsToWin})");
            GameEventLog.AppendGlobal($"Round win: Player ({playerRoundWins}/{roundsToWin}).");
        }
        else
        {
            opponentRoundWins++;
            Debug.Log($"Opponent wins the round! ({opponentRoundWins}/{roundsToWin})");
            GameEventLog.AppendGlobal($"Round win: Opponent ({opponentRoundWins}/{roundsToWin}).");
        }

        UpdateScoreUI();

        // Check match winner
        if (playerRoundWins >= roundsToWin)
        {
            GameEventLog.AppendGlobal("Match end: Player wins.");
            GameOver(true);
            return true;
        }

        if (opponentRoundWins >= roundsToWin)
        {
            GameEventLog.AppendGlobal("Match end: Opponent wins.");
            GameOver(false);
            return true;
        }

        // Nobody has won the match yet — reset for next round
        GameEventLog.AppendGlobal("Next round starting...");
        StartCoroutine(StartNextRound());
        return true;
    }

    /// <summary>
    /// Called from ResolveChallenge — same logic, just not inside the GameLoop.
    /// </summary>
    private void CheckRoundEndFromChallenge()
    {
        if (isResettingRound) return;

        if (HandManager.Instance == null) return;

        bool playerLost = HandManager.Instance.playerHandCards.Count == 0;
        bool opponentLost = HandManager.Instance.opponentHandCards.Count == 0;

        if (!playerLost && !opponentLost) return;

        if (opponentLost)
        {
            playerRoundWins++;
            Debug.Log($"Player wins the round! ({playerRoundWins}/{roundsToWin})");
            GameEventLog.AppendGlobal($"Round win: Player ({playerRoundWins}/{roundsToWin}).");
        }
        else
        {
            opponentRoundWins++;
            Debug.Log($"Opponent wins the round! ({opponentRoundWins}/{roundsToWin})");
            GameEventLog.AppendGlobal($"Round win: Opponent ({opponentRoundWins}/{roundsToWin}).");
        }

        UpdateScoreUI();

        if (playerRoundWins >= roundsToWin)
        {
            GameEventLog.AppendGlobal("Match end: Player wins.");
            GameOver(true);
            return;
        }

        if (opponentRoundWins >= roundsToWin)
        {
            GameEventLog.AppendGlobal("Match end: Opponent wins.");
            GameOver(false);
            return;
        }

        GameEventLog.AppendGlobal("Next round starting...");
        StartCoroutine(StartNextRound());
    }

    private IEnumerator StartNextRound()
    {
        isResettingRound = true;
        currentRound++;

        if (roundText != null)
            roundText.text = $"Round {currentRound}";

        // Clear all hands
        if (HandManager.Instance != null)
            HandManager.Instance.ClearHands();

        // Reset both players' coins at the end of every round.
        if (RoleAbilityManager.Instance != null)
            RoleAbilityManager.Instance.ResetCoinsForNewRound();

        yield return new WaitForSeconds(1f);

        // Reshuffle and redeal
        if (DeckManager.Instance != null)
        {
            yield return StartCoroutine(DeckManager.Instance.DealHandsCoroutine());
        }

        isResettingRound = false;
    }

    private void UpdateScoreUI()
    {
        if (playerScoreText != null)
            playerScoreText.text = $"{playerRoundWins}";

        if (opponentScoreText != null)
            opponentScoreText.text = $"{opponentRoundWins}";
    }
    #endregion

    #region Role Claiming
    public void ClaimRole(CardDefiner claimedRole)
    {
        if (claimedRole == null)
        {
            Debug.LogError("Cannot claim null role!");
            return;
        }

        Debug.Log($"Player claims: {claimedRole.cardName} → {claimedRole.possibleActions}");
        GameEventLog.AppendGlobal($"Player claims: {claimedRole.cardName}.");
        lastClaimedRole = claimedRole;

        // Playing a real card — execute abilities immediately, no challenge possible
        if (RoleAbilityManager.Instance != null && claimedRole.abilities != null)
        {
            foreach (var ability in claimedRole.abilities)
            {
                if (ability != null)
                    RoleAbilityManager.Instance.ExecuteAbility(ability, true);
            }
        }
    }


    public void ClaimRoleByName(string roleName)
    {
        
        if (CardDatabase.cardList == null)
        {
            Debug.LogError("ClaimRoleByName: CardDatabase.cardList is null!");
            return;
        }

        if (string.IsNullOrEmpty(roleName))
        {
            Debug.LogError("ClaimRoleByName: Role name is null or empty!");
            return;
        }

        CardDefiner fakeRole = FindRoleInDatabase(roleName);
        if (fakeRole == null)
        {
            Debug.LogWarning($"ClaimRoleByName: Role '{roleName}' not found in CardDatabase!");
            return;
        }

        lastClaimedRole = fakeRole;
        Debug.Log($"BLUFF: You claim {roleName}");
        GameEventLog.AppendGlobal($"Player bluffs: {roleName}.");


        StartCoroutine(AIDecideChallenge());

        EndTurn();
    }

    private CardDefiner FindRoleInDatabase(string roleName)
    {
        foreach (var card in CardDatabase.cardList)
        {
            if (card != null && card.cardName != null && card.cardName == roleName)
                return card;
        }
        return null;
    }
    #endregion

    #region Challenge System
    private IEnumerator PlayerDecideChallenge()
    {
        yield return new WaitForSeconds(5);
        Debug.Log("PLAYER CHALLENGES");
        GameEventLog.AppendGlobal("Player challenges.");
        OpponentChallengeIssued();
    }
    private IEnumerator AIDecideChallenge()
    {
        yield return new WaitForSeconds(5);

        if (Random.value < 0.5f)
        {
            ChallengeIssued();
        }
        else
        {
            Debug.Log("Opponent lets it pass...");
            GameEventLog.AppendGlobal("Opponent lets it pass.");
            yield return new WaitForSeconds(5);
            ExecuteClaimedRoleAbilities(true);
            yield return new WaitForSeconds(5);
        }
    }
    public void OpponentChallengeIssued()
    {
        Debug.Log("Player challenges ! ");
        GameEventLog.AppendGlobal("Player challenges opponent's claim.");
        ResolveOpponentChallenge(true);
    }

    public void ChallengeIssued()
    {
        Debug.Log("OPPONENT CHALLENGES!");
        GameEventLog.AppendGlobal("Opponent challenges.");
        ResolveChallenge(true);
    }

    public void ResolveChallenge(bool revealClaimed)
    {
        if (!revealClaimed || lastClaimedRole == null)
            return;

        bool playerHasRole = PlayerHasRoleInHand(lastClaimedRole);
        Debug.Log($"REVEAL: You {(playerHasRole ? "HAD" : "DID NOT HAVE")} {lastClaimedRole.cardName}");
        GameEventLog.AppendGlobal($"Reveal (player): {(playerHasRole ? "had" : "did not have")} {lastClaimedRole.cardName}.");

        if (playerHasRole)
        {
            if (TryLoseRandomCard(false))
                Debug.Log("You had the role! Opponent loses a card.");
            GameEventLog.AppendGlobal("Challenge result: player truthful; opponent loses a card.");
            ExecuteClaimedRoleAbilities(true);
        }
        else
        {
            if (TryLoseRandomCard(true))
                Debug.Log("You lied! You lose a random card.");
            GameEventLog.AppendGlobal("Challenge result: player lied; player loses a card.");
        }

        if (ChallengeButton.Instance != null)
            ChallengeButton.Instance.HideChallenge();

        // Use challenge version — we're outside the GameLoop here
        CheckRoundEndFromChallenge();
    }
    public void ResolveOpponentChallenge(bool revealClaimed)
    {
        if (!revealClaimed || lastClaimedRole == null)
            return;

        bool opponentHasRole = OpponentHasRoleInHand(lastClaimedRole);
        Debug.Log($"REVEAL: You {(opponentHasRole ? "HAD" : "DID NOT HAVE")} {lastClaimedRole.cardName}");
        GameEventLog.AppendGlobal($"Reveal (opponent): {(opponentHasRole ? "had" : "did not have")} {lastClaimedRole.cardName}.");

        if (opponentHasRole)
        {
            if (TryLoseRandomCard(true))
                Debug.Log("Opponent had the role! You lose a random card.");
            GameEventLog.AppendGlobal("Challenge result: opponent truthful; player loses a card.");
            ExecuteClaimedRoleAbilities(false);
        }
        else
        {
            if (TryLoseRandomCard(false))
                Debug.Log("Opponent lied! Opponent loses a random card.");
            GameEventLog.AppendGlobal("Challenge result: opponent lied; opponent loses a card.");
        }

        if (ChallengeButton.Instance != null)
            ChallengeButton.Instance.HideChallenge();

        FinalizePendingOpponentPlay();

        // Use challenge version — we're outside the GameLoop here
        CheckRoundEndFromChallenge();
    }

    private bool PlayerHasRoleInHand(CardDefiner role)
    {
        if (role == null || HandManager.Instance == null)
            return false;

        foreach (var card in HandManager.Instance.playerHandCards)
        {
            if (card == null) continue;

            CardDisplay display = card.GetComponent<CardDisplay>();
            if (display != null && display.card != null && display.card.cardId == role.cardId)
                return true;
        }

        return false;
    }
    private bool OpponentHasRoleInHand(CardDefiner role)
    {
        if (role == null || HandManager.Instance == null)
            return false;

        foreach (var card in HandManager.Instance.opponentHandCards)
        {
            if (card == null) continue;

            CardDisplay display = card.GetComponent<CardDisplay>();
            if (display != null && display.card != null && display.card.cardId == role.cardId)
                return true;
        }

        return false;
    }

    private void ExecuteClaimedRoleAbilities(bool isPlayerActor)
    {
        if (lastClaimedRole == null)
        {
            Debug.LogWarning("ExecuteClaimedRoleAbilities: lastClaimedRole is null!");
            return;
        }

        if (lastClaimedRole.abilities == null)
        {
            Debug.LogWarning($"ExecuteClaimedRoleAbilities: Role '{lastClaimedRole.cardName}' has no abilities!");
            return;
        }

        if (RoleAbilityManager.Instance == null)
        {
            Debug.LogError("ExecuteClaimedRoleAbilities: RoleAbilityManager.Instance is null!");
            return;
        }

        foreach (var ability in lastClaimedRole.abilities)
        {
            if (ability != null)
                RoleAbilityManager.Instance.ExecuteAbility(ability, isPlayerActor);
        }
    }
    #endregion

    #region Opponent Claim Window
    public void BeginOpponentClaim(CardDefiner claimedRole, GameObject playedCard)
    {
        if (claimedRole == null)
        {
            Debug.LogError("BeginOpponentClaim: claimedRole is null.");
            return;
        }

        pendingOpponentPlayedCard = playedCard;
        lastClaimedRole = claimedRole;
        GameEventLog.AppendGlobal($"Opponent claims: {claimedRole.cardName}.");

        // If there's no Challenge UI in the scene, resolve immediately (old behavior).
        if (ChallengeButton.Instance == null)
        {
            GameEventLog.AppendGlobal("No challenge UI: resolving opponent claim.");
            ExecuteClaimedRoleAbilities(false);
            FinalizePendingOpponentPlay();
            return;
        }

        ChallengeButton.Instance.ShowChallengeOpponent(claimedRole.cardName);

        if (opponentChallengeWindowRoutine != null)
            StopCoroutine(opponentChallengeWindowRoutine);

        opponentChallengeWindowRoutine = StartCoroutine(OpponentClaimChallengeWindow());
    }

    private IEnumerator OpponentClaimChallengeWindow()
    {
        // Small window for the player to click "Challenge"
        yield return new WaitForSecondsRealtime(3f);

        if (ChallengeButton.Instance != null)
            ChallengeButton.Instance.HideChallenge();

        // No challenge: execute opponent's claimed abilities, then discard the played card
        GameEventLog.AppendGlobal("No challenge: opponent claim resolves.");
        ExecuteClaimedRoleAbilities(false);
        FinalizePendingOpponentPlay();
    }

    private void FinalizePendingOpponentPlay()
    {
        if (pendingOpponentPlayedCard == null)
        {
            pendingOpponentPlayedCard = null;
            return;
        }

        // End the pending-claim state without forcing an extra card loss.
        // Opponent card loss should happen only from challenge/ability outcomes.
        /*CardDisplay display = pendingOpponentPlayedCard.GetComponent<CardDisplay>();
        if (display != null)
        {
            display.SetFaceUp(false);
        }*/

        pendingOpponentPlayedCard = null;
    }
    #endregion

    #region Card Loss Management
    public bool TryLoseRandomCard(bool isPlayer)
    {
        if (HandManager.Instance == null)
        {
            Debug.LogError("TryLoseRandomCard: HandManager.Instance is null!");
            return false;
        }

        var hand = isPlayer
            ? HandManager.Instance.playerHandCards
            : HandManager.Instance.opponentHandCards;

        if (hand == null || hand.Count == 0)
        {
            string playerName = isPlayer ? "Player" : "Opponent";
            Debug.LogWarning($"TryLoseRandomCard: {playerName} has no cards to lose!");
            return false;
        }

        GameObject randomCard = HandManager.Instance.GetRandomCard(isPlayer);
        if (randomCard == null)
        {
            Debug.LogWarning($"TryLoseRandomCard: GetRandomCard returned null!");
            return false;
        }

        HandManager.Instance.RemoveCardFromHand(randomCard, isPlayer);
        return true;
    }

    [System.Obsolete("Use TryLoseRandomCard instead for safety checks")]
    public void LoseRandomCard(bool isPlayer)
    {
        if (!TryLoseRandomCard(isPlayer))
            Debug.LogWarning($"LoseRandomCard: Failed to remove card from {(isPlayer ? "player" : "opponent")} hand.");
    }
    #endregion

    #region Animations
    public void GameOver(bool playerWon)
    {
        Time.timeScale = 0f;
        GameEventLog.AppendGlobal(playerWon ? "Game over: Victory." : "Game over: Defeat.");

        if (GameOverAnimation.Instance == null)
        {
            Debug.LogError("GameOverAnimation.Instance is null!");
            return;
        }

        if (playerWon)
            GameOverAnimation.Instance.ShowVictory();
        else
            GameOverAnimation.Instance.ShowDefeat();
    }
    #endregion
}