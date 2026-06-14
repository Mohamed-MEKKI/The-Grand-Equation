using System.Collections.Generic;
using UnityEngine;

public class RoleAbilityManager : MonoBehaviour
{
    public static RoleAbilityManager Instance { get; private set; }

    [Header("Game State")]
    public int playerCoins = 2;
    public int opponentCoins = 2;
    
    public AudioSource audSource;
    public AudioClip coinSound;
    public AudioClip bulletSound;
    public AudioClip sirenSound;
    public AudioClip areaSound;
    public AudioClip swapSound;
    public AudioClip stabSound;

    public List<PlayedCard> playerRoles = new List<PlayedCard>();
    public List<PlayedCard> opponentRoles = new List<PlayedCard>();



    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
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

        GameEventLog.AppendGlobal($"{(isPlayer ? "Player" : "Opponent")} ability: {ability.type}.");

        switch (ability.type)
        {
            case AbilityType.BlockAssassination:
                Debug.Log($"🛡️ Assassination BLOCKED by General! ({(isPlayer ? "Player" : "Opponent")})");
                GameEventLog.AppendGlobal($"{(isPlayer ? "Player" : "Opponent")} blocks assassination.");
                break;

            case AbilityType.PredictRole:
                // Show guess panel for player to select a role
                if (isPlayer)
                {
                    ClaimMenu.Instance.HideClaimMenu();
                    ShowGuessPanel();
                }
                else
                {
                    // AI opponent can randomly predict
                    PredictRoleAI();
                }
                break;

            case AbilityType.GetCoins:
                Add4coinstoPlayer(isPlayer);
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

    void Add4coinstoPlayer(bool isPlayer)
    {
        // Steal from opponent (opposite of who is using the ability)
        int sourceCoins = isPlayer ? opponentCoins : playerCoins;
        int targetCoins = isPlayer ? playerCoins : opponentCoins;

        int amount = 4;

        if (isPlayer == true && playerCoins < 9)
        {
            playerCoins += 4;
            UpdateCoinUI(true, amount);
            GameEventLog.AppendGlobal("Coins: Player +4.");
            audSource.PlayOneShot(coinSound);

        }
        else if (isPlayer == false && opponentCoins < 9)
        {
            opponentCoins += 4;
            UpdateCoinUI(false, amount);
            GameEventLog.AppendGlobal("Coins: Opponent +4.");
            audSource.PlayOneShot(coinSound);

        }
        else
        {
            Debug.Log($"💰 you have more than required coins");

        }

        Debug.Log($"💰 4 coins added to account {(isPlayer ? "Player: {playerCoins} " : "Opponent: {opponentCoins}")}");
    }

    void RevealRandomOpponentRole(bool isPlayer)
    {
        // Reveal opponent's role (opposite of who is using the ability)
        var targetRoles = isPlayer ? opponentRoles : playerRoles;
        if (targetRoles.Count == 0) return;
        PlayedCard randomRole = targetRoles[Random.Range(0, targetRoles.Count)];
        if (randomRole == null) return;

        randomRole.Reveal();
        audSource.PlayOneShot(sirenSound);
        StartCoroutine(HideRevealedRoleAfterDelay(randomRole, 10f));
    }

    System.Collections.IEnumerator HideRevealedRoleAfterDelay(PlayedCard playedCard, float delaySeconds)
    {
        yield return new WaitForSecondsRealtime(delaySeconds);

        if (playedCard == null || playedCard.isDead) yield break;

        CardDisplay display = playedCard.GetComponent<CardDisplay>();
        if (display != null)
        {
            display.SetFaceUp(false);
            playedCard.isRevealed = false;
        }
    }

    void StealCoinsFromOpponent(int amount, bool isPlayer)
    {
        // Steal from opponent (opposite of who is using the ability)
        int sourceCoins = isPlayer ? opponentCoins : playerCoins;
        int targetCoins = isPlayer ? playerCoins : opponentCoins;

        int stolen = Mathf.Min(amount, sourceCoins);

        if (isPlayer == true && opponentCoins >1 && playerCoins < 12)
        {
            opponentCoins -= stolen;
            playerCoins += stolen;
            UpdateCoinUI(true, stolen);
            UpdateCoinUI(false, -stolen);
            audSource.PlayOneShot(coinSound);
            GameEventLog.AppendGlobal($"Coins: Player steals {stolen}.");

        }
        else if (isPlayer == false && playerCoins > 1 && opponentCoins < 12)
        {
            playerCoins -= stolen;
            opponentCoins += stolen;
            UpdateCoinUI(true, -stolen);
            UpdateCoinUI(false, +stolen);
            audSource.PlayOneShot(coinSound);
            GameEventLog.AppendGlobal($"Coins: Opponent steals {stolen}.");
        }

        
        Debug.Log($"💰 {(isPlayer ? "Player" : "Opponent")} stole {stolen} coins! Player: {playerCoins} | Opponent: {opponentCoins}");
    }

    void TaxAllPlayers(int amount, bool isPlayer)
    {
        // Tax affects the player who uses the ability
        
        if (isPlayer == true && opponentCoins >0 && playerCoins < 12)
        {
            playerCoins += amount;
            opponentCoins -= amount;
            UpdateCoinUI(true, amount);
            UpdateCoinUI(false, -amount);
            GameEventLog.AppendGlobal($"Coins: Player tax +{amount} (opponent -{amount}).");
            audSource.PlayOneShot(coinSound);

        }
        else if (isPlayer == false && playerCoins > 0 && opponentCoins < 12)
        {
            playerCoins -= amount;
            opponentCoins += amount;
            UpdateCoinUI(false, -amount);
            UpdateCoinUI(true, amount);
            GameEventLog.AppendGlobal($"Coins: Opponent tax +{amount} (player -{amount}).");
            audSource.PlayOneShot(coinSound);

        }

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
                audSource.PlayOneShot(swapSound);
                GameEventLog.AppendGlobal("Opponent swaps a card.");
            }
            return;
        }

