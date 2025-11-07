using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rect;
    private Image outline;
    public float hoverScale = 1.3f;
    public float hoverHeight = 50f;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        outline = GetComponent<Image>();
        if (outline) outline.material = new Material(Shader.Find("UI/Default")); // Add outline shader
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rect.SetAsLastSibling(); // Bring to front
        rect.localScale = Vector3.one * hoverScale;
        rect.anchoredPosition += Vector2.up * hoverHeight;
        if (outline) outline.material.SetColor("_Color", Color.yellow); // Glow effect
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rect.localScale = Vector3.one;
        rect.anchoredPosition -= Vector2.up * hoverHeight;
        if (outline) outline.material.SetColor("_Color", Color.white);
    }
}