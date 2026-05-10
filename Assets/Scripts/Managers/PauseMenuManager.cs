using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI")]
    public GameObject pausePanel;
    public Button pauseButton;
    [SerializeField] private GameObject RulesPanel;
    [SerializeField] private GameObject SettingsPanel;


    public bool isPaused = false;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            pausePanel.SetActive(false);
        }
    }

    void Awake()
    {
        Instance = this;
        // Defensive: ensure we never start a scene paused with an active overlay blocking clicks.
        isPaused = false;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    // --- BUTTON CALLBACKS (called from Pause Button OnClick) ---
    public void OnPauseButtonClicked()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }
    public void OpenSettingsButton()
    {
        SetPanelActive(SettingsPanel, true);
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    public void OnClickShowRulesPanel()
    {
        SetPanelActive(RulesPanel, true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void CloseSettingsPanel()
    {
        SetPanelActive(SettingsPanel, false);
    }

    public void CloseRulesPanel()
    {
        SetPanelActive(RulesPanel, false);
    }

    // --- OTHER BUTTONS ---
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel == null) return;
        panel.SetActive(active);
    }
}