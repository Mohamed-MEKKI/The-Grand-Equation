using System;
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
    private bool isMatchOver = false;

    [Header("Multiplayer (pass the device)")]
    [Tooltip("If true, match reads rounds-to-win from PlayerPrefs MultiplayerRoundsToWin (set by menu).")]
    [SerializeField] private bool applyMultiplayerRoundsFromMenu = true;

    public const string PlayerPrefsMultiplayerRounds = "MultiplayerRoundsToWin";

    [Header("UI")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI opponentScoreText;

    [Header("Animations")]
    [SerializeField] private EndRoundAnimation endRoundAnimation;
    [SerializeField] private GameOverAnimation gameOverAnimation;
    [SerializeField] private float endRoundAnimationFallbackDelay = 3.1f;



    private CardDefiner lastClaimedRole;
    private GameObject pendingOpponentPlayedCard;
    private Coroutine opponentChallengeWindowRoutine;
    private bool pendingSwapAfterActions = false;
    private bool awaitingPlayerAction = false;
    private int CurrentLevel { get; set; } = 0;
    // Time to wait between automated turn steps; adjustable by difficulty level.
    private float currentTurnWait = 1.2f;
    // Index 0=Beginner, 1=Intermediate, 2=Hard
    private float[] turnWaitByDifficulty = new float[] { 2.0f, 1.5f, 1.0f };
    // Probability the AI will challenge a bluff: 25 / 50 / 75 %
    private float[] challengeProbabilityByDifficulty = new float[] { 0.25f, 0.5f, 0.75f };
    private float currentChallengeProbability = 0.5f;
    public string DifficultyName { get; private set; } = "Intermediate";
    /// <summary>Two players taking turns on the same device (pass the phone). Set from menu via PlayerPrefs or <see cref="SetMultiplayerMode"/>.</summary>
    public bool IsMultiplayer { get; private set; }

    /// <summary>
    /// After an odd number of <see cref="SwapSidesIfMultiplayer"/> calls, player/opponent scoreboard rects have swapped positions.
    /// Coin values are always updated in <c>playerCoins</c> (bottom hand) / <c>opponentCoins</c> (top hand) after each hand swap;
    /// this flag tells <see cref="RoleAbilityManager"/> how to map those onto the fixed Player vs Opponent scoreboard widgets.
    /// </summary>
    public bool LocalMultiplayerScoreboardsSwapped { get; private set; }
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

        // Read multiplayer preference persisted by main menu
        int mp = PlayerPrefs.GetInt("IsMultiplayer", -1);
        if (mp == 1)
        {
            SetMultiplayerMode(true);
        }
        else if (mp == 0)
        {
            SetMultiplayerMode(false);
        }
        // Diagnostic log: confirm Awake ran and what value was read
        Debug.Log($"GameManager.Awake: PlayerPrefs.IsMultiplayer={mp}; IsMultiplayer={IsMultiplayer}");

        // Apply difficulty saved by LevelsManager (1=Beginner, 2=Intermediate, 3=Hard)
        int diff = PlayerPrefs.GetInt("SelectedDifficulty", 2);
        SetSelectedLevel(diff);
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
        if (gameOverAnimation != null)
            gameOverAnimation.HideMatchResultCanvas();
        StartCoroutine(GameLoop());
    }

    public void SetSelectedLevel(int index)
    {
        CurrentLevel = index;
        // index is 1-based (1=Beginner, 2=Intermediate, 3=Hard)
        int arrayIndex = Mathf.Clamp(index - 1, 0, turnWaitByDifficulty.Length - 1);
        currentTurnWait = turnWaitByDifficulty[arrayIndex];
        currentChallengeProbability = challengeProbabilityByDifficulty[arrayIndex];
        DifficultyName = index switch { 1 => "Beginner", 3 => "Hard", _ => "Intermediate" };
        Debug.Log($"Difficulty set: {DifficultyName} (level {index}), turnWait={currentTurnWait}s, challengeProb={currentChallengeProbability}");
    }

    public void SetMultiplayerMode(bool isMultiplayer)
    {
        IsMultiplayer = isMultiplayer;
        if (!isMultiplayer)
            LocalMultiplayerScoreboardsSwapped = false;
        if (isMultiplayer)
            ApplyMultiplayerMatchRulesFromPreferences();
        Debug.Log("Game mode set to: " + (isMultiplayer ? "Local 2-Player" : "Single Player"));
    }

    /// <summary>Re-read multiplayer rounds from menu prefs (e.g. after changing settings in lobby).</summary>
    public void ApplyMultiplayerMatchRulesFromPreferences()
    {
        if (!IsMultiplayer || !applyMultiplayerRoundsFromMenu)
            return;

        int fromMenu = PlayerPrefs.GetInt(PlayerPrefsMultiplayerRounds, -1);
        if (fromMenu >= 3 && fromMenu <= 5)
        {
            roundsToWin = fromMenu;
            roundsToWin = Mathf.Clamp(roundsToWin, 3, 5);
        }
    }
    #endregion

    #region Game Loop & Turn Management
    private IEnumerator GameLoop()
    {
        while (!isMatchOver)
        {
            CheckRoundEnd();
            if (isMatchOver) yield break;
            yield return new WaitForSecondsRealtime(currentTurnWait);
            if (isMatchOver) yield break;
            yield return StartCoroutine(PlayerTurn());
            if (isMatchOver) yield break;
            yield return new WaitForSecondsRealtime(currentTurnWait);
            if (isMatchOver) yield break;
            CheckRoundEnd();
            if (isMatchOver) yield break;
            yield return new WaitForSecondsRealtime(currentTurnWait);
            if (isMatchOver) yield break;

            if (IsMultiplayer)
            {
                yield return StartCoroutine(PlayerTurn());
            }
            else
            {
                yield return StartCoroutine(OpponentTurn());
            }
        }
    }

    private IEnumerator PlayerTurn()
    {
        if (isMatchOver) yield break;
        isPlayerTurn = true;

        UpdateTurnUI();
        StartPlayerTurnTimers();

        // Multiplayer: do not swap here — SwapSidesIfMultiplayer runs when a player finishes their play
        // so the next person holding the device sees their hand at the bottom and their scoreboard side.

        // Wait until player plays a card (turn ends automatically)
        while (isPlayerTurn && !isMatchOver)
        {
            yield return null;
        }

        // Turn has ended
        StopPlayerTurnTimers();
        UpdateTurnUI();
    }

    private IEnumerator OpponentTurn()
    {
        if (isMatchOver) yield break;
        if (IsMultiplayer)
        {
            Debug.LogError("OpponentTurn was invoked in multiplayer; use two PlayerTurn passes instead.");
            yield break;
        }

        isPlayerTurn = false;
        UpdateTurnUI();

        if (ClaimMenu.Instance != null)
            ClaimMenu.Instance.DisableClaimButton();

        // Brief pause so player sees it's opponent's turn
        yield return new WaitForSecondsRealtime(1.2f);

        if (isMatchOver) yield break;

        if (HandManager.Instance == null)
        {
            Debug.LogWarning("HandManager is null in OpponentTurn!");
            yield break;
        }

        // === SINGLE PLAYER - AI Turn (multiplayer uses a second PlayerTurn instead) ===
        HandManager.Instance.OpponentPlayRandomCard();

        // Wait for AI to finish playing card + any challenge/animation
        float timeout = 12f;
        while (pendingOpponentPlayedCard != null && timeout > 0f && !isMatchOver)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        StopPlayerTurnTimers();
    }
    /*
    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        Debug.Log("YOUR TURN");
        UpdateTurnUI();

        // In multiplayer mode swap hand positions so the active player's hand appears in the correct place
        if (IsMultiplayer)
        {
            if (HandManager.Instance != null)
            {
                HandManager.Instance.SwapHandPositions();
                UITurnManager.Instance.SwapScoreboards();

            }
            else
            {
                Debug.LogWarning("StartPlayerTurn: HandManager.Instance is null. Cannot swap hands.");
            }
        }

        StartPlayerTurnTimers();
    }
    */
    public void SetAwaitingPlayerAction(bool awaiting)
    {
        awaitingPlayerAction = awaiting;
    }

    public void EndTurn()
    {
        if (!isPlayerTurn) return;
        if (awaitingPlayerAction) return;

        // Pass-the-device: normally after any action that ends the turn the other player
        // gets the bottom hand + their scoreboard side. If a swap has been deferred
        // (e.g. we deferred until claim/ability actions resolved), don't swap now.
        if (!IsMultiplayer || !pendingSwapAfterActions)
        {
            SwapSidesIfMultiplayer();
        }

        isPlayerTurn = false;
        StopPlayerTurnTimers();
        UpdateTurnUI();
        // Full reset (duration + visuals) when the turn ends, not only pause at last value.
    }

    public void EndPlayerTurn()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.isPaused) return;
        if (awaitingPlayerAction)
        {
            awaitingPlayerAction = false;
            HandManager.Instance?.ExitSwapMode();
            GameEventLog.AppendGlobal("Swap cancelled: turn timed out.");
        }
        Debug.Log("Player ran out of time!");
        GameEventLog.AppendGlobal("Turn timed out: Player ended turn.");
        EndTurn();
    }

    private void UpdateTurnUI()
    {
        if (turnText != null)
        {
            if (isPlayerTurn)
                turnText.text = "Your Turn";
            else if (IsMultiplayer)
                turnText.text = "Pass the device — next player";
            else
                turnText.text = "Opponent's Turn";
        }

        //if (ClaimMenu.Instance != null)
          //  ClaimMenu.Instance.SetClaimButtonVisible(isPlayerTurn);
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
        if (isResettingRound || isMatchOver) return false;

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
        PlayEndRoundAnimation(currentRound);

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
        if (isResettingRound || isMatchOver) return;

        if (HandManager.Instance == null) return;

        bool playerLost = HandManager.Instance.playerHandCards.Count == 0;
        bool opponentLost = HandManager.Instance.opponentHandCards.Count == 0;

        if (!playerLost && !opponentLost) return;

        PlayEndRoundAnimation(currentRound);

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



        // Wait matches the animation duration (fadeIn + display + fadeOut)
        yield return new WaitForSeconds(3.1f); // 0.3 + 2 + 0.8 = 3.1

        if (roundText != null)
            roundText.text = $"Round {currentRound}";

        if (HandManager.Instance != null)
            HandManager.Instance.ClearHands();

        if (RoleAbilityManager.Instance != null)
            RoleAbilityManager.Instance.ResetCoinsForNewRound();

        yield return new WaitForSeconds(1f);

        if (DeckManager.Instance != null)
            yield return StartCoroutine(DeckManager.Instance.DealHandsCoroutine());

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

        EndTurn();
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

        // Single-player: AI decides whether to challenge.
        if (!IsMultiplayer)
        {
            StartCoroutine(AIDecideChallenge());        
            EndTurn();
        }
        else
        {
            // Multiplayer (pass-the-device): defer the visual swap until the
            // opponent's claim/challenge actions have resolved. Mark a pending
            // swap, end the current player's turn and then present the opponent
            // with the claim/challenge UI.
            pendingSwapAfterActions = true;
            EndTurn();
            BeginOpponentClaim(fakeRole, null);
        }
    }


    private void SwapSidesIfMultiplayer()
    {
        if (!IsMultiplayer) return;
        if (HandManager.Instance != null)
        {
            HandManager.Instance.SwapHandPositions();
            // Bottom hand is always playerHandCards; swap wallets so isPlayer==true still maps to playerCoins (bottom seat).
            RoleAbilityManager.Instance?.SwapHandCoinSemanticsForLocalMultiplayer();
            UITurnManager.Instance?.SwapScoreboards();
            LocalMultiplayerScoreboardsSwapped ^= true;
            RoleAbilityManager.Instance?.RefreshCoinDisplays();
        }
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
        yield return new WaitForSeconds(currentTurnWait);

        if (UnityEngine.Random.value < currentChallengeProbability)
        {
            ChallengeIssued();
        }
        else
        {
            Debug.Log("Opponent lets it pass...");
            GameEventLog.AppendGlobal("Opponent lets it pass.");
            yield return new WaitForSeconds(currentTurnWait);
            ExecuteClaimedRoleAbilities(true);
            yield return new WaitForSeconds(currentTurnWait);
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

        // If a swap was deferred while waiting for opponent to decide, perform it now
        if (pendingSwapAfterActions)
        {
            SwapSidesIfMultiplayer();
            pendingSwapAfterActions = false;
        }

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

        // Perform any deferred swap now that opponent actions resolved.
        if (pendingSwapAfterActions)
        {
            SwapSidesIfMultiplayer();
            pendingSwapAfterActions = false;
        }
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
        if (isMatchOver)
            return;

        isMatchOver = true;
        isPlayerTurn = false;
        pendingOpponentPlayedCard = null;
        if (opponentChallengeWindowRoutine != null)
        {
            StopCoroutine(opponentChallengeWindowRoutine);
            opponentChallengeWindowRoutine = null;
        }
        StopPlayerTurnTimers();
        ClaimMenu.Instance?.DisableClaimButton();
        StartCoroutine(ShowMatchResultAfterEndRoundAnimation(playerWon));
    }

    private IEnumerator ShowMatchResultAfterEndRoundAnimation(bool playerWon)
    {
        if (endRoundAnimation != null)
        {
            bool endRoundFinished = false;

            void HandleEndRoundComplete()
            {
                endRoundFinished = true;
            }

            endRoundAnimation.OnAnimationComplete += HandleEndRoundComplete;

            float timeout = Mathf.Max(0.1f, endRoundAnimationFallbackDelay + 0.5f);
            while (!endRoundFinished && timeout > 0f)
            {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }

            endRoundAnimation.OnAnimationComplete -= HandleEndRoundComplete;
        }
        else
        {
            yield return new WaitForSecondsRealtime(endRoundAnimationFallbackDelay);
        }

        Time.timeScale = 0f;
        GameEventLog.AppendGlobal(playerWon ? "Game over: Victory." : "Game over: Defeat.");
        ShowMatchResultUI(playerWon);
    }

    private void ShowMatchResultUI(bool playerWon)
    {
        GameOverAnimation anim = gameOverAnimation != null ? gameOverAnimation : GameOverAnimation.Instance;
        if (anim == null)
        {
            Debug.LogWarning("GameManager: gameOverAnimation is missing (field and singleton are null).");
            return;
        }

        // Play the animated overlay first; show the static score panel when it finishes.
        System.Action showCanvas = () => anim.ShowMatchResultCanvas(playerWon, playerRoundWins, opponentRoundWins);
        if (playerWon)
            anim.ShowVictory(showCanvas);
        else
            anim.ShowDefeat(showCanvas);
    }

    private void PlayEndRoundAnimation(int roundNumber)
    {
        EndRoundAnimation anim = endRoundAnimation != null ? endRoundAnimation : EndRoundAnimation.Instance;
        if (anim != null)
        {
            anim.Play(roundNumber);
        }
        else
        {
            Debug.LogWarning("GameManager: EndRoundAnimation is missing (field and singleton are null).");
        }
    }
    #endregion
}