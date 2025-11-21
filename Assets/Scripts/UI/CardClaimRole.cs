using UnityEngine;
using UnityEngine.EventSystems;

public class CardClaimHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!GameManager.Instance.isPlayerTurn) return;

        var display = GetComponent<CardDisplay>();
        if (display?.card == null) return;

        GameManager.Instance.ClaimRole(display.card);
    }
}