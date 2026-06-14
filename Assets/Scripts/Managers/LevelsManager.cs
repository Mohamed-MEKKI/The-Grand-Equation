using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelsManager : MonoBehaviour
{
    public enum Difficulty { Beginner = 1, Intermediate = 2, Hard = 3 }

    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";


    private void Start()
    {

    }

    // ── Button wiring ──────────────────────────────────────────────

    private void AddDifficultyListener(Button button, Difficulty difficulty)
    {
        if (button == null)
        {
            Debug.LogWarning($"LevelsManager: {difficulty} button is not assigned.");
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnDifficultySelected(difficulty));
    }

    // ── onClick handlers ───────────────────────────────────────────

    private void OnDifficultySelected(Difficulty difficulty)
    {
        SaveDifficulty(difficulty);
        LoadGameScene();
    }

    public void OnBeginnerClicked() => OnDifficultySelected(Difficulty.Beginner);
    public void OnIntermediateClicked() => OnDifficultySelected(Difficulty.Intermediate);
    public void OnHardClicked() => OnDifficultySelected(Difficulty.Hard);
    public void OnMainMenuClicked() => SceneManager.LoadScene(mainMenuSceneName);

    // ── Core logic ─────────────────────────────────────────────────

    private void SaveDifficulty(Difficulty difficulty)
    {
        PlayerPrefs.SetInt("SelectedDifficulty", (int)difficulty);
        PlayerPrefs.Save();
        Debug.Log($"LevelsManager: Difficulty saved → {difficulty} ({(int)difficulty})");
    }

    private void LoadGameScene()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("LevelsManager: gameSceneName is empty.");
            return;
        }

        PlayerPrefs.SetInt("IsMultiplayer", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }
}