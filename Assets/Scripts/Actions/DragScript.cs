using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        // Find the parent canvas (needed for proper dragging)
        canvas = GetComponentInParent<Canvas>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(GetComponentInParent<Canvas>().transform);
        rectTransform.SetAsLastSibling();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        if (IsOverPlayZone(eventData))
        {
            GameManager.Instance.PlayCard(GetComponent<CardDisplay>().card);
            HandManager.Instance.RemoveCardFromHand(gameObject);
        }
        else
        {
            HandManager.Instance.ArrangeHand(true); // Snap back
        }
    }

    bool IsOverPlayZone(PointerEventData eventData)
    {
        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        return raycastResults.Any(r => r.gameObject.CompareTag("PlayZone"));
    }
    

    public void OnDrag(PointerEventData eventData)
    {
        // Move card with the mouse
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}

