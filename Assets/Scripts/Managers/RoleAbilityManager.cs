using System.Collections.Generic;
using UnityEngine;

public class RoleAbilityManager : MonoBehaviour
{
    public static RoleAbilityManager Instance { get; private set; }

    [Header("Game State")]
    public int playerCoins = 2;
    public int opponentCoins = 2;
    public List<PlayedCard> playerRoles = new List<PlayedCard>();
    public List<PlayedCard> opponentRoles = new List<PlayedCard>();



    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void ExecuteAbility(RoleAbility ability, bool isPlayer)
    {
        if (ability == null)
        {
            Debug.LogError("ExecuteAbility: ability parameter is null!");
            return;
        }

        switch (ability.type)
        {
            case AbilityType.BlockAssassination:
                Debug.Log($"🛡️ Assassination BLOCKED by General! ({(isPlayer ? "Player" : "Opponent")})");
                break;

            case AbilityType.PredictRole:
                // Show guess panel for player to select a role
                if (isPlayer)
                {
                    ShowGuessPanel();
                }
                else
                {
                    // AI opponent can randomly predict
                    PredictRoleAI();
                }
                break;

            case AbilityType.PeekOtherCard:
                // Reveal a random opponent role
                RevealRandomOpponentRole(isPlayer);
                break;

            case AbilityType.StealCoins:
                StealCoinsFromOpponent(ability.value, isPlayer);
                break;

            case AbilityType.TaxAllPlayers:
                TaxAllPlayers(ability.value, isPlayer);
                break;

            case AbilityType.SwapCards:
                ChangeCards(isPlayer);
                break;

            case AbilityType.Assassinate:
                Assassinate(ability.value, isPlayer);
                break;

            case AbilityType.EliminateCard:
                EliminateCard(isPlayer);
                break;
        }
    }

    // REAL ACTIONS:
    void RevealRandomOpponentRole(bool isPlayer)
    {
        // Reveal opponent's role (opposite of who is using the ability)
        var targetRoles = isPlayer ? opponentRoles : playerRoles;
        if (targetRoles.Count == 0) return;
        PlayedCard randomRole = targetRoles[Random.Range(0, targetRoles.Count)];
        randomRole.Reveal();
    }

    void StealCoinsFromOpponent(int amount, bool isPlayer)
    {
        // Steal from opponent (opposite of who is using the ability)
        int sourceCoins = isPlayer ? opponentCoins : playerCoins;
        int targetCoins = isPlayer ? playerCoins : opponentCoins;

        int stolen = Mathf.Min(amount, sourceCoins);

        if (isPlayer == true && opponentCoins >0)
        {
            opponentCoins -= stolen;
            playerCoins += stolen;
        }
        else if (isPlayer == false && playerCoins > 0)
        {
            playerCoins -= stolen;
            opponentCoins += stolen;
        }

        UpdateCoinUI();
        Debug.Log($"💰 {(isPlayer ? "Player" : "Opponent")} stole {stolen} coins! Player: {playerCoins} | Opponent: {opponentCoins}");
    }

    void TaxAllPlayers(int amount, bool isPlayer)
    {
        // Tax affects the player who uses the ability
        
        if (isPlayer == true && opponentCoins >0)
        {
            playerCoins += amount;
            opponentCoins -= amount;
        }
        else if (isPlayer == false && playerCoins > 0)
        {
            playerCoins = amount;
            opponentCoins += amount;
        }
        UpdateCoinUI();
        Debug.Log($"💰 {(isPlayer ? "Player" : "Opponent")} gained {amount} coins from tax! Player: {playerCoins} | Opponent: {opponentCoins}");
    }

    void ChangeCards(bool isPlayer)
    {
        if (!isPlayer)
        {
            // Opponent AI can swap cards automatically
            if (HandManager.Instance.opponentHandCards.Count > 0)
            {
                int opponentRandomIndex = Random.Range(0, HandManager.Instance.opponentHandCards.Count);
                GameObject targCard = HandManager.Instance.opponentHandCards[opponentRandomIndex];
                HandManager.Instance.RemoveCardFromHand(targCard, false);
                DeckManager.Instance.DrawToHand(false);
            }
            return;
        }

        // Player needs to select a card manually
        if (!GameManager.Instance.isPlayerTurn)
        {
            Debug.Log("❌ Not your turn!");
            return;
        }

        if (HandManager.Instance.playerHandCards.Count == 0) return;

        int playerRandomIndex = Random.Range(0, HandManager.Instance.playerHandCards.Count);
        GameObject targetCard = HandManager.Instance.playerHandCards[playerRandomIndex];
        HandManager.Instance.RemoveCardFromHand(targetCard, true);
        DeckManager.Instance.DrawToHand(true);
    }

    void Assassinate(int cost, bool isPlayer)
    {
        // Check if the actor has enough coins
        int actorCoins = isPlayer ? playerCoins : opponentCoins;
        if (actorCoins < cost)
        {
            Debug.Log($"❌ {(isPlayer ? "Player" : "Opponent")} doesn't have enough coins to assassinate! (Need {cost}, have {actorCoins})");
            return;
        }

        // Deduct coins from the actor
        if (isPlayer)
        {
            playerCoins -= cost;
        }
        else
        {
            opponentCoins -= cost;
        }
        UpdateCoinUI();

        // Assassinate opponent's card (opposite of who is using the ability)
        var targetHand = isPlayer ? HandManager.Instance.opponentHandCards : HandManager.Instance.playerHandCards;
        if (targetHand.Count == 0)
        {
            Debug.Log($"No {(isPlayer ? "opponent" : "player")} cards to assassinate!");
            return;
        }

        int randomIndex = Random.Range(0, targetHand.Count);
        GameObject targetCard = targetHand[randomIndex];

        Debug.Log($"🗡️ {(isPlayer ? "Player" : "Opponent")} ASSASSINATED! {(isPlayer ? "Opponent" : "Player")} loses a card!");

        HandManager.Instance.RemoveCardFromHand(targetCard, !isPlayer);  // !isPlayer = target is opponent
    }

