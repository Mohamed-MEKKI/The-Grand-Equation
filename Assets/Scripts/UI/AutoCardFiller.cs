using UnityEngine;

[ExecuteInEditMode]
public class CardAutoFiller : MonoBehaviour
{
    [Header("Type the ID → it auto-fills!")]
    public int cardId = 0;

    private CardDisplay cardDisplay;

    private void OnValidate()
    {
        // This runs in Editor when you change the number
        if (cardId < 0) cardId = 0;
        if (cardId >= CardDatabase.cardList.Count) cardId = CardDatabase.cardList.Count - 1;

        UpdateCardFromId();
    }

    private void Awake()
    {
        // Also fill at runtime (for drawn cards)
        UpdateCardFromId();
    }

    public void UpdateFromId()
    {
        cardDisplay = GetComponent<CardDisplay>();
        if (cardDisplay == null) return;

        CardDefiner def = CardDatabase.cardList[cardId];
        cardDisplay.Setup(def);  // This fills name, art, abilities, etc.
    }

    void UpdateCardFromId() => UpdateFromId(); // Editor alias
}