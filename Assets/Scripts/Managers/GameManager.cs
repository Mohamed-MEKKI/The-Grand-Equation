using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Managers")]
    public DeckManager deckManager;
    public HandManager handManager;

    [Header("Game State")]
    public int playerCoins = 0;
    public int opponentCoins = 0;
    public int playerMana = 1;
    public int maxMana = 10;
    public bool isPlayerTurn = true;

    [Header("UI References — DRAG THESE!")]
    public TextMeshProUGUI playerCoinsText;
    public TextMeshProUGUI opponentCoinsText;
    public TextMeshProUGUI turnText;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        // 1. Mulligan / Draw hands
        yield return StartCoroutine(deckManager.StartGame());

        // 2. Player turn loop
        while (playerCoins > 0 && opponentCoins > 0)
        {
            yield return StartCoroutine(PlayerTurn());
            if (playerCoins <= 0) yield break;

            yield return StartCoroutine(OpponentTurn());
            if (opponentCoins <= 0) yield break;
        }

        EndGame();
    }

    IEnumerator PlayerTurn()
    {
        isPlayerTurn = true;
        playerMana = Mathf.Min(playerMana + 1, maxMana);  // Mana refresh
        UpdateUI();

        const float turnDuration = 50f; // 50 seconds per request
        float elapsed = 0f;

        while (elapsed < turnDuration)
        {
            // If a PauseManager exists and the game is paused, wait without advancing elapsed time.
            if (PauseManager.Instance != null && PauseManager.Instance.isPaused)
            {
                yield return null;
                continue;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Turn time expired, coroutine ends and GameLoop continues to opponent turn.
    }

    IEnumerator OpponentTurn()
    {
        isPlayerTurn = false;
        UpdateUI();

        // Simple AI: Play 1-2 random cards
        yield return new WaitForSeconds(2f);
        handManager.OpponentPlayRandomCard();  // If you added this
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

    public void UpdateUI()
    {
        // SAFETY: Prevent crash if references missing
        if (playerCoinsText != null)
            playerCoinsText.text = playerCoins.ToString();

        if (opponentCoinsText != null)
            opponentCoinsText.text = opponentCoins.ToString();

        if (turnText != null)
            turnText.text = isPlayerTurn ? "Your Turn" : "Opponent Turn";

        // Optional: warn in console if missing
        if (playerCoinsText == null) Debug.LogError("PlayerCoinsText not assigned!", this);
        if (opponentCoinsText == null) Debug.LogError("OpponentCoinsText not assigned!", this);
        if (turnText == null) Debug.LogError("TurnText not assigned!", this);
    }

    public void EndPlayerTurn()
    {
        if (PauseManager.Instance.isPaused) return;
        StartCoroutine("PlayerTurn");
        StopCoroutine(OpponentTurn());
        Debug.Log("Your turn has been ended");
    }


    void EndGame()
    {
        Debug.Log(playerCoins == 0 ? "Player Wins!" : "Opponent Wins!");
    }

    IEnumerator ScaleIn(GameObject card)
    {
        RectTransform rt = card.GetComponent<RectTransform>();
        rt.localScale = Vector3.zero;
        float t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t / 0.3f);
            yield return null;
        }
    }
}
