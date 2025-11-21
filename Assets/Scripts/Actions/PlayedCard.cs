using UnityEngine;

public class PlayedCard : MonoBehaviour
{
    public CardDefiner cardDef;
    public bool isRevealed = false;
    public bool isDead = false;

    public void Setup(CardDefiner def)
    {
        cardDef = def;
        GetComponent<CardDisplay>().Setup(def);
        FlipToFaceDown();  // Start face down (hidden role)
    }

    public void Reveal()
    {
        isRevealed = true;
        FlipToFaceUp();
        Debug.Log($"🔓 {cardDef.cardName} REVEALED!");
    }

    public void Kill()
    {
        isDead = true;
        Debug.Log($"💀 {cardDef.cardName} KILLED!");
        Destroy(gameObject, 2f);  // Remove after 2s
    }

    void FlipToFaceDown() => GetComponent<CardDisplay>().SetFaceUp(false);
    void FlipToFaceUp() => GetComponent<CardDisplay>().SetFaceUp(true);
}