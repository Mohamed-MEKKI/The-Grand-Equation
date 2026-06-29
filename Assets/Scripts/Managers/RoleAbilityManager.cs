using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoleAbilityManager : MonoBehaviour
{
    public static RoleAbilityManager Instance { get; private set; }

    [Header("Game State")]
    public int playerCoins = 2;
    public int opponentCoins = 2;
    public List<PlayedCard> playerRoles = new List<PlayedCard>();
    public List<PlayedCard> opponentRoles = new List<PlayedCard>();
    public Button finishHimButton;

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
        if (finishHimButton != null)
            finishHimButton.gameObject.SetActive(false);
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
#if UNITY_EDITOR
        Debug.Log($"Assassination BLOCKED by General! ({(isPlayer ? "Player" : "Opponent")})");
#endif
                GameEventLog.AppendGlobal($"{(isPlayer ? "Player" : "Opponent")} blocks assassination.");
                break;

            case AbilityType.PredictRole:
                if (isPlayer)
                {
                    ClaimMenu.Instance?.HideClaimMenu();
                    ClaimMenu.Instance?.DisableClaimButton();
                    ShowGuessPanel();
                }
                else
                {
                    PredictRoleAI();
                }
                break;

            case AbilityType.GetCoins:
                Add4coinstoPlayer(isPlayer);
                break;

            case AbilityType.PeekOtherCard:
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
        const int amount = 4;

        if (isPlayer && playerCoins < 9)
        {
            playerCoins += amount;
            RefreshCoinDisplays();
            GameEventLog.AppendGlobal("Coins: Player +4.");
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.coinSound);
        }
        else if (!isPlayer && opponentCoins < 9)
        {
            opponentCoins += amount;
            RefreshCoinDisplays();
            GameEventLog.AppendGlobal("Coins: Opponent +4.");
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.coinSound);
        }
        else
        {
            Debug.Log("You have more than the required coins.");
        }
    }

    private Coroutine _peekRoutine;
    private Coroutine _peekMultiRoutine;

    void RevealRandomOpponentRole(bool isPlayer)
    {
        if (HandManager.Instance == null) return;

        AudioManager.Instance?.PlaySFX(AudioManager.Instance.sirenSound);

        if (GameManager.Instance != null && GameManager.Instance.IsMultiplayer)
        {
            // Multiplayer — reveal one card from opponent's face-down hand
            HandManager.Instance.RevealOneCardOnly(true);
            if (_peekMultiRoutine != null) StopCoroutine(_peekMultiRoutine);
            _peekMultiRoutine = StartCoroutine(HidePeekedCardAfterDelayMultiplayer(3f));
        }
        else
        {
            // AI peek — log only, no visual reveal
            if (!isPlayer)
            {
                var validCards = HandManager.Instance.GetValidCards(true); // reusable, no alloc
                if (validCards.Count > 0)
                {
                    var peeked = validCards[UnityEngine.Random.Range(0, validCards.Count)];
                    var display = peeked.GetComponent<CardDisplay>();
                    if (display?.card != null)
                        GameEventLog.AppendGlobal("AI Peek: " + display.card.cardName + ".");
                }
                return;
            }
            // Player peek — reveal one opponent card visually
            HandManager.Instance.RevealOneCardOnly(false);
            if (_peekRoutine != null) StopCoroutine(_peekRoutine);
            _peekRoutine = StartCoroutine(HidePeekedCardAfterDelay(4f));
        }
    }

    private IEnumerator HidePeekedCardAfterDelay(float delaySeconds)
    {
        yield return new WaitForSecondsRealtime(delaySeconds);
        // Hide only opponent cards back down
        if (HandManager.Instance == null) yield break;
        foreach (var card in HandManager.Instance.opponentHandCards)
            card?.GetComponent<CardDisplay>()?.SetFaceUp(false);
    }

    private IEnumerator HidePeekedCardAfterDelayMultiplayer(float delaySeconds)
    {
        yield return new WaitForSecondsRealtime(delaySeconds);
        HandManager.Instance?.FlipAllCardsDown();
        HandManager.Instance?.RevealPlayerHandAnimated();
    }

    void StealCoinsFromOpponent(int amount, bool isPlayer)
    {
        int stolen = Mathf.Min(amount, isPlayer ? opponentCoins : playerCoins);

        if (isPlayer && opponentCoins > 1 && playerCoins < 12)
        {
            opponentCoins -= stolen;
            playerCoins += stolen;
            RefreshCoinDisplays();
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.coinSound);
            GameEventLog.AppendGlobal($"Coins: Player steals {stolen}.");
        }
        else if (!isPlayer && playerCoins > 1 && opponentCoins < 12)
        {
            playerCoins -= stolen;
            opponentCoins += stolen;
            RefreshCoinDisplays();
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.coinSound);
            GameEventLog.AppendGlobal($"Coins: Opponent steals {stolen}.");
        }
    }

    void TaxAllPlayers(int amount, bool isPlayer)
    {
        if (isPlayer && opponentCoins > 0 && playerCoins < 12)
        {
            playerCoins += amount;
            opponentCoins -= amount;
            RefreshCoinDisplays();
            GameEventLog.AppendGlobal($"Coins: Player tax +{amount} (opponent -{amount}).");
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.coinSound);
        }
        else if (!isPlayer && playerCoins > 0 && opponentCoins < 12)
        {
            playerCoins -= amount;
            opponentCoins += amount;
            RefreshCoinDisplays();
            GameEventLog.AppendGlobal($"Coins: Opponent tax +{amount} (player -{amount}).");
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.coinSound);
        }
    }

    void ChangeCards(bool isPlayer)
    {
        if (!isPlayer)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsMultiplayer)
            {
                Debug.LogWarning("ChangeCards: opponent path called in multiplayer — skipping.");
                return;
            }

            if (HandManager.Instance.opponentHandCards.Count > 0)
            {
                int idx = Random.Range(0, HandManager.Instance.opponentHandCards.Count);
                GameObject targCard = HandManager.Instance.opponentHandCards[idx];
                HandManager.Instance.RemoveCardFromHand(targCard, false);
                DeckManager.Instance.DrawToHand(false);
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.swapSound);
                GameEventLog.AppendGlobal("Opponent swaps a card.");
            }
            return;
        }

        if (HandManager.Instance.playerHandCards.Count == 0) return;

        if (GameManager.Instance != null && !GameManager.Instance.isPlayerTurn)
            GameManager.Instance.isPlayerTurn = true;

        GameManager.Instance.SetAwaitingPlayerAction(true);
        HandManager.Instance.EnterSwapMode();
    }

    public void CompletePlayerSwap(GameObject selectedCard)
    {
        HandManager.Instance.RemoveCardFromHand(selectedCard, true);
        DeckManager.Instance.DrawToHand(true);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.swapSound);
        GameEventLog.AppendGlobal("Player swaps a card.");
        GameManager.Instance?.SetAwaitingPlayerAction(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.isPlayerTurn = true;
            GameManager.Instance.EndTurn();
        }
    }

    void Assassinate(int cost, bool isPlayer)
    {
        int actorCoins = isPlayer ? playerCoins : opponentCoins;
        if (actorCoins < cost)
        {
            Debug.Log($"{(isPlayer ? "Player" : "Opponent")} doesn't have enough coins to assassinate!");
            GameEventLog.AppendGlobal($"{(isPlayer ? "Player" : "Opponent")} assassination failed: not enough coins.");
            return;
        }

        if (isPlayer) playerCoins -= cost;
        else opponentCoins -= cost;
        RefreshCoinDisplays();

        var targetHand = isPlayer
            ? HandManager.Instance.opponentHandCards
            : HandManager.Instance.playerHandCards;

        if (targetHand.Count == 0)
        {
            GameEventLog.AppendGlobal($"{(isPlayer ? "Player" : "Opponent")} assassination: no target.");
            return;
        }

        int randomIndex = Random.Range(0, targetHand.Count);
        GameObject targetCard = targetHand[randomIndex];
        GameEventLog.AppendGlobal($"{(isPlayer ? "Player" : "Opponent")} assassinates: target loses a card.");
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.stabSound);
        HandManager.Instance.RemoveCardFromHand(targetCard, !isPlayer);
    }

    public void HideAssassinateButton()
    {
        if (finishHimButton != null)
            finishHimButton.gameObject.SetActive(false);
    }

    public void ShowAssassinateButton()
    {
        if (finishHimButton == null) return;

        const int assassinateCost = 7;
        bool shouldShow = playerCoins >= assassinateCost
                          && GameManager.Instance != null
                          && GameManager.Instance.isPlayerTurn;

        finishHimButton.gameObject.SetActive(shouldShow);
    }

    public void OnClickAssassinateOpponent()
    {
        if (GameManager.Instance == null || HandManager.Instance == null)
        {
            Debug.LogError("OnClickAssassinateOpponent: Missing GameManager or HandManager instance.");
            return;
        }

        if (!GameManager.Instance.isPlayerTurn)
        {
            Debug.Log("Not your turn!");
            return;
        }

        if (HandManager.Instance.opponentHandCards == null || HandManager.Instance.opponentHandCards.Count == 0)
        {
            GameEventLog.AppendGlobal("Assassinate canceled: no opponent target.");
            return;
        }

        const int assassinateCost = 7;
        if (playerCoins < assassinateCost)
        {
            GameEventLog.AppendGlobal("Assassinate canceled: not enough coins.");
            return;
        }

        Assassinate(assassinateCost, true);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.bulletSound);
        GameManager.Instance.EndTurn();
    }

    void EliminateCard(bool isPlayer)
    {
        if (!isPlayer)
        {
            if (HandManager.Instance.opponentHandCards.Count > 0)
            {
                int randomIndex = Random.Range(0, HandManager.Instance.opponentHandCards.Count);
                GameObject targetCard = HandManager.Instance.opponentHandCards[randomIndex];
                HandManager.Instance.RemoveCardFromHand(targetCard, false);
                GameEventLog.AppendGlobal("Opponent eliminates a card.");
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.bulletSound);
            }
            return;
        }

        HandManager.Instance.EnterEliminationMode();
        GameEventLog.AppendGlobal("Player is selecting a card to eliminate.");
    }

    public void SwapHandCoinSemanticsForLocalMultiplayer()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsMultiplayer)
            return;

        int t = playerCoins;
        playerCoins = opponentCoins;
        opponentCoins = t;
    }

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

        ShowAssassinateButton();
    }

    public void ResetCoinsForNewRound()
    {
        playerCoins = 2;
        opponentCoins = 2;
        RefreshCoinDisplays();
    }

    void ShowGuessPanel()
    {
        if (!GameManager.Instance.isPlayerTurn)
        {
            Debug.Log("Not your turn!");
            return;
        }

        if (GuessMenu.Instance == null)
        {
            Debug.LogError("GuessMenu instance not found!");
            return;
        }

        GuessMenu.Instance.ShowGuessMenu();
    }

    public void PredictRole(string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
        {
            Debug.LogError("PredictRole: roleName is null or empty!");
            return;
        }

        CardDefiner predictedRole = FindRoleInDatabase(roleName);
        if (predictedRole == null)
        {
            Debug.LogWarning($"PredictRole: Role '{roleName}' not found!");
            return;
        }

        bool opponentHasRole = OpponentHasRoleInHand(predictedRole);

        if (opponentHasRole && HandManager.Instance.opponentHandCards.Count > 0)
        {
            GameEventLog.AppendGlobal($"Prediction correct: opponent had {roleName}.");
            int idx = Random.Range(0, HandManager.Instance.opponentHandCards.Count);
            GameObject targetCard = HandManager.Instance.opponentHandCards[idx];
            HandManager.Instance.RemoveCardFromHand(targetCard, false);
        }
        else
        {
            GameEventLog.AppendGlobal($"Prediction wrong: opponent did not have {roleName}.");
            int amount = 4;
            if ((12 - opponentCoins > 4) && (playerCoins > 4))
            {
                opponentCoins += amount;
                playerCoins -= amount;
                RefreshCoinDisplays();
            }
        }
        GameManager.Instance.EndTurn();
    }

    void PredictRoleAI()
    {
        if (CardDatabase.cardList == null || CardDatabase.cardList.Count == 0)
        {
            Debug.LogError("PredictRoleAI: CardDatabase is empty!");
            return;
        }

        int randomIndex = Random.Range(1, CardDatabase.cardList.Count);
        CardDefiner randomRole = CardDatabase.cardList[randomIndex];
        bool playerHasRole = PlayerHasRoleInHand(randomRole);

        if (playerHasRole && HandManager.Instance.playerHandCards.Count > 0)
        {
            GameEventLog.AppendGlobal($"AI prediction correct: player had {randomRole.cardName}.");
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.areaSound);
            int idx = Random.Range(0, HandManager.Instance.playerHandCards.Count);
            GameObject targetCard = HandManager.Instance.playerHandCards[idx];
            HandManager.Instance.RemoveCardFromHand(targetCard, true);
        }
        else
        {
            GameEventLog.AppendGlobal($"AI prediction wrong: player did not have {randomRole.cardName}.");
            int amount = 4;
            if ((12 - playerCoins > 4) && (opponentCoins > 4))
            {
                opponentCoins -= amount;
                playerCoins += amount;
                RefreshCoinDisplays();
            }
        }
    }

    bool OpponentHasRoleInHand(CardDefiner role)
    {
        if (role == null || HandManager.Instance == null) return false;

        foreach (var card in HandManager.Instance.opponentHandCards)
        {
            if (card == null) continue;
            CardDisplay display = card.GetComponent<CardDisplay>();
            if (display != null && display.card != null && display.card.cardId == role.cardId)
                return true;
        }
        return false;
    }

    bool PlayerHasRoleInHand(CardDefiner role)
    {
        if (role == null || HandManager.Instance == null) return false;

        foreach (var card in HandManager.Instance.playerHandCards)
        {
            if (card == null) continue;
            CardDisplay display = card.GetComponent<CardDisplay>();
            if (display != null && display.card != null && display.card.cardId == role.cardId)
                return true;
        }
        return false;
    }

    CardDefiner FindRoleInDatabase(string roleName)
    {
        return CardDatabase.FindByName(roleName);
    }
}