        // Player picks which card to swap — defer turn end until selection is made
        if (!GameManager.Instance.isPlayerTurn)
        {
            Debug.Log("❌ Not your turn!");
            return;
        }

        if (HandManager.Instance.playerHandCards.Count == 0) return;

        GameManager.Instance.SetAwaitingPlayerAction(true);
        HandManager.Instance.EnterSwapMode();
    }

    public void CompletePlayerSwap(GameObject selectedCard)
    {
        HandManager.Instance.RemoveCardFromHand(selectedCard, true);
        DeckManager.Instance.DrawToHand(true);
        audSource.PlayOneShot(swapSound);
        GameEventLog.AppendGlobal("Player swaps a card.");
        GameManager.Instance?.SetAwaitingPlayerAction(false);
        GameManager.Instance?.EndTurn();
    }

    void Assassinate(int cost, bool isPlayer)
    {
        // Check if the actor has enough coins
        int actorCoins = isPlayer ? playerCoins : opponentCoins;
        if (actorCoins < cost)
        {
            Debug.Log($"❌ {(isPlayer ? "Player" : "Opponent")} doesn't have enough coins to assassinate! (Need {cost}, have {actorCoins})");
            GameEventLog.AppendGlobal($"{(isPlayer ? "Player" : "Opponent")} assassination failed: not enough coins.");
            return;
        }

        // Deduct coins from the actor
        if (isPlayer && playerCoins>3)
        {
            playerCoins -= cost;
            UpdateCoinUI(isPlayer, -cost);

        }
        else 
        {
            opponentCoins -= cost;
            UpdateCoinUI(false,-cost);
        }
        

        // Assassinate opponent's card (opposite of who is using the ability)
        var targetHand = isPlayer ? HandManager.Instance.opponentHandCards : HandManager.Instance.playerHandCards;
        if (targetHand.Count == 0)
        {
            Debug.Log($"No {(isPlayer ? "opponent" : "player")} cards to assassinate!");
            GameEventLog.AppendGlobal($"{(isPlayer ? "Player" : "Opponent")} assassination: no target.");
            return;
        }

        int randomIndex = Random.Range(0, targetHand.Count);
        GameObject targetCard = targetHand[randomIndex];

        Debug.Log($"🗡️ {(isPlayer ? "Player" : "Opponent")} ASSASSINATED! {(isPlayer ? "Opponent" : "Player")} loses a card!");
        GameEventLog.AppendGlobal($"{(isPlayer ? "Player" : "Opponent")} assassinates: target loses a card.");
        audSource.PlayOneShot(stabSound);

        HandManager.Instance.RemoveCardFromHand(targetCard, !isPlayer);  // !isPlayer = target is opponent
        audSource.PlayOneShot(stabSound);

    }

    // UI hook: connect the "Assassinate" button to this method.
    public void OnClickAssassinteOpponent()
    {
        if (GameManager.Instance == null || HandManager.Instance == null)
        {
            Debug.LogError("OnClickAssassinteOpponent: Missing GameManager or HandManager instance.");
            return;
        }

        if (!GameManager.Instance.isPlayerTurn)
        {
            Debug.Log("❌ Not your turn!");
            return;
        }

        if (HandManager.Instance.opponentHandCards == null || HandManager.Instance.opponentHandCards.Count == 0)
        {
            Debug.Log("No opponent cards to assassinate.");
            GameEventLog.AppendGlobal("Assassinate canceled: no opponent target.");
            return;
        }

        const int assassinateCost = 7;
        if (playerCoins < assassinateCost)
        {
            Debug.Log($"❌ Player doesn't have enough coins to assassinate! (Need {assassinateCost}, have {playerCoins})");
            GameEventLog.AppendGlobal("Assassinate canceled: not enough coins.");
            return;
        }

        Assassinate(assassinateCost, true);
        audSource.PlayOneShot(bulletSound);

        GameManager.Instance.EndTurn();
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
                GameEventLog.AppendGlobal("Opponent eliminates a card.");
                audSource.PlayOneShot(bulletSound);

            }
            return;
        }

        HandManager.Instance.EnterEliminationMode();  // ← Starts the process
        GameEventLog.AppendGlobal("Player is selecting a card to eliminate.");
    }

    void UpdateCoinUI(bool isPlayer, int amount)
    {
        _ = isPlayer;
        _ = amount;
        RefreshCoinDisplays();
    }

    /// <summary>
    /// After swapping hands in local multiplayer, swap stored coin totals so
    /// <see cref="playerCoins"/> always tracks the bottom hand (active human) and <see cref="opponentCoins"/> the top hand.
    /// </summary>
    public void SwapHandCoinSemanticsForLocalMultiplayer()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsMultiplayer)
            return;

        int t = playerCoins;
        playerCoins = opponentCoins;
        opponentCoins = t;
    }

    /// <summary>
    /// Pushes current <see cref="playerCoins"/> / <see cref="opponentCoins"/> to match scoreboards.
    /// In pass-the-device mode, scoreboard <i>widgets</i> swap screen sides while names stay fixed — invert mapping when needed.
    /// </summary>
    public void RefreshCoinDisplays()
    {
        bool invert = GameManager.Instance != null
                      && GameManager.Instance.IsMultiplayer
                      && GameManager.Instance.LocalMultiplayerScoreboardsSwapped;

        if (invert)
        {
            if (MatchScoreboard.PlayerInstance != null)
                MatchScoreboard.PlayerInstance.SetCredits(opponentCoins);
            if (MatchScoreboard.OpponentInstance != null)
                MatchScoreboard.OpponentInstance.SetCredits(playerCoins);
        }
        else
        {
            if (MatchScoreboard.PlayerInstance != null)
                MatchScoreboard.PlayerInstance.SetCredits(playerCoins);
            if (MatchScoreboard.OpponentInstance != null)
                MatchScoreboard.OpponentInstance.SetCredits(opponentCoins);
        }
    }

    public void ResetCoinsForNewRound()
    {
        playerCoins = 2;
        opponentCoins = 2;
        UpdateCoinUI(true, 0);
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

        if (opponentHasRole && HandManager.Instance.opponentHandCards.Count>0)
        {
            Debug.Log($"✅ CORRECT! Opponent has {roleName} in their hand!");
            GameEventLog.AppendGlobal($"Prediction correct: opponent had {roleName}.");
            // Reveal the role card
            int opponentRandomIndex = Random.Range(0, HandManager.Instance.opponentHandCards.Count);
            GameObject targetCard = HandManager.Instance.opponentHandCards[opponentRandomIndex];
            HandManager.Instance.RemoveCardFromHand(targetCard, false);
        }
        else
        {
            Debug.Log($"❌ WRONG! Opponent does NOT have {roleName} in their hand.");
            GameEventLog.AppendGlobal($"Prediction wrong: opponent did not have {roleName}.");
            int amount = 4;

            if ((12 - opponentCoins > 4) && (playerCoins > 4))
            {
                opponentCoins += amount;
                playerCoins -= amount;
                UpdateCoinUI(true, amount);
                UpdateCoinUI(false, -amount);
            }
        }
        GameManager.Instance.EndTurn();
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

        if (playerHasRole && HandManager.Instance.playerHandCards.Count>0)
        {
            Debug.Log($"🤖 AI correctly predicted you have {randomRole.cardName}!");
            GameEventLog.AppendGlobal($"AI prediction correct: player had {randomRole.cardName}.");

            audSource.PlayOneShot(areaSound);

            int playerRandomIndex = Random.Range(0, HandManager.Instance.playerHandCards.Count);
            GameObject targetCard = HandManager.Instance.playerHandCards[playerRandomIndex];
            HandManager.Instance.RemoveCardFromHand(targetCard, true);
        }
        else
        {
            Debug.Log($"🤖 AI incorrectly predicted you have {randomRole.cardName}.");
            GameEventLog.AppendGlobal($"AI prediction wrong: player did not have {randomRole.cardName}.");
            int amount = 4;
            if ( (12 - playerCoins > 4)&&(opponentCoins > 4))
            {
                opponentCoins -= amount;
                playerCoins += amount;
                UpdateCoinUI(true, amount);
                UpdateCoinUI(false, -amount);
            }
            

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