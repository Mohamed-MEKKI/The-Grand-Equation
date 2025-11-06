using UnityEngine;
using UnityEngine.EventSystems;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalPos;
    private RectTransform rect;

    void Awake() => rect = GetComponent<RectTransform>();

    public void OnPointerEnter(PointerEventData eventData)
    {
        originalPos = rect.anchoredPosition;
        rect.anchoredPosition += Vector2.up * 50; // Chang
        rect.localScale = Vector3.one * 1.3f;
        rect.SetAsLastSibling();  // Bring to front
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rect.anchoredPosition = originalPos;
        rect.localScale = Vector3.one;
    }
}
