using System.Collections.Generic;
using UnityEngine;

public class RoleAbilityManager : MonoBehaviour
{
    public static RoleAbilityManager Instance { get; private set; }
    DeckManager deck;

    [Header("Game State")]
    public int playerCoins = 2;
    public int opponentCoins = 2;
    public List<PlayedCard> playerRoles = new List<PlayedCard>();
    public List<PlayedCard> opponentRoles = new List<PlayedCard>();



    void Awake() { Instance = this; }

    public void ExecuteAbility(RoleAbility ability)
    {
        switch (ability.type)
        {
            case AbilityType.BlockAssassination:
                Debug.Log($"🛡️ Assassination BLOCKED by General!");
                break;

            case AbilityType.PredictRole:
                RevealRandomOpponentRole();
                break;

            case AbilityType.PeekOtherCard:
                RevealRandomOpponentRole();
                break;

            case AbilityType.StealCoins:
                StealCoinsFromOpponent(ability.value);
                break;

            case AbilityType.TaxAllPlayers:
                playerCoins += ability.value;
                UpdateCoinUI();
                break;

            case AbilityType.ShuffleRoles:
                //deck.SchuffleCards();
                break;

            case AbilityType.Assassinate:
                if (playerCoins >= ability.value)
                {
                    playerCoins -= ability.value;
                    AssassinateRandomOpponentRole();
                }
                break;

            case AbilityType.EliminateCard:
                if (!GameManager.Instance.isPlayerTurn)
                {
                    Debug.Log("❌ Not your turn!");
                    return;
                }
                HandManager.Instance.EnterEliminationMode();  // ← Starts the process
                break;
        }
    }

    // REAL ACTIONS:
    void RevealRandomOpponentRole()
    {
        if (opponentRoles.Count == 0) return;
        PlayedCard randomRole = opponentRoles[Random.Range(0, opponentRoles.Count)];
        randomRole.Reveal();
    }

    void StealCoinsFromOpponent(int amount)
    {
        int stolen = Mathf.Min(amount, opponentCoins);
        opponentCoins -= stolen;
        playerCoins += stolen;
        UpdateCoinUI();
        Debug.Log($"💰 Stole {stolen} coins! Player: {playerCoins} | Opponent: {opponentCoins}");
    }

    void AssassinateRandomOpponentRole()
    {
        if (opponentRoles.Count == 0) return;
        PlayedCard target = opponentRoles[Random.Range(0, opponentRoles.Count)];
        target.Kill();
        opponentRoles.Remove(target);
    }

    void UpdateCoinUI()
    {
        // Update your Scoreboard
        MatchScoreboard.PlayerInstance.AddCredits(playerCoins);
        MatchScoreboard.OpponentInstance.AddCredits(opponentCoins);
    }
}