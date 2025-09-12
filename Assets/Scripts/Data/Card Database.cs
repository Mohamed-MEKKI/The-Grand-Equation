using NUnit.Framework;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor.PackageManager;
using UnityEngine;

public class CardDatabase : MonoBehaviour 
{
    public static readonly List<CardDefiner> cardList = new List<CardDefiner>
    {
        new CardDefiner { cardId = 0, cardName = " ", artwork = null, possibleActions = " ", description = " " },
        new CardDefiner { cardId = 1, cardName = "General", artwork = null, possibleActions = "Stop the assassin", description = "Protects against assassination." },
        new CardDefiner { cardId = 2, cardName = "National Gard", artwork = null, possibleActions = "Predict the role", description = "Guess another player’s role." },
        new CardDefiner { cardId = 3, cardName = "Deputy", artwork = null, possibleActions = "Sees player’s other card", description = "Look at one of another player’s cards." },
        new CardDefiner { cardId = 4, cardName = "Thief", artwork = null, possibleActions = "Steal from the player", description = "Take coins from another player." },
        new CardDefiner { cardId = 5, cardName = "Fiscality", artwork = null, possibleActions = "Tax the players", description = "Collect taxes from all players." },
        new CardDefiner { cardId = 6, cardName = "Boss", artwork = null, possibleActions = "Shuffle the cards", description = "Rearrange cards in play." },
        new CardDefiner { cardId = 7, cardName = "Assassin", artwork = null, possibleActions = "Boss take money from the bank", description = "Kill a role unless blocked." }
    };
}
/*
 * Resources.Load<Sprite>("assassin")
 */