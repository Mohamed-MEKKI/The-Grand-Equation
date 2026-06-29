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

    void Awake()
    {
        Instance = this;
        isPaused = false;
        SetPanelActive(pausePanel, false);
        SetPanelActive(RulesPanel, false);
        SetPanelActive(SettingsPanel, false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void Update()
    {
        bool escapePressed = false;

        if (Keyboard.current != null)
            escapePressed = Keyboard.current.escapeKey.wasPressedThisFrame;
        else
            escapePressed = Input.GetKeyDown(KeyCode.Escape);

        if (!escapePressed) return;

        // Sub-panels take priority — close them first before resuming
        if (SettingsPanel != null && SettingsPanel.activeSelf)
        {
            CloseSettingsPanel();
            return;
        }

        /*if (RulesPanel != null && RulesPanel.activeSelf)
        {
            CloseRulesPanel();
            return;
        }*/

        if (isPaused) ResumeGame();
        else PauseGame();
    }

    // --- BUTTON CALLBACKS ---
    public void OnPauseButtonClicked()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void OnResumeButtonClicked() => ResumeGame();
    public void OpenSettingsButton() => SetPanelActive(SettingsPanel, true);
    public void OnClickShowRulesPanel() => SetPanelActive(RulesPanel, true);
    public void CloseSettingsPanel() => SetPanelActive(SettingsPanel, false);
    //public void CloseRulesPanel() => SetPanelActive(RulesPanel, false);

    public void PauseGame()
    {
        isPaused = true;
        SetPanelActive(pausePanel, true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        Debug.Log("Resuming game");
        isPaused = false;
        SetPanelActive(pausePanel, false);
        SetPanelActive(RulesPanel, false);
        SetPanelActive(SettingsPanel, false);
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (pauseButton != null)
            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(pauseButton.gameObject);
    }

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