using System.Collections;
using TMPro;
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

    [Header("UI")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI turnText;

    private CardDefiner lastClaimedRole;
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
    }

    private IEnumerator Start()
    {
        if (DeckManager.Instance == null)
        {
            Debug.LogError("DeckManager.Instance is null. Cannot deal hands or start game loop.");
            yield break;
        }

        yield return StartCoroutine(DeckManager.Instance.DealHandsCoroutine());
        StartCoroutine(GameLoop());
    }
    #endregion

    #region Game Loop & Turn Management
    private IEnumerator GameLoop()
    {
        while (true)
        {
            yield return StartCoroutine(PlayerTurn());
            if (CheckWinCondition()) yield break;

            yield return StartCoroutine(OpponentTurn());
            if (CheckWinCondition()) yield break;

            currentRound++;
            if (roundText != null)
            {
                roundText.text = $"Round {currentRound}";
            }
        }
    }

    private IEnumerator PlayerTurn()
    {
        isPlayerTurn = true;
        UpdateTurnUI();

        if (TurnTimer.Instance != null)
        {
            TurnTimer.Instance.StartNewTurn();
        }

        // Wait until player ends their turn
        while (isPlayerTurn)
        {
            yield return null;
        }

        if (TurnTimer.Instance != null)
        {
            // Ensure the timer UI/value is restored when the turn ends
            TurnTimer.Instance.ResetTimer();
            TurnTimer.Instance.StopTimer();
        }
    }

    private IEnumerator OpponentTurn()
    {
        isPlayerTurn = false;
        UpdateTurnUI();

        if (HandManager.Instance != null)
        {
            HandManager.Instance.OpponentPlayRandomCard();
        }

        yield return new WaitForSecondsRealtime(1.5f);
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        Debug.Log("YOUR TURN");
        UpdateTurnUI();

        if (TurnTimer.Instance != null)
        {
            TurnTimer.Instance.StartNewTurn();
        }
        else
        {
            Debug.LogWarning("TurnTimer.Instance is null! Timer will not work.");
        }
    }


    public void EndTurn()
    {
        if (!isPlayerTurn) return;

        isPlayerTurn = false;
        UpdateTurnUI();

        // DO NOT TOUCH THE TIMER HERE!
        // Just stop it if it's still running (safety)
        TurnTimer.Instance?.StopTimer();
    }

    public void EndPlayerTurn() // Called by timer when time == 0
    {
        if (PauseManager.Instance != null && PauseManager.Instance.isPaused) return;

        // Only end the turn — do NOT reset timer here!
        EndTurn();

        // Optional: play "time's up" sound, flash screen, etc.
        Debug.Log("Player ran out of time!");
    }    

    private void UpdateTurnUI()
    {
        if (turnText != null)
        {
            turnText.text = isPlayerTurn ? "Your Turn" : "Opponent's Turn";
        }
    }
    #endregion

    #region Win Condition
    private bool CheckWinCondition()
    {
        if (HandManager.Instance == null)
        {
            Debug.LogWarning("HandManager.Instance is null! Cannot check win condition.");
            return false;
        }

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

    private void ShowWinScreen(string message)
    {
        Debug.Log("🎉 GAME OVER: " + message);
        
        // Determine if player won
        bool playerWon = HandManager.Instance != null && HandManager.Instance.opponentHandCards.Count == 0;
        
        // Call GameOver animation
        GameOver(playerWon);
    }
    #endregion

    #region Card Playing
    public void PlayCard(CardDefiner cardDef, GameObject cardObj)
    {
        if (cardDef == null || cardObj == null)
        {
            Debug.LogError("PlayCard: cardDef or cardObj is null!");
            return;
        }

        if (HandManager.Instance == null)
        {
            Debug.LogError("PlayCard: HandManager.Instance is null!");
            return;
        }

        // Remove from hand
        HandManager.Instance.RemoveCardFromHand(cardObj, true);

        // Spawn on playfield (FACE DOWN)
        GameObject playedCard = Instantiate(cardObj);
        PlayedCard played = playedCard.GetComponent<PlayedCard>();
        if (played != null)
        {
            played.Setup(cardDef);

            if (RoleAbilityManager.Instance != null)
            {
                RoleAbilityManager.Instance.playerRoles.Add(played);
            }
        }

        // Trigger abilities
        if (cardDef.abilities != null && RoleAbilityManager.Instance != null)
        {
            foreach (var ability in cardDef.abilities)
            {
                if (ability != null)
                {
                    RoleAbilityManager.Instance.ExecuteAbility(ability, true); // Player plays card
                }
            }
        }

        Debug.Log($"✅ Played {cardDef.cardName} → {cardDef.possibleActions}");
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

        lastClaimedRole = claimedRole;

    }

    public void ClaimRoleByName(string roleName)
    {
        // Validation
        if (CardDatabase.cardList == null)
        {
            Debug.LogError("ClaimRoleByName: CardDatabase.cardList is null! Make sure CardDatabase is initialized.");
            return;
        }

        if (string.IsNullOrEmpty(roleName))
        {
            Debug.LogError("ClaimRoleByName: Role name is null or empty!");
            return;
        }

        // Find role in database
        CardDefiner fakeRole = FindRoleInDatabase(roleName);
        if (fakeRole == null)
        {
            Debug.LogWarning($"ClaimRoleByName: Role '{roleName}' not found in CardDatabase!");
            return;
        }

        lastClaimedRole = fakeRole;
        Debug.Log($"BLUFF: You claim {roleName}");

        // Show challenge button
        if (ChallengeButton.Instance != null)
        {
            ChallengeButton.Instance.ShowChallenge(roleName);
        }
        else
        {
            Debug.LogWarning("ClaimRoleByName: ChallengeButton.Instance is null!");
        }

        // Start AI challenge decision
        StartCoroutine(AIDecideChallenge());
    }

    private CardDefiner FindRoleInDatabase(string roleName)
    {
        foreach (var card in CardDatabase.cardList)
        {
            if (card != null && card.cardName != null && card.cardName == roleName)
            {
                return card;
            }
        }
        return null;
    }
    #endregion

    #region Challenge System
    private IEnumerator AIDecideChallenge()
    {
        yield return new WaitForSeconds(1.5f);

        if (Random.value < 0.5f)
        {
            ChallengeIssued();
        }
        else
        {
            Debug.Log("Opponent lets it pass...");
            ExecuteClaimedRoleAbilities();
        }
    }

    public void ChallengeIssued()
    {
        Debug.Log("OPPONENT CHALLENGES!");
        ResolveChallenge(true);
    }

    public void ResolveChallenge(bool revealClaimed)
    {
        if (!revealClaimed || lastClaimedRole == null)
        {
            return;
        }

        bool playerHasRole = PlayerHasRoleInHand(lastClaimedRole);
        Debug.Log($"REVEAL: You {(playerHasRole ? "HAD" : "DID NOT HAVE")} {lastClaimedRole.cardName}");

        if (playerHasRole)
        {
            // Challenge failed - player had the role
            if (TryLoseRandomCard(false))
            {
                Debug.Log("You had the role! Opponent loses a card.");
            }
            ExecuteClaimedRoleAbilities();
        }
        else
        {
            // Challenge succeeded - player lied
            if (TryLoseRandomCard(true))
            {
                Debug.Log("You lied! You lose a random card.");
            }
            // NO abilities executed when challenge succeeds
        }

        if (ChallengeButton.Instance != null)
        {
            ChallengeButton.Instance.HideChallenge();
        }

        CheckWinCondition();
    }

    private bool PlayerHasRoleInHand(CardDefiner role)
    {
        if (role == null || HandManager.Instance == null)
        {
            return false;
        }

        foreach (var card in HandManager.Instance.playerHandCards)
        {
            if (card == null) continue;

            CardDisplay display = card.GetComponent<CardDisplay>();
            if (display != null && display.card != null)
            {
                if (display.card.cardId == role.cardId)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void ExecuteClaimedRoleAbilities()
    {
        if (lastClaimedRole == null)
        {
            Debug.LogWarning("ExecuteClaimedRoleAbilities: lastClaimedRole is null!");
            return;
        }

        if (lastClaimedRole.abilities == null)
        {
            Debug.LogWarning($"ExecuteClaimedRoleAbilities: Role '{lastClaimedRole.cardName}' has no abilities list!");
            return;
        }

        if (RoleAbilityManager.Instance == null)
        {
            Debug.LogError("ExecuteClaimedRoleAbilities: RoleAbilityManager.Instance is null! Make sure RoleAbilityManager exists in the scene.");
            return;
        }

        Debug.Log($"Executing abilities for claimed role: {lastClaimedRole.cardName}");
        foreach (var ability in lastClaimedRole.abilities)
        {
            if (ability != null)
            {
                RoleAbilityManager.Instance.ExecuteAbility(ability, true); // Player's claimed role
            }
        }
    }
    #endregion

    #region Card Loss Management
    /// <summary>
    /// Attempts to remove a random card from the specified player's hand.
    /// Returns true if successful, false if hand is empty or invalid.
    /// </summary>
    public bool TryLoseRandomCard(bool isPlayer)
    {
        if (HandManager.Instance == null)
        {
            Debug.LogError("TryLoseRandomCard: HandManager.Instance is null!");
            return false;
        }

        var hand = isPlayer ? HandManager.Instance.playerHandCards : HandManager.Instance.opponentHandCards;
        
        // Check if hand has cards before attempting to remove
        if (hand == null || hand.Count == 0)
        {
            string playerName = isPlayer ? "Player" : "Opponent";
            Debug.LogWarning($"TryLoseRandomCard: {playerName} has no cards to lose! (Hand count: {(hand == null ? "null" : hand.Count.ToString())})");
            return false;
        }

        GameObject randomCard = HandManager.Instance.GetRandomCard(isPlayer);
        if (randomCard == null)
        {
            Debug.LogWarning($"TryLoseRandomCard: GetRandomCard returned null for {(isPlayer ? "player" : "opponent")}!");
            return false;
        }

        HandManager.Instance.RemoveCardFromHand(randomCard, isPlayer);
        return true;
    }

    /// <summary>
    /// Legacy method - use TryLoseRandomCard instead for safety checks.
    /// </summary>
    [System.Obsolete("Use TryLoseRandomCard instead for safety checks")]
    public void LoseRandomCard(bool isPlayer)
    {
        if (!TryLoseRandomCard(isPlayer))
        {
            Debug.LogWarning($"LoseRandomCard: Failed to remove card from {(isPlayer ? "player" : "opponent")} hand.");
        }
    }
    #endregion

    #region Animations 
    public void GameOver(bool playerWon)
    {
        Time.timeScale = 0f;
        
        if (GameOverAnimation.Instance == null)
        {
            Debug.LogError("GameOverAnimation.Instance is null! Make sure GameOverAnimation exists in the scene.");
            return;
        }
        
        if (playerWon)
            GameOverAnimation.Instance.ShowVictory();
        else
            GameOverAnimation.Instance.ShowDefeat();
    }
    #endregion
}
