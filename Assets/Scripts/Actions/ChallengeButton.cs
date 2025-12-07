using UnityEngine;
using UnityEngine.UI;


public class ChallengeButton : MonoBehaviour
{
    public Button btn;
    public static ChallengeButton Instance;

    void Awake()
    {
        Instance = this;
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnChallenge);
        gameObject.SetActive(false);  // Hidden by default
    }

    public void ShowChallenge(string claimedRole)
    {
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
        GameManager.Instance.ResolveChallenge(true);  // true = challenged player must reveal
        HideChallenge();
    }
}