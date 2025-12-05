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
        originalPos = rect.anchoredPosition;
        rect.anchoredPosition += Vector2.up * 50;
        rect.localScale = Vector3.one * 1.3f;
        rect.SetAsLastSibling();

        if (tooltipPanel && cardDisplay?.card != null)
        {
            // Use enhanced tooltip text if abilities exist, otherwise use simple text
            if (cardDisplay.card.abilities != null && cardDisplay.card.abilities.Length > 0)
            {
                Powers.text = BuildTooltipText(cardDisplay.card.abilities);
            }
            else
            {
                Powers.text = cardDisplay.card.possibleActions;
            }
            tooltipPanel.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rect.anchoredPosition = originalPos;
        rect.localScale = Vector3.one;
        if (tooltipPanel) tooltipPanel.SetActive(false);
    }

    // Consolidated tooltip methods - now integrated into OnPointerEnter/Exit
    private void ShowTooltip()
    {
        if (tooltipPanel == null || cardDisplay?.card == null) return;

        Powers.text = BuildTooltipText(cardDisplay.card.abilities);
        tooltipPanel.SetActive(true);
        tooltipPanel.transform.SetAsLastSibling();
    }

    private void HideTooltip() => tooltipPanel?.SetActive(false);

    private string BuildTooltipText(RoleAbility[] abilities)
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