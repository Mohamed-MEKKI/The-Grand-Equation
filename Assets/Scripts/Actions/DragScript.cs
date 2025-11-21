using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private Transform originalParent;

    private void Awake()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        canvas = canvases.Length > 0 ? canvases[0] : null;
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        // Safety: make sure CanvasGroup exists
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!GameManager.Instance.isPlayerTurn) return;

        originalPosition = rectTransform.position;
        originalParent = transform.parent;

        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;

        // Bring to front
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!GameManager.Instance.isPlayerTurn) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // SAFETY: Check if HandManager exists
        if (HandManager.Instance == null)
        {
            Debug.LogError("HandManager.Instance is NULL! Did you forget to add HandManager to the scene?");
            ResetCardPosition();
            return;
        }

        // SUCCESS: Drop anywhere = add to hand (no PlayZone needed)
       
        HandManager.Instance.AddCardToHand(gameObject, true); // true = player
  


        // Optional: rearrange hand
        HandManager.Instance.ArrangeHand(true);

        // If card was dragged from deck → keep it in hand
        // If it was already in hand → it just snaps back nicely
    }

    private void ResetCardPosition()
    {
        rectTransform.position = originalPosition;
        if (originalParent != null)
            transform.SetParent(originalParent);
    }
}
