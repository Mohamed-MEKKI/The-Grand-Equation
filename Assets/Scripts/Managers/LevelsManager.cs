using UnityEngine;

public class LevelsManager : MonoBehaviour
{
    public enum Difficulty { Beginner = 1, Intermediate = 2, Hard = 3 }

    // ── onClick handlers ───────────────────────────────────────────

    private void OnDifficultySelected(Difficulty difficulty)
    {
        SaveDifficulty(difficulty);
        LoadGameScene();
    }

    public void OnBeginnerClicked() => OnDifficultySelected(Difficulty.Beginner);
    public void OnIntermediateClicked() => OnDifficultySelected(Difficulty.Intermediate);
    public void OnHardClicked() => OnDifficultySelected(Difficulty.Hard);
    public void OnMainMenuClicked() => SceneLoader.GoToMainMenu();

    // ── Core logic ─────────────────────────────────────────────────

    private void SaveDifficulty(Difficulty difficulty)
    {
        PlayerPrefs.SetInt("SelectedDifficulty", (int)difficulty);
        PlayerPrefs.Save();
        Debug.Log($"LevelsManager: Difficulty saved → {difficulty} ({(int)difficulty})");
    }

    private void LoadGameScene()
    {
        // Explicitly mark single-player so GameManager reads the correct mode.
        // Note: this clears any multiplayer flag set by a lobby screen.
        PlayerPrefs.SetInt("IsMultiplayer", 0);
        PlayerPrefs.Save();
        SceneLoader.GoToGame();
    }

}