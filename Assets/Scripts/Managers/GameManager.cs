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

        // 1. Stop timer
        if (TurnTimer.Instance != null)
            TurnTimer.Instance.StopTimer();

        // 2. Opponent turn (INSTANT + FAST)
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

    public void EndPlayerTurn()
    {
        if (PauseManager.Instance.isPaused) return;
        StartCoroutine("PlayerTurn");
        StopCoroutine(OpponentTurn());
        Debug.Log("Your turn has been ended");
    }
}
