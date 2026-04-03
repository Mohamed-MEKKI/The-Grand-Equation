using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ClaimMenu : MonoBehaviour
{
    public static ClaimMenu Instance;

    [Header("UI")]
    public GameObject claimMenuPanel;
    public Button claimMenuButton;  // Drag your claim menu button here

    public bool isclicked = false;

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isPlayerTurn)
            SetClaimButtonVisible(false);

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("key pressed");
            HideClaimMenu();
        }
    }
    private void Awake()
    {
        Instance = this;
    }

    public void OnClaimRoleButtonClicked()
    {
        if (isclicked) HideClaimMenu();
        else ShowClaimMenu();
    }

    public void ShowClaimMenu()
    {
        isclicked = true;
        claimMenuPanel.SetActive(true);
    }

    public void HideClaimMenu()
    {
        isclicked = false;
        claimMenuPanel.SetActive(false);
    }


    // CONNECT THESE TO YOUR BUTTONS IN INSPECTOR!
    public void ClaimDuke() => Claim("National Guard");
    public void ClaimThief() => Claim("Thief");
    public void ClaimAssassin() => Claim("Assassin");
    public void ClaimFiscality() => Claim("Fiscality");
    public void ClaimGeneral() => Claim("General");
    public void ClaimDeputy() => Claim("Deputy");
    public void ClaimBoss() => Claim("Boss");


    public void DisableClaimButton()
    {
        claimMenuButton.gameObject.SetActive(false);
    }

    public void SetClaimButtonVisible(bool isVisible)
    {
        if (claimMenuButton != null)
            claimMenuButton.gameObject.SetActive(isVisible);

        // If we hide the claim button, ensure its panel is also closed.
        if (!isVisible)
            HideClaimMenu();
    }
    private void Claim(string roleName)
    {
        // Tell the GameManager to process the claim
        GameManager.Instance.ClaimRoleByName(roleName);

        // Always hide the menu after claiming and update state consistently.
        // Removed duplicate calls and special-case that re-called ClaimRoleByName.
        HideClaimMenu();
    }
}