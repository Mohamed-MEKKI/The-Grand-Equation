using UnityEngine;
using TMPro;

public class MatchScoreboard : MonoBehaviour
{
    public static MatchScoreboard PlayerInstance { get; private set; }
    public static MatchScoreboard OpponentInstance { get; private set; }

    public static PauseManager PauseInstance { get; private set; }

    [Header("UI References - MUST ASSIGN IN INSPECTOR")]
    public TMP_Text agentNameText;
    public TMP_Text creditsText;

    [Header("CONFIGURE IN INSPECTOR")]
    public bool isPlayerScoreboard = true; // true = Player, false = Opponent

    [Header("Player Data")]
    public string agentName = "AGENT_SPECTER";
    public int credits = 15750;

    private void Awake()
    {
        // Register as Player or Opponent
        if (isPlayerScoreboard)
            PlayerInstance = this;
        else
            OpponentInstance = this;
    }

    private void Start()
    {
        // Safety check + update
        if (agentNameText == null || creditsText == null)
        {
            Debug.LogError($"[MatchScoreboard] Missing UI references on {gameObject.name}!", this);
            return;
        }

        UpdateScoreboard();
    }

    public void UpdateScoreboard()
    {
        if (agentNameText != null)
            agentNameText.text = agentName;

        if (creditsText != null)
            creditsText.text = credits.ToString("N0");   // ← this line stays exactly the same

        // OPTIONAL – instant feedback if something is missing
        else if (creditsText == null)
            Debug.LogWarning($"[MatchScoreboard] Credits Text is not assigned on {gameObject.name}!", this);
    }

    public void AddCredits(int amount)
    {
        credits += amount;
        UpdateScoreboard();
    }


    public void OnAddCreditsButtonClicked()
    {
        // Prevent adding if game is paused or player already has 12+ credits
        if (PauseManager.Instance != null && PauseManager.Instance.isPaused) return;
        if (PlayerInstance == null) return;
        if (PlayerInstance.credits >= 12) return;   // ≥ 12 (so max 11 → 12 is blocked)

        PlayerInstance.AddCredits(1);
    }

    public void OnAddCreditsButtonClickedOpponent()
    {
        // Same checks for opponent
        if (PauseManager.Instance != null && PauseManager.Instance.isPaused) return;
        if (OpponentInstance == null) return;
        if (OpponentInstance.credits >= 12) return;

        OpponentInstance.AddCredits(1);
    }
}