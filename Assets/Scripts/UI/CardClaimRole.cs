using UnityEngine;
using UnityEngine.EventSystems;

public class CardClaimHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!GameManager.Instance.isPlayerTurn) return;

        var display = GetComponent<CardDisplay>();
        if (display?.card == null) return;
        Debug.Log($"Player claims: {display.card.cardName} → {display.card.possibleActions}");

    }
}