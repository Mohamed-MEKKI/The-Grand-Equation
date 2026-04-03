using UnityEngine;
using UnityEngine.UI;


public class ChallengeButton : MonoBehaviour
{
    public Button btn;
    public static ChallengeButton Instance;
    private bool challengeOpponent;

    void Awake()
    {
        Instance = this;
        // Prefer the inspector assignment; fall back to local/child Button components.
        if (btn == null)
            btn = GetComponent<Button>();
        if (btn == null)
            btn = GetComponentInChildren<Button>(true);

        if (btn == null)
        {
            Debug.LogError("ChallengeButton: No UnityEngine.UI.Button found on this object/children. Clicks will not work.");
            return;
        }

        // Avoid stacking multiple listeners when scenes reload.
        btn.onClick.RemoveListener(OnChallenge);
        btn.onClick.AddListener(OnChallenge);
        gameObject.SetActive(false);  // Hidden by default
    }

    public void ShowChallenge(string claimedRole)
    {
        challengeOpponent = false;
        if (btn != null) btn.interactable = true;
        btn.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = $"Challenge {claimedRole}!";
        gameObject.SetActive(true);
    }

    public void ShowChallengeOpponent(string claimedRole)
    {
        challengeOpponent = true;
        if (btn != null) btn.interactable = true;
        btn.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = $"Challenge {claimedRole}!";
        gameObject.SetActive(true);
    }

    public void HideChallenge()
    {
        gameObject.SetActive(false);
    }

    void OnChallenge()
    {
        Debug.Log("CHALLENGE ISSUED!");
        GameEventLog.AppendGlobal(challengeOpponent ? "Challenge clicked: opponent claim." : "Challenge clicked: player claim.");
        if (GameManager.Instance == null)
        {
            Debug.LogError("ChallengeButton.OnChallenge: GameManager.Instance is null.");
            HideChallenge();
            return;
        }

        if (challengeOpponent)
        {
            Debug.Log("ChallengeButton: issuing opponent challenge.");
            GameManager.Instance.OpponentChallengeIssued(); // player challenges opponent
        }
        else
        {
            Debug.Log("ChallengeButton: issuing player challenge.");
            GameManager.Instance.ResolveChallenge(true);  // true = challenged player must reveal
        }
        HideChallenge();
    }
}