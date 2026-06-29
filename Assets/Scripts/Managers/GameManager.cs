using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    #endregion

    [UnityEngine.ContextMenu("Test Victory Animation")]
    private void DebugTestVictory() => GameOver(true);
    [UnityEngine.ContextMenu("Test Defeat Animation")]
    private void DebugTestDefeat() => GameOver(false);

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

    [Header("Multiplayer")]
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

    [Header("Action Delays")]
    [SerializeField] private float turnTransitionDelay = 5f;
    [SerializeField] private float abilityResolveDelay = 3f;
    [SerializeField] private float challengeResultDelay = 5f;
    [SerializeField] private float roundStartDelay = 5f;

    [Header("Multiplayer Timing")]
    [SerializeField] private float hideAndSwapDelay = 0.6f;
    [SerializeField] private float swapToRevealDelay = 3f;
    [SerializeField] private float revealToChallengeDelay = 2f;
    [SerializeField] private float challengeWindowDuration = 4.0f;

    private bool multiplayerChallengeIssued = false;
    private bool multiplayerSequenceRunning = false;
    private Coroutine multiplayerTurnRoutine = null;

    private CardDefiner lastClaimedRole;
    private GameObject pendingOpponentPlayedCard;
    private Coroutine opponentChallengeWindowRoutine;
    private bool awaitingPlayerAction = false;
    private float currentTurnWait = 1.2f;
    private float[] turnWaitByDifficulty = { 2.0f, 1.5f, 1.0f };
    private float[] challengeProbabilityByDifficulty = { 0.25f, 0.5f, 0.75f };
    private float currentChallengeProbability = 0.5f;
    public string DifficultyName { get; private set; } = "Intermediate";
    public bool IsMultiplayer { get; private set; }
    public bool LocalMultiplayerScoreboardsSwapped { get; private set; }
    #endregion

    #region Initialization
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        roundsToWin = Mathf.Clamp(roundsToWin, 3, 5);
        int mp = PlayerPrefs.GetInt("IsMultiplayer", -1);
        if (mp == 1) SetMultiplayerMode(true);
        else if (mp == 0) SetMultiplayerMode(false);
        SetSelectedLevel(PlayerPrefs.GetInt("SelectedDifficulty", 2));
    }

    private void OnValidate() => roundsToWin = Mathf.Clamp(roundsToWin, 3, 5);

    private IEnumerator Start()
    {
        if (DeckManager.Instance == null) { Debug.LogError("DeckManager.Instance is null."); yield break; }
        yield return StartCoroutine(DeckManager.Instance.DealHandsCoroutine());
        UpdateScoreUI();
        gameOverAnimation?.HideMatchResultCanvas();
        if (roundText != null)
            roundText.text = LocalizationHelper.Loc("Round {0}", currentRound);
        StartCoroutine(GameLoop());
    }

    public void OnBackToMenuClicked() => SceneLoader.GoToMainMenu();

    public void SetSelectedLevel(int index)
    {
        int i = Mathf.Clamp(index - 1, 0, turnWaitByDifficulty.Length - 1);
        currentTurnWait = turnWaitByDifficulty[i];
        currentChallengeProbability = challengeProbabilityByDifficulty[i];
        DifficultyName = index switch { 1 => "Beginner", 3 => "Hard", _ => "Intermediate" };
    }

    public void SetMultiplayerMode(bool isMultiplayer)
    {
        IsMultiplayer = isMultiplayer;
        if (!isMultiplayer) LocalMultiplayerScoreboardsSwapped = false;
        if (isMultiplayer) ApplyMultiplayerMatchRulesFromPreferences();
        Debug.Log("Game mode: " + (isMultiplayer ? "Local 2-Player" : "Single Player"));
    }

    public void ApplyMultiplayerMatchRulesFromPreferences()
    {
        if (!IsMultiplayer || !applyMultiplayerRoundsFromMenu) return;
        int fromMenu = PlayerPrefs.GetInt(PlayerPrefsMultiplayerRounds, -1);
        if (fromMenu >= 3 && fromMenu <= 5)
            roundsToWin = Mathf.Clamp(fromMenu, 3, 5);
    }
    #endregion

    #region Game Loop & Turn Management
    private IEnumerator GameLoop()
    {
        while (!isMatchOver)
        {
            CheckRoundEnd();
            if (isMatchOver) yield break;
            yield return new WaitForSecondsRealtime(turnTransitionDelay);
            if (isMatchOver) yield break;

            yield return StartCoroutine(PlayerTurn());
            if (isMatchOver) yield break;

            yield return null;
            while (multiplayerSequenceRunning) yield return null;

            yield return new WaitForSecondsRealtime(turnTransitionDelay);
            if (isMatchOver) yield break;

            CheckRoundEnd();
            if (isMatchOver) yield break;

            yield return null;
            while (multiplayerSequenceRunning) yield return null;

            yield return new WaitForSecondsRealtime(turnTransitionDelay);
            if (isMatchOver) yield break;

            if (IsMultiplayer)
                yield return StartCoroutine(PlayerTurn());
            else
                yield return StartCoroutine(OpponentTurn());
        }
    }

    private IEnumerator PlayerTurn()
    {
        if (isMatchOver) yield break;
        isPlayerTurn = true;
        UpdateTurnUI();

        // Single player — unlock and show claim button
        // Multiplayer — claim button unlocked and shown by MultiplayerEndTurnSequence Step 10
        if (!IsMultiplayer)
        {
            ClaimMenu.Instance?.UnlockClaimButton();
            ClaimMenu.Instance?.SetClaimButtonVisible(true);
        }


        StartPlayerTurnTimers();
        while (isPlayerTurn && !isMatchOver) yield return null;
        StopPlayerTurnTimers();
        UpdateTurnUI();
    }

    private IEnumerator OpponentTurn()
    {
        if (isMatchOver) yield break;
        if (IsMultiplayer) { Debug.LogError("OpponentTurn invoked in multiplayer!"); yield break; }

        isPlayerTurn = false;
        UpdateTurnUI();
        ClaimMenu.Instance?.DisableClaimButton();

        yield return new WaitForSecondsRealtime(1.2f);
        if (isMatchOver) yield break;
        if (HandManager.Instance == null) { Debug.LogWarning("HandManager null in OpponentTurn!"); yield break; }

        HandManager.Instance.OpponentPlayRandomCard();

        float timeout = 12f;
        while (pendingOpponentPlayedCard != null && timeout > 0f && !isMatchOver)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }
        StopPlayerTurnTimers();
    }

    public void SetAwaitingPlayerAction(bool awaiting) => awaitingPlayerAction = awaiting;

    public void EndTurn()
    {
        if (!isPlayerTurn) return;
        if (awaitingPlayerAction) return;

        isPlayerTurn = false;
        StopPlayerTurnTimers();
        UpdateTurnUI();

        if (IsMultiplayer)
        {
            if (multiplayerTurnRoutine != null)
                StopCoroutine(multiplayerTurnRoutine);
            multiplayerTurnRoutine = StartCoroutine(MultiplayerEndTurnSequence());
        }
    }

    private IEnumerator MultiplayerEndTurnSequence()
    {
        multiplayerSequenceRunning = true;
        multiplayerChallengeIssued = false;

        // Step 1 — Hide cards + lock claim button
        HandManager.Instance?.FlipAllCardsDown();
        ClaimMenu.Instance?.DisableClaimButton();
        yield return new WaitForSecondsRealtime(hideAndSwapDelay);

        // Step 2 — Swap hands + coins
        if (HandManager.Instance != null)
        {
            HandManager.Instance.SwapHandPositions();
            RoleAbilityManager.Instance?.SwapHandCoinSemanticsForLocalMultiplayer();
            RoleAbilityManager.Instance?.RefreshCoinDisplays();
        }

        // Step 3 — Wait for device handoff
        yield return new WaitForSecondsRealtime(swapToRevealDelay);

        // Step 4 — Reveal next player's hand
        HandManager.Instance?.RevealPlayerHandAnimated();

        // Step 5 — Wait before challenge window
        yield return new WaitForSecondsRealtime(revealToChallengeDelay);

        // Step 6 — Show challenge button, keep claim + finish him locked
        ClaimMenu.Instance?.DisableClaimButton();
        RoleAbilityManager.Instance?.HideAssassinateButton();
        if (lastClaimedRole != null && ChallengeButton.Instance != null)
            ChallengeButton.Instance.ShowChallenge(lastClaimedRole.cardName);

        // Step 7 — Challenge window
        float timer = challengeWindowDuration;
        while (timer > 0f && !multiplayerChallengeIssued && !isMatchOver)
        {
            timer -= Time.unscaledDeltaTime;
            yield return null;
        }

        // Step 8 — Hide challenge button
        ChallengeButton.Instance?.HideChallenge();

        // Step 9 — Execute role if no challenge
        if (!multiplayerChallengeIssued && lastClaimedRole != null)
        {
            GameEventLog.AppendGlobal("No challenge — role resolves.");
            ExecuteClaimedRoleAbilities(false);
        }

        // Step 10 — Unlock and restore claim button for next player
        ClaimMenu.Instance?.UnlockClaimButton();
        ClaimMenu.Instance?.SetClaimButtonVisible(true);
        multiplayerSequenceRunning = false;
        multiplayerTurnRoutine = null;
    }

    public void EndPlayerTurn()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.isPaused) return;
        if (awaitingPlayerAction)
        {
            awaitingPlayerAction = false;
            HandManager.Instance?.ExitSwapMode();
            GameEventLog.AppendGlobal(LocalizationHelper.Loc("Swap cancelled"));
        }
        GameEventLog.AppendGlobal(LocalizationHelper.Loc("Turn timed out"));
        EndTurn();
    }

    private void UpdateTurnUI()
    {
        if (turnText != null)
        {
            turnText.text = isPlayerTurn
                ? LocalizationHelper.Loc("Your Turn")
                : IsMultiplayer
                    ? LocalizationHelper.Loc("Pass the device")
                    : LocalizationHelper.Loc("Opponent's Turn");
        }
        RoleAbilityManager.Instance?.ShowAssassinateButton();
    }

    private void StartPlayerTurnTimers()
    {
        if (TurnTimer.Instance != null) { TurnTimer.Instance.ResetTimer(); TurnTimer.Instance.StartNewTurn(); }
        GameTimer.Instance?.BeginPlayerTurn();
    }

    private void StopPlayerTurnTimers()
    {
        TurnTimer.Instance?.ResetTimer();
        GameTimer.Instance?.StopBetweenTurns();
    }
    #endregion

    #region Round Condition
    private bool CheckRoundEnd()
    {
        if (isResettingRound || isMatchOver) return false;
        if (HandManager.Instance == null) return false;

        bool playerLost = HandManager.Instance.playerHandCards.Count == 0;
        bool opponentLost = HandManager.Instance.opponentHandCards.Count == 0;
        if (!playerLost && !opponentLost) return false;

        if (opponentLost)
        {
            playerRoundWins++;
            GameEventLog.AppendGlobal(LocalizationHelper.Loc("Round win Player", playerRoundWins, roundsToWin));
        }
        else
        {
            opponentRoundWins++;
            GameEventLog.AppendGlobal(LocalizationHelper.Loc("Round win Opponent", opponentRoundWins, roundsToWin));
        }

        PlayEndRoundAnimation(currentRound);
        UpdateScoreUI();

        if (playerRoundWins >= roundsToWin) { GameOver(true); return true; }
        if (opponentRoundWins >= roundsToWin) { GameOver(false); return true; }

        StartCoroutine(StartNextRound());
        return true;
    }

    private IEnumerator StartNextRound()
    {
        isResettingRound = true;
        currentRound++;
        yield return new WaitForSeconds(endRoundAnimationFallbackDelay);
        if (roundText != null)
            roundText.text = LocalizationHelper.Loc("Round {0}", currentRound);
        HandManager.Instance?.ClearHands();
        RoleAbilityManager.Instance?.ResetCoinsForNewRound();
        ResetChallengeState();
        yield return new WaitForSeconds(roundStartDelay);
        if (DeckManager.Instance != null)
            yield return StartCoroutine(DeckManager.Instance.DealHandsCoroutine());

        // Restore claim button for new round
        ClaimMenu.Instance?.UnlockClaimButton();
        ClaimMenu.Instance?.SetClaimButtonVisible(true);

        isResettingRound = false;
    }

    private void ResetChallengeState()
    {
        lastClaimedRole = null;
        pendingOpponentPlayedCard = null;
        awaitingPlayerAction = false;
        multiplayerChallengeIssued = false;
        multiplayerSequenceRunning = false;
        ClaimMenu.Instance?.UnlockClaimButton();
        ChallengeButton.Instance?.ResetForNewRound();
        RoleAbilityManager.Instance?.ShowAssassinateButton();
        if (opponentChallengeWindowRoutine != null)
        {
            StopCoroutine(opponentChallengeWindowRoutine);
            opponentChallengeWindowRoutine = null;
        }
    }

    private void UpdateScoreUI()
    {
        if (playerScoreText != null) playerScoreText.text = $"{playerRoundWins}";
        if (opponentScoreText != null) opponentScoreText.text = $"{opponentRoundWins}";
    }
    #endregion

    #region Role Claiming
    public void ClaimRole(CardDefiner claimedRole)
    {
        if (claimedRole == null) { Debug.LogError("Cannot claim null role!"); return; }
        lastClaimedRole = claimedRole;
        ClaimMenu.Instance?.DisableClaimButton();
        if (RoleAbilityManager.Instance != null && claimedRole.abilities != null)
            foreach (var ability in claimedRole.abilities)
                if (ability != null)
                    RoleAbilityManager.Instance.ExecuteAbility(ability, true);
        EndTurn();
    }

    public void ClaimRoleByName(string roleName)
    {
        if (CardDatabase.cardList == null) { Debug.LogError("CardDatabase.cardList is null!"); return; }
        if (string.IsNullOrEmpty(roleName)) { Debug.LogError("Role name is null or empty!"); return; }
        CardDefiner fakeRole = FindRoleInDatabase(roleName);
        if (fakeRole == null) { Debug.LogWarning($"Role '{roleName}' not found!"); return; }
        lastClaimedRole = fakeRole;
        GameEventLog.AppendGlobal(LocalizationHelper.Loc("Player bluffs {0}", LocalizationHelper.Loc(roleName)));
        ClaimMenu.Instance?.DisableClaimButton();
        if (!IsMultiplayer) { StartCoroutine(AIDecideChallenge()); EndTurn(); }
        else { HandManager.Instance?.FlashCardsDown(true); EndTurn(); }
    }

    private CardDefiner FindRoleInDatabase(string roleName) => CardDatabase.FindByName(roleName);
    #endregion

    #region Challenge System
    private IEnumerator AIDecideChallenge()
    {
        yield return new WaitForSeconds(currentTurnWait);
        if (UnityEngine.Random.value < currentChallengeProbability)
            ChallengeIssued();
        else
        {
            GameEventLog.AppendGlobal(LocalizationHelper.Loc("Opponent lets it pass"));
            yield return new WaitForSeconds(currentTurnWait);
            ExecuteClaimedRoleAbilities(true);
        }
    }

    public void OpponentChallengeIssued()
    {
        GameEventLog.AppendGlobal(LocalizationHelper.Loc("Player challenges opponent claim"));
        ResolveOpponentChallenge(true);
    }

    public void ChallengeIssued()
    {
        GameEventLog.AppendGlobal(LocalizationHelper.Loc("Opponent challenges"));
        multiplayerChallengeIssued = true;
        ResolveChallenge(true);
    }

    public void ResolveChallenge(bool revealClaimed) => StartCoroutine(ResolveChallengeCoroutine(revealClaimed));

    private IEnumerator ResolveChallengeCoroutine(bool revealClaimed)
    {
        if (!revealClaimed || lastClaimedRole == null) yield break;
        bool playerHasRole = PlayerHasRoleInHand(lastClaimedRole);
        GameEventLog.AppendGlobal(playerHasRole
            ? LocalizationHelper.Loc("Reveal player had {0}", LocalizationHelper.Loc(lastClaimedRole.cardName))
            : LocalizationHelper.Loc("Reveal player had not {0}", LocalizationHelper.Loc(lastClaimedRole.cardName)));
        ChallengeButton.Instance?.HideChallenge();
        yield return new WaitForSeconds(challengeResultDelay);
        if (playerHasRole)
        {
            TryLoseRandomCard(false);
            GameEventLog.AppendGlobal(LocalizationHelper.Loc("Challenge player truthful"));
            ExecuteClaimedRoleAbilities(true);
        }
        else
        {
            TryLoseRandomCard(true);
            GameEventLog.AppendGlobal(LocalizationHelper.Loc("Challenge player lied"));
        }
        CheckRoundEnd();
    }

    public void ResolveOpponentChallenge(bool revealClaimed) => StartCoroutine(ResolveOpponentChallengeCoroutine(revealClaimed));

    private IEnumerator ResolveOpponentChallengeCoroutine(bool revealClaimed)
    {
        if (!revealClaimed || lastClaimedRole == null) yield break;
        bool opponentHasRole = OpponentHasRoleInHand(lastClaimedRole);
        GameEventLog.AppendGlobal(opponentHasRole
            ? LocalizationHelper.Loc("Reveal opponent had {0}", LocalizationHelper.Loc(lastClaimedRole.cardName))
            : LocalizationHelper.Loc("Reveal opponent had not {0}", LocalizationHelper.Loc(lastClaimedRole.cardName)));
        ChallengeButton.Instance?.HideChallenge();
        yield return new WaitForSeconds(challengeResultDelay);
        if (opponentHasRole)
        {
            TryLoseRandomCard(true);
            GameEventLog.AppendGlobal(LocalizationHelper.Loc("Challenge opponent truthful"));
            ExecuteClaimedRoleAbilities(false);
        }
        else
        {
            TryLoseRandomCard(false);
            GameEventLog.AppendGlobal(LocalizationHelper.Loc("Challenge opponent lied"));
        }
        FinalizePendingOpponentPlay();
        CheckRoundEnd();
    }

    private bool PlayerHasRoleInHand(CardDefiner role)
    {
        if (role == null || HandManager.Instance == null) return false;
        foreach (var card in HandManager.Instance.playerHandCards)
        {
            if (card == null) continue;
            var d = card.GetComponent<CardDisplay>();
            if (d?.card != null && d.card.cardId == role.cardId) return true;
        }
        return false;
    }

    private bool OpponentHasRoleInHand(CardDefiner role)
    {
        if (role == null || HandManager.Instance == null) return false;
        foreach (var card in HandManager.Instance.opponentHandCards)
        {
            if (card == null) continue;
            var d = card.GetComponent<CardDisplay>();
            if (d?.card != null && d.card.cardId == role.cardId) return true;
        }
        return false;
    }

    private void ExecuteClaimedRoleAbilities(bool isPlayerActor) => StartCoroutine(ExecuteClaimedRoleAbilitiesCoroutine(isPlayerActor));

    private IEnumerator ExecuteClaimedRoleAbilitiesCoroutine(bool isPlayerActor)
    {
        if (IsMultiplayer) isPlayerActor = false;
        if (lastClaimedRole?.abilities == null || RoleAbilityManager.Instance == null) yield break;
        yield return new WaitForSeconds(abilityResolveDelay);
        foreach (var ability in lastClaimedRole.abilities)
            if (ability != null)
                RoleAbilityManager.Instance.ExecuteAbility(ability, isPlayerActor);
    }
    #endregion

    #region Opponent Claim Window
    public void BeginOpponentClaim(CardDefiner claimedRole, GameObject playedCard)
    {
        if (claimedRole == null) { Debug.LogError("BeginOpponentClaim: claimedRole is null."); return; }
        pendingOpponentPlayedCard = playedCard;
        lastClaimedRole = claimedRole;
        if (ChallengeButton.Instance == null) { ExecuteClaimedRoleAbilities(false); FinalizePendingOpponentPlay(); return; }
        ChallengeButton.Instance.ShowChallengeOpponent(claimedRole.cardName);
        if (opponentChallengeWindowRoutine != null) StopCoroutine(opponentChallengeWindowRoutine);
        opponentChallengeWindowRoutine = StartCoroutine(OpponentClaimChallengeWindow());
    }

    private IEnumerator OpponentClaimChallengeWindow()
    {
        yield return new WaitForSecondsRealtime(3f);
        ChallengeButton.Instance?.HideChallenge();
        ExecuteClaimedRoleAbilities(false);
        FinalizePendingOpponentPlay();
    }

    private void FinalizePendingOpponentPlay() => pendingOpponentPlayedCard = null;
    #endregion

    #region Card Loss Management
    public bool TryLoseRandomCard(bool isPlayer)
    {
        if (HandManager.Instance == null) return false;
        var hand = isPlayer ? HandManager.Instance.playerHandCards : HandManager.Instance.opponentHandCards;
        if (hand == null || hand.Count == 0) return false;
        var card = HandManager.Instance.GetRandomCard(isPlayer);
        if (card == null) return false;
        HandManager.Instance.RemoveCardFromHand(card, isPlayer);
        return true;
    }
    #endregion

    #region Animations
    public void GameOver(bool playerWon)
    {
        if (isMatchOver) return;
        isMatchOver = true;
        isPlayerTurn = false;
        pendingOpponentPlayedCard = null;
        multiplayerSequenceRunning = false;
        ClaimMenu.Instance?.DisableClaimButton();
        if (opponentChallengeWindowRoutine != null) { StopCoroutine(opponentChallengeWindowRoutine); opponentChallengeWindowRoutine = null; }
        StopPlayerTurnTimers();
        StartCoroutine(ShowMatchResultAfterEndRoundAnimation(playerWon));
    }

    private IEnumerator ShowMatchResultAfterEndRoundAnimation(bool playerWon)
    {
        if (endRoundAnimation != null)
        {
            bool done = false;
            void OnComplete() => done = true;
            endRoundAnimation.OnAnimationComplete += OnComplete;
            float timeout = Mathf.Max(0.1f, endRoundAnimationFallbackDelay + 0.5f);
            while (!done && timeout > 0f) { timeout -= Time.unscaledDeltaTime; yield return null; }
            endRoundAnimation.OnAnimationComplete -= OnComplete;
        }
        else yield return new WaitForSecondsRealtime(endRoundAnimationFallbackDelay);
        Time.timeScale = 0f;
        GameEventLog.AppendGlobal(LocalizationHelper.Loc(playerWon ? "Game over Victory" : "Game over Defeat"));
        ShowMatchResultUI(playerWon);
    }

    private void ShowMatchResultUI(bool playerWon)
    {
        var anim = gameOverAnimation != null ? gameOverAnimation : GameOverAnimation.Instance;
        if (anim == null) { Debug.LogWarning("GameManager: gameOverAnimation missing."); return; }
        Action showCanvas = () => anim.ShowMatchResultCanvas(playerWon, playerRoundWins, opponentRoundWins);
        if (playerWon) anim.ShowVictory(showCanvas);
        else anim.ShowDefeat(showCanvas);
    }

    public void ReplayGame()
    {
        Time.timeScale = 1f;
        StopAllCoroutines();
        var anim = gameOverAnimation != null ? gameOverAnimation : GameOverAnimation.Instance;
        anim?.StopAnimation(); anim?.HideMatchResultCanvas();

        isMatchOver = false; isResettingRound = false; isPlayerTurn = true;
        currentRound = 1; playerRoundWins = 0; opponentRoundWins = 0;
        lastClaimedRole = null;
        awaitingPlayerAction = false;
        pendingOpponentPlayedCard = null; opponentChallengeWindowRoutine = null;
        multiplayerSequenceRunning = false; multiplayerChallengeIssued = false;

        UpdateScoreUI(); UpdateTurnUI();
        if (roundText != null) roundText.text = LocalizationHelper.Loc("Round {0}", currentRound);
        RoleAbilityManager.Instance?.ResetCoinsForNewRound();
        HandManager.Instance?.ClearHands();
        DeckManager.Instance?.ResetDeck();
        ClaimMenu.Instance?.UnlockClaimButton();
        ClaimMenu.Instance?.SetClaimButtonVisible(true);
        StartPlayerTurnTimers();
        StartCoroutine(ReplayStartCoroutine());
    }

    private IEnumerator ReplayStartCoroutine()
    {
        if (DeckManager.Instance != null)
            yield return StartCoroutine(DeckManager.Instance.DealHandsCoroutine());
        yield return new WaitForSeconds(roundStartDelay);
        StartCoroutine(GameLoop());
    }

    private void PlayEndRoundAnimation(int roundNumber)
    {
        var anim = endRoundAnimation != null ? endRoundAnimation : EndRoundAnimation.Instance;
        if (anim != null) anim.Play(roundNumber);
        else Debug.LogWarning("GameManager: EndRoundAnimation missing.");
    }

    public void OnPlayAgainClicked()
    {
        if (GameManager.Instance == null) { Debug.LogError("GameManager.Instance is null."); return; }
        GameManager.Instance.ReplayGame();
    }
    #endregion
}