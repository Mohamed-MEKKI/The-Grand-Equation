using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Managers")]
    public DeckManager deckManager;
    public HandManager handManager;

    [Header("Game State")]
    public int playerHealth = 30;
    public int opponentHealth = 30;
    public int playerMana = 1;
    public int maxMana = 10;
    public bool isPlayerTurn = true;

    [Header("UI")]
    public TMPro.TextMeshProUGUI playerManaText;
    public TMPro.TextMeshProUGUI playerHealthText;

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
        while (playerHealth > 0 && opponentHealth > 0)
        {
            yield return StartCoroutine(PlayerTurn());
            if (opponentHealth <= 0) yield break;

            yield return StartCoroutine(OpponentTurn());
            if (playerHealth <= 0) yield break;
        }

        EndGame();
    }

    IEnumerator PlayerTurn()
    {
        isPlayerTurn = true;
        playerMana = Mathf.Min(playerMana + 1, maxMana);  // Mana refresh
        UpdateUI();

        // Player gets 30s to play cards
        yield return new WaitForSeconds(30f);
    }

    IEnumerator OpponentTurn()
    {
        isPlayerTurn = false;
        UpdateUI();

        // Simple AI: Play 1-2 random cards
        yield return new WaitForSeconds(2f);
        handManager.OpponentPlayRandomCard();  // If you added this
    }

    public void PlayCard(CardData card)
    {
        //if (!isPlayerTurn || playerMana < card.manaCost) return;

        //playerMana -= card.manaCost;
        UpdateUI();
        //handManager.RemoveCardFromHand(/* the dragged card */, true);

        // TODO: Spawn on battlefield
        //GameObject playedCard = Instantiate(cardPrefab, PlayZone.transform);
        //playedCard.GetComponent<CardDisplay>().Setup(card);
        //StartCoroutine(ScaleIn(playedCard));

        Debug.Log($"Played: {card.cardName}");
    }

    void UpdateUI()
    {
        playerManaText.text = $"{playerMana}/{maxMana}";
        playerHealthText.text = playerHealth.ToString();
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
        Debug.Log(playerHealth > 0 ? "Player Wins!" : "Opponent Wins!");
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
