using UnityEngine;
using UnityEngine.UI;

public class UITurnManager : MonoBehaviour
{
    public static UITurnManager Instance { get; private set; }

    [Header("Scoreboards")]
    public RectTransform playerScoreboard;
    public RectTransform opponentScoreboard;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SwapContainers(RectTransform containerA, RectTransform containerB)
    {
        Vector2 tempPos = containerA.position;
        containerA.position = containerB.position;
        containerB.position = tempPos;
    }

    // Specific helpers that call the generic function
    public void SwapScoreboards()
    {
        SwapContainers(playerScoreboard, opponentScoreboard);
    }
}