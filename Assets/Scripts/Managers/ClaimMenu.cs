using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ClaimMenu : MonoBehaviour
{
    public static ClaimMenu Instance;

    [Header("UI")]
    public GameObject claimMenuPanel;
    public Button claimMenuButton;

    public bool isclicked = false;
    private bool claimButtonLocked = false;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            HideClaimMenu();
    }

    private void Awake()
    {
        Instance = this;
        HideClaimMenu();
    }

    public void OnClaimRoleButtonClicked()
    {
        if (isclicked) HideClaimMenu();
        else ShowClaimMenu();
    }

    public void ShowClaimMenu()
    {
        isclicked = true;
        if (claimMenuPanel != null)
            claimMenuPanel.SetActive(true);
    }

    public void HideClaimMenu()
    {
        isclicked = false;
        if (claimMenuPanel != null)
            claimMenuPanel.SetActive(false);
    }

    public void DisableClaimButton()
    {
        claimButtonLocked = true;
        SetClaimButtonVisible(false);
    }

    public void UnlockClaimButton()
    {
        claimButtonLocked = false;
    }

    public void SetClaimButtonVisible(bool isVisible)
    {
        if (isVisible && claimButtonLocked) return;

        if (claimMenuButton != null)
            claimMenuButton.gameObject.SetActive(isVisible);

        if (!isVisible)
            HideClaimMenu();
    }

    // CONNECT THESE TO YOUR BUTTONS IN INSPECTOR
    public void ClaimDuke() => Claim("National Guard");
    public void ClaimThief() => Claim("Thief");
    public void ClaimAssassin() => Claim("Assassin");
    public void ClaimFiscality() => Claim("Fiscality");
    public void ClaimGeneral() => Claim("General");
    public void ClaimDeputy() => Claim("Deputy");
    public void ClaimBoss() => Claim("Boss");

    private void Claim(string roleName)
    {
        GameManager.Instance.ClaimRoleByName(roleName);
        HideClaimMenu();
    }
}