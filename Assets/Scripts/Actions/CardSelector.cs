using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSelector : MonoBehaviour, IPointerClickHandler
{
    public HandManager.SelectionType selectionType;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (HandManager.Instance == null) return;

        switch (selectionType)
        {
            case HandManager.SelectionType.EliminateOpponentHand:
                HandleElimination();
                break;

            case HandManager.SelectionType.SwapPlayerCard:
                HandleSwap();
                break;
        }
    }

    private void HandleElimination()
    {
        if (!HandManager.Instance.opponentHandCards.Contains(gameObject))
        {
            Debug.LogWarning("Can't eliminate own card!");
            return;
        }

        Debug.Log($"ELIMINATED: {gameObject.name}");
        HandManager.Instance.opponentHandCards.Remove(gameObject);
        HandManager.Instance.ArrangeHand(false);
        HandManager.Instance.ExitEliminationMode();
        StartCoroutine(DestroyEffect());
    }

    private void HandleSwap()
    {
        if (!HandManager.Instance.playerHandCards.Contains(gameObject))
        {
            Debug.LogWarning("Can't swap opponent's card!");
            return;
        }

        Debug.Log($"SWAPPED: {gameObject.name}");
        HandManager.Instance.ExitSwapMode();
        RoleAbilityManager.Instance?.CompletePlayerSwap(gameObject);
    }

    private IEnumerator DestroyEffect()
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
