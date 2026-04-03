using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MenuManager : MonoBehaviour
{
        [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private GameObject RulesPanel;

    private void Start()
    {
        
    }

    // --- Button Actions ---
    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnClickSaveSettings()
    {

        HideSettingsMenu();
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