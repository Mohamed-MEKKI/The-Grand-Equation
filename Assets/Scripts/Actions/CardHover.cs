using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip UI")]
    public GameObject tooltipPanel;
    public TMP_Text Powers;

    private CardDisplay cardDisplay;
    private RectTransform rect;
    private Vector2 originalPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        cardDisplay = GetComponent<CardDisplay>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Hover ON");
        originalPos = rect.anchoredPosition;
        rect.anchoredPosition += Vector2.up * 50;
        rect.localScale = Vector3.one * 1.3f;
        rect.SetAsLastSibling();

        if (tooltipPanel && cardDisplay?.card != null)
        {
            Powers.text = cardDisplay.card.possibleActions;
            tooltipPanel.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Hover OFF");
        rect.anchoredPosition = originalPos;
        rect.localScale = Vector3.one;
        if (tooltipPanel) tooltipPanel.SetActive(false);
    }

    void ShowTooltip()
    {
        if (tooltipPanel == null || cardDisplay?.card == null) return;

        Powers.text = BuildTooltipText(cardDisplay.card.abilities);
        tooltipPanel.SetActive(true);
        tooltipPanel.transform.SetAsLastSibling();
    }

    void HideTooltip() => tooltipPanel?.SetActive(false);

    string BuildTooltipText(RoleAbility[] abilities)
    {
        var card = cardDisplay.card;
        if (abilities == null || abilities.Length == 0) return card.possibleActions;

        string tooltip = card.possibleActions + "\n\nAbilities:\n";
        for (int i = 0; i < abilities.Length; i++)
        {
            tooltip += "• " + abilities[i].abilityName;
            if (abilities[i].value > 0) tooltip += $" ({abilities[i].value})";
            tooltip += "\n";
        }
        return tooltip.Trim();
    }
}