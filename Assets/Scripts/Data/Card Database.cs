using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    public static readonly List<CardDefiner> cardList = new List<CardDefiner>
    {
        // ID 0: Empty placeholder
        new CardDefiner { cardId = 0, cardName = " ", artwork = null, possibleActions = " ", description = " " },
        
        // ID 1: General (Duke/Contessa equivalent - blocks assassination)
        new CardDefiner
        {
            cardId = 1,
            cardName = "General",
            possibleActions = "Stop the assassin",
            description = "Protects against assassination.",
            abilities = new RoleAbility[]
            {
                new RoleAbility { abilityName = "Block Assassination", type = AbilityType.BlockAssassination, value = 1, target = "Assassin" }
            }
        },
        
        // ID 2: National Guard (Seer - predict roles)
        new CardDefiner
        {
            cardId = 2,
            cardName = "National Guard",
            possibleActions = "Predict the role",
            description = "Guess another player's role.",
            abilities = new RoleAbility[]
            {
                new RoleAbility { abilityName = "Predict Role", type = AbilityType.PredictRole, value = 1, target = "Player" }
            }
        },
        
        // ID 3: Deputy (Lookout - peek cards)
        new CardDefiner
        {
            cardId = 3,
            cardName = "Deputy",
            possibleActions = "Sees player's other card",
            description = "Look at one of another player's cards.",
            abilities = new RoleAbility[]
            {
                new RoleAbility { abilityName = "Peek Card", type = AbilityType.PeekOtherCard, value = 1, target = "Player" }
            }
        },
        
        // ID 4: Thief (Thief - steal coins)
        new CardDefiner
        {
            cardId = 4,
            cardName = "Thief",
            possibleActions = "Steal from the player",
            description = "Take coins from another player.",
            abilities = new RoleAbility[]
            {
                new RoleAbility { abilityName = "Steal 2 Coins", type = AbilityType.StealCoins, value = 2, target = "Player" }
            }
        },
        
        // ID 5: Fiscality (Taxman - tax all)
        new CardDefiner
        {
            cardId = 5,
            cardName = "Fiscality",
            possibleActions = "Tax the players",
            description = "Collect taxes from all players.",
            abilities = new RoleAbility[]
            {
                new RoleAbility { abilityName = "Tax All (+2 Coins)", type = AbilityType.TaxAllPlayers, value = 2, target = "All" }
            }
        },
        
        // ID 6: Boss (Ambassador - shuffle/rearrange)
        new CardDefiner
        {
            cardId = 6,
            cardName = "Boss",
            possibleActions = "Shuffle the cards",
            description = "Rearrange cards in play.",
            abilities = new RoleAbility[]
            {
                new RoleAbility { abilityName = "Shuffle Deck", type = AbilityType.ShuffleRoles, value = 0, target = "Deck" }
            }
        },
        
        // ID 7: Assassin (Assassin - kill unless blocked)
        new CardDefiner
        {
            cardId = 7,
            cardName = "Assassin",
            possibleActions = "Boss take money from the bank",
            description = "Kill a role unless blocked.",
            abilities = new RoleAbility[]
            {
                new RoleAbility { abilityName = "Assassinate (Cost: 3 Coins)", type = AbilityType.Assassinate, value = 3, target = "Player" }
            }
        }
    };
}