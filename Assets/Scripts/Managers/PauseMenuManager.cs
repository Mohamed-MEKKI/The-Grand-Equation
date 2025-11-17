using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI")]
    public GameObject pausePanel;
    public Button pauseButton;  // Drag your pause button here

    public bool isPaused = false;

    void Awake()
    {
        Instance = this;
    }

    // --- BUTTON CALLBACKS (called from Pause Button OnClick) ---
    public void OnPauseButtonClicked()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;

        // Optional: Change button text to "Resume"
        pauseButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "▶️";
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Optional: Change button text back to "Pause"
        pauseButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "⏸️";
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
}