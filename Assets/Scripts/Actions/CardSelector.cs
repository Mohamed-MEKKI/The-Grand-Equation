using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.tvOS;

public class CardSelector : MonoBehaviour, IPointerClickHandler
{
    // ADD THIS LINE BACK!
    public HandManager.SelectionType selectionType;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Only allow elimination mode
        if (selectionType != HandManager.SelectionType.EliminateOpponentHand) return;

        // SAFETY #2: Not in opponent hand? Ignore (can't eliminate own cards)
        if (!HandManager.Instance.opponentHandCards.Contains(gameObject))
        {
            Debug.LogWarning("Can't eliminate own card!");
            return;
        }

        if (HandManager.Instance == null)
        {
            Debug.LogError("HandManager.Instance is NULL!");
            return;
        }

        Debug.Log($"ELIMINATED: {gameObject.name}");

        HandManager.Instance.opponentHandCards.Remove(gameObject);
        HandManager.Instance.ArrangeHand(false); // opponent hand

        HandManager.Instance.ExitEliminationMode();

        StartCoroutine(DestroyEffect());
    }

    private void Eliminate()
    {
        Debug.Log($"ELIMINATED: {gameObject.name}");

        // Remove from opponent's hand list
        HandManager.Instance.opponentHandCards.Remove(gameObject);

        // Rearrange opponent's hand
        HandManager.Instance.ArrangeHand(false);  // false = opponent

        // Destroy with cool effect
        StartCoroutine(DestroyEffect());
    }

    System.Collections.IEnumerator DestroyEffect()
    {
        RectTransform rt = GetComponent<RectTransform>();
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        float t = 0;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.one * (1f + t * 3f);
            cg.alpha = 1f - t * 2.5f;
            yield return null;
        }
        Destroy(gameObject);
    }
}