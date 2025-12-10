using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text nameText;
    public Image artworkImage;
    public Image[] abilityIcons;  // Array size 3 (drag from prefab)

    public CardDefiner card;  // Now CardDefiner instead of CardData
    public GameObject cardBack;


    public void Setup(CardDefiner definer)
    {

        card = definer;
        nameText.text = card.cardName;
        artworkImage.sprite = card.artwork;

        // NEW: Show abilities above card
        ShowAbilities(card.abilities);
    }

    void ShowAbilities(RoleAbility[] abilities)  // RoleAbility from your Abilities.cs
    {
        // Hide all
        for (int i = 0; i < abilityIcons.Length; i++)
            abilityIcons[i].gameObject.SetActive(false);

        // Show active ones
        for (int i = 0; i < abilities.Length && i < abilityIcons.Length; i++)
        {
            abilityIcons[i].sprite = GetAbilityIcon(abilities[i].type);
            abilityIcons[i].gameObject.SetActive(true);
        }
    }

    public void SetFaceUp(bool faceUp)
    {
        // Set face up/down state
        if (artworkImage != null)
        {
            artworkImage.gameObject.SetActive(faceUp);
        }
        else
        {
            Debug.LogWarning($"CardDisplay.SetFaceUp: artworkImage is null on card '{gameObject.name}'");
        }
        
        if (cardBack != null)
        {
            cardBack.SetActive(!faceUp);
        }
        else
        {
            Debug.LogWarning($"CardDisplay.SetFaceUp: cardBack is null on card '{gameObject.name}'");
        }
        
        // Also handle nameText visibility if it should be hidden when face down
        if (nameText != null)
        {
            nameText.gameObject.SetActive(faceUp);
        }
    }

    Sprite GetAbilityIcon(AbilityType type)
    {
        return type switch
        {
            AbilityType.BlockAssassination => Resources.Load<Sprite>("Icons/Shield"),
            AbilityType.PredictRole => Resources.Load<Sprite>("Icons/Crystal"),
            AbilityType.PeekOtherCard => Resources.Load<Sprite>("Icons/Eye"),
            AbilityType.StealCoins => Resources.Load<Sprite>("Icons/CoinSteal"),
            AbilityType.TaxAllPlayers => Resources.Load<Sprite>("Icons/Tax"),
            AbilityType.SwapCards => Resources.Load<Sprite>("Icons/Shuffle"),
            AbilityType.Assassinate => Resources.Load<Sprite>("Icons/Dagger"),
            _ => null
        };
    }
}