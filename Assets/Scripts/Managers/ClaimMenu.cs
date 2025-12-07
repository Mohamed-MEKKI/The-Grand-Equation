using UnityEngine;
using UnityEngine.UI;

public class ClaimMenu : MonoBehaviour
{
    private static ClaimMenu Instance;

    [Header("UI")]
    public GameObject claimMenuPanel;
    public Button claimMenuButton;  // Drag your pause button here

    public bool isclicked = false;
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
    public void ClaimDuke() => Claim("Duke");
    public void ClaimThief() => Claim("Thief");
    public void ClaimAssassin() => Claim("Assassin");
    public void ClaimFiscality() => Claim("Fiscality");
    public void ClaimGeneral() => Claim("General");
    public void ClaimDeputy() => Claim("Deputy");
    public void ClaimBoss() => Claim("Boss");

    private void Claim(string roleName)
    {
        GameManager.Instance.ClaimRoleByName(roleName);
    }
}