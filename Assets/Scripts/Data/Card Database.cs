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
            possibleActions = "Predict another player role and Stop the assassin",
            description = "Pays 4 coins to guess other player role and Protects against assassination.",
            canBeChallenged = true,
            artwork = Resources.Load<Sprite>("Resources/general"),
            abilities = new RoleAbility[]
            {
                //new RoleAbility { abilityName = "Block Assassination", type = AbilityType.BlockAssassination, value = 1, target = "Assassin" },
                new RoleAbility { abilityName = "Predict Role", type = AbilityType.PredictRole, value = 1, target = "Player" }
            }
        },
        
        // ID 2: National Guard (Seer - predict roles)
        new CardDefiner
        {
            cardId = 2,
            cardName = "National Guard",
            possibleActions = "Peeks other players card",
            description = "He is able to see one player's random card",
            canBeChallenged = true,
            artwork = Resources.Load<Sprite>("Resources/national_guard"),
            abilities = new RoleAbility[]
            {
                new RoleAbility { abilityName = "Peek Card", type = AbilityType.PeekOtherCard, value = 1, target = "Player" }
            }
        },
        
        // ID 3: Deputy (Lookout - peek cards)
        new CardDefiner
        {
            cardId = 3,
            cardName = "Deputy",
            possibleActions = "Swap his own cards",
            description = "He is able to change one or more cards.",
            canBeChallenged = true,
            artwork = Resources.Load<Sprite>("Resources/deputy"),
            abilities = new RoleAbility[]
            {
                new RoleAbility { abilityName = "Swap Cards", type = AbilityType.SwapCards, value = 1, target = "Player" }
            }
        },
        
        // ID 4: Thief (Thief - steal coins)
        new CardDefiner
        {
            cardId = 4,
            cardName = "Thief",
            possibleActions = "Steal from the player",
            description = "Take coins from another player.",
            canBeChallenged = true,
            artwork = Resources.Load<Sprite>("Resources/robber"),
            abilities = new RoleAbility[]
            {
                new RoleAbility { abilityName = "Steal 2 Coins", type = AbilityType.StealCoins, value = 2, target = "Player" },
                //new RoleAbility { abilityName = "Bloc other Robberies", type = AbilityType.BlocRobbery, value = 2, target = "Player" }
            }
        },
        
        // ID 5: Fiscality (Taxman - tax all)
        new CardDefiner
        {
            cardId = 5,
            cardName = "Fiscality",
            possibleActions = "Tax the players",
            description = "Collect taxes from all players.",
            canBeChallenged = true,
            artwork = Resources.Load<Sprite>("Resources/fiscality"),
            abilities = new RoleAbility[]
            {
                new RoleAbility { abilityName = "Tax All (+2 Coins)", type = AbilityType.TaxAllPlayers, value = 2, target = "All" },
                //new RoleAbility { abilityName = "Stops Governmental aids", type = AbilityType.StopGovAid, value = 2, target = "All" }
            }
        },
        
        // ID 6: Boss (Ambassador - gets coins from banks)
        new CardDefiner
        {
            cardId = 6,
            cardName = "Boss",
            possibleActions =  "take money from the bank",
            description = "Takes 4 coins from the bank.",
            canBeChallenged = true,
            artwork = Resources.Load<Sprite>("Resources/boss"),
            abilities = new RoleAbility[]
            {
                new RoleAbility { abilityName = "Gets 4 coins from the bank", type = AbilityType.GetCoins, value = 0, target = "Deck" }
            }
        },
        
        // ID 7: Assassin (Assassin - kill unless blocked)
        new CardDefiner
        {
            cardId = 7,
            cardName = "Assassin",
            possibleActions = "Eliminate other player's role",
            description = "Kill a role unless blocked.",
            canBeChallenged = true,
            artwork = Resources.Load<Sprite>("Resources/assassin"),
            abilities = new RoleAbility[]
            {
                new RoleAbility { abilityName = "Assassinate (Cost: 3 Coins)", type = AbilityType.Assassinate, value = 3, target = "Player" }
            }
        }
    };
}