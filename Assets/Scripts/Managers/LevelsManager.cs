using System;
using UnityEngine;
using UnityEngine.UI;           // Important for Button
using UnityEngine.SceneManagement;

public class LevelsManager : MonoBehaviour
{
    [Header("Difficulty Buttons")]
    public Button beginnerButton;
    public Button intermediateButton;
    public Button hardButton;
    [Space]
    [SerializeField] private string gameSceneName = "GameScene";

    private enum Difficulty
    {
        Beginner = 1,
        Intermediate = 2,
        Hard = 3
    }

    private void Start()
    {
        ConfigureButton(beginnerButton, Difficulty.Beginner);
        ConfigureButton(intermediateButton, Difficulty.Intermediate);
        ConfigureButton(hardButton, Difficulty.Hard);
    }

    private void ConfigureButton(Button button, Difficulty difficulty)
    {
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => LoadDifficultyLevel((int)difficulty));
    }

    // One clean function that handles all difficulties
    public void LoadDifficultyLevel(int difficultyIndex)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetSelectedLevel(difficultyIndex);
            Debug.Log("Selected difficulty level: " + difficultyIndex);
        }
        else
        {
            Debug.LogWarning("LoadDifficultyLevel: GameManager.Instance is null. Difficulty stored nowhere.");
        }

        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("LoadDifficultyLevel: gameSceneName is empty. Cannot load scene.");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}