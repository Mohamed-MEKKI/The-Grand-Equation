using UnityEngine;
using UnityEngine.EventSystems;

public class DeckClickHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        DeckManager.Instance.DrawToHand(true);
    }
}