using UnityEngine;
using UnityEngine.UI;

public class GuessMenu : MonoBehaviour
{
    private static GuessMenu Instance;

    [Header("UI")]
    public GameObject guessPanel;
    public Button guessMenuButton;  // Drag your guess menu button here

    public bool isClicked = false;
    
    private void Awake()
    {
        Instance = this;
    }

    public void OnGuessRoleButtonClicked()
    {
        if (isClicked) HideGuessMenu();
        else ShowGuessMenu();
    }

    public void ShowGuessMenu()
    {
        if (guessPanel == null)
        {
            Debug.LogError("GuessMenu: guessPanel is not assigned!");
            return;
        }

        isClicked = true;
        guessPanel.SetActive(true);
    }

    public void HideGuessMenu()
    {
        if (guessPanel == null) return;
        
        isClicked = false;
        guessPanel.SetActive(false);
    }

    // CONNECT THESE TO YOUR BUTTONS IN INSPECTOR!
    public void GuessDuke() => GuessRole("National Guard");
    public void GuessThief() => GuessRole("Thief");
    public void GuessAssassin() => GuessRole("Assassin");
    public void GuessFiscality() => GuessRole("Fiscality");
    public void GuessGeneral() => GuessRole("General");
    public void GuessDeputy() => GuessRole("Deputy");
    public void GuessBoss() => GuessRole("Boss");

    private void GuessRole(string roleName)
    {
        if (RoleAbilityManager.Instance == null)
        {
            Debug.LogError("GuessRole: RoleAbilityManager.Instance is null!");
            return;
        }

        RoleAbilityManager.Instance.PredictRole(roleName);
        HideGuessMenu(); // Hide menu after guessing
    }

    public static GuessMenu GetInstance()
    {
        return Instance;
    }
}