    void EliminateCard(bool isPlayer)
    {
        if (!isPlayer)
        {
            // Opponent AI can eliminate a random card
            if (HandManager.Instance.opponentHandCards.Count > 0)
            {
                int randomIndex = Random.Range(0, HandManager.Instance.opponentHandCards.Count);
                GameObject targetCard = HandManager.Instance.opponentHandCards[randomIndex];
                HandManager.Instance.RemoveCardFromHand(targetCard, false);
                Debug.Log("Opponent eliminated a card!");
            }
            return;
        }

        // Player needs to select a card manually
        if (!GameManager.Instance.isPlayerTurn)
        {
            Debug.Log("❌ Not your turn!");
            return;
        }
        HandManager.Instance.EnterEliminationMode();  // ← Starts the process
    }

    void UpdateCoinUI()
    {
        // Update your Scoreboard
        MatchScoreboard.PlayerInstance.AddCredits(playerCoins);
        MatchScoreboard.OpponentInstance.AddCredits(opponentCoins);
    }

    // ROLE PREDICTION METHODS:
    void ShowGuessPanel()
    {
        if (!GameManager.Instance.isPlayerTurn)
        {
            Debug.Log("❌ Not your turn!");
            return;
        }

        if (GuessMenu.GetInstance() == null)
        {
            Debug.LogError("GuessMenu instance not found! Make sure GuessMenu script exists in the scene.");
            return;
        }

        GuessMenu.GetInstance().ShowGuessMenu();
        Debug.Log("🔮 Guess panel opened - Select a role to predict!");
    }

    public void PredictRole(string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
        {
            Debug.LogError("PredictRole: roleName is null or empty!");
            return;
        }

        // Find the role in the database
        CardDefiner predictedRole = FindRoleInDatabase(roleName);
        if (predictedRole == null)
        {
            Debug.LogWarning($"PredictRole: Role '{roleName}' not found in CardDatabase!");
            return;
        }

        // Check if opponent has this role in their hand
        bool opponentHasRole = OpponentHasRoleInHand(predictedRole);

        if (opponentHasRole)
        {
            Debug.Log($"✅ CORRECT! Opponent has {roleName} in their hand!");
            // Reveal the role card
            int opponentRandomIndex = Random.Range(0, HandManager.Instance.opponentHandCards.Count);
            GameObject targetCard = HandManager.Instance.opponentHandCards[opponentRandomIndex];
            HandManager.Instance.RemoveCardFromHand(targetCard, true);
            DeckManager.Instance.DrawToHand(true);
        }
        else
        {
            Debug.Log($"❌ WRONG! Opponent does NOT have {roleName} in their hand.");
        }
    }

    void PredictRoleAI()
    {
        // AI opponent randomly predicts a role
        if (CardDatabase.cardList == null || CardDatabase.cardList.Count == 0)
        {
            Debug.LogError("PredictRoleAI: CardDatabase is empty!");
            return;
        }

        // Get a random role from database (skip empty placeholder at index 0)
        int randomIndex = Random.Range(1, CardDatabase.cardList.Count);
        CardDefiner randomRole = CardDatabase.cardList[randomIndex];

        bool playerHasRole = PlayerHasRoleInHand(randomRole);

        if (playerHasRole)
        {
            Debug.Log($"🤖 AI correctly predicted you have {randomRole.cardName}!");
            RevealPlayerRoleCard(randomRole);
        }
        else
        {
            Debug.Log($"🤖 AI incorrectly predicted you have {randomRole.cardName}.");
        }
    }

    bool OpponentHasRoleInHand(CardDefiner role)
    {
        if (role == null || HandManager.Instance == null)
        {
            return false;
        }

        foreach (var card in HandManager.Instance.opponentHandCards)
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

    bool PlayerHasRoleInHand(CardDefiner role)
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

    void RevealOpponentRoleCard(CardDefiner role)
    {
        if (HandManager.Instance == null || role == null) return;

        // Find and reveal the opponent's card with this role
        foreach (var card in HandManager.Instance.opponentHandCards)
        {
            if (card == null) continue;

            CardDisplay display = card.GetComponent<CardDisplay>();
            if (display != null && display.card != null && display.card.cardId == role.cardId)
            {
                display.SetFaceUp(true);
                Debug.Log($"🔓 Revealed opponent's {role.cardName} card!");
                break;
            }
        }
    }

    void RevealPlayerRoleCard(CardDefiner role)
    {
        if (HandManager.Instance == null || role == null) return;

        // Find and reveal the player's card with this role
        foreach (var card in HandManager.Instance.playerHandCards)
        {
            if (card == null) continue;

            CardDisplay display = card.GetComponent<CardDisplay>();
            if (display != null && display.card != null && display.card.cardId == role.cardId)
            {
                display.SetFaceUp(true);
                Debug.Log($"🔓 Revealed your {role.cardName} card!");
                break;
            }
        }
    }

    CardDefiner FindRoleInDatabase(string roleName)
    {
        if (CardDatabase.cardList == null)
        {
            Debug.LogError("FindRoleInDatabase: CardDatabase.cardList is null!");
            return null;
        }

        foreach (var card in CardDatabase.cardList)
        {
            if (card != null && card.cardName != null && card.cardName == roleName)
            {
                return card;
            }
        }

        return null;
    }
}