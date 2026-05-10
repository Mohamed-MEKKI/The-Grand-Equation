using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private GameObject RulesPanel;

    [Header("Multiplayer launch")]
    [Range(3, 5)]
    [SerializeField] private int multiplayerRoundsToWin = 3;

    private void Start()
    {
        
    }

    // --- Button Actions ---
    public void PlayGame()
    {
        SceneManager.LoadScene("LevelsScene");
    }

    public void OnClickSaveSettings()
    {

        HideSettingsMenu();
    }

    public void OnClickSelectMultiplayer()
    {
        // Persist multiplayer selection so the Game scene can pick it up on load
        PlayerPrefs.SetInt("IsMultiplayer", 1);
        PlayerPrefs.SetInt(GameManager.PlayerPrefsMultiplayerRounds, multiplayerRoundsToWin);
        PlayerPrefs.Save();

        // If GameManager exists in this scene (unlikely), set immediately
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMultiplayerMode(true);
        }
        SceneManager.LoadScene("GameScene");
    }

    public void OnClickSettingsMenu()
    {
        Debug.Log("Clicked");
        SetPanelActive(SettingsPanel, true);
    }

    public void HideSettingsMenu()
    {
        SetPanelActive(SettingsPanel, false);
    }

    public void ToggleSettingsMenu()
    {
        if (SettingsPanel == null) return;
        SetPanelActive(SettingsPanel, !SettingsPanel.activeSelf);
    }

    public void OnClickShowRulesPanel()
    {
        SetPanelActive(RulesPanel, true);
    }

    public void OnClickHideRulesPanel()
    {
        SetPanelActive(RulesPanel, false);
    }

    public void QuitGame()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }

    // --- Private helpers ---
    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel == null) return;
        panel.SetActive(active);
    }

 
}