// SoloModeManager.cs - Attach to Canvas in MainMenu or PersistentUI
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SoloModeManager : MonoBehaviour
{
    public static SoloModeManager Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject soloPanel;                // Main panel (disabled by default)
    public TMP_Dropdown levelDropdown;
    public TMP_Dropdown opponentsDropdown;      // "Number of Players" = 1v1, 1v2, etc.
    public TMP_Dropdown difficultyDropdown;
    public Button playButton;
    public Button backButton;
    public TextMeshProUGUI titleText;           // "#solo_mode_title"

    [Header("Data")]
    public List<SoloLevelData> allLevels;       // Assign ScriptableObjects in Inspector
    public SoloLevelData currentLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this && gameObject!=null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetupUI();
    }

    void SetupUI()
    {
        // Populate dropdowns
        PopulateLevelDropdown();
        PopulateOpponentsDropdown();
        PopulateDifficultyDropdown();

        playButton.onClick.AddListener(StartSoloGame);
        backButton.onClick.AddListener(ClosePanel);

        // Disable play until selections made
        UpdatePlayButton();
        levelDropdown.onValueChanged.AddListener(_ => UpdatePlayButton());
        opponentsDropdown.onValueChanged.AddListener(_ => UpdatePlayButton());
        difficultyDropdown.onValueChanged.AddListener(_ => UpdatePlayButton());
    }

    void PopulateLevelDropdown()
    {
        levelDropdown.ClearOptions();
        foreach (var level in allLevels)
        {
            levelDropdown.options.Add(new TMP_Dropdown.OptionData($"Level {level.levelNumber} - {level.name}"));
        }
        levelDropdown.value = 0;
    }

    void PopulateOpponentsDropdown()
    {
        opponentsDropdown.ClearOptions();
        opponentsDropdown.options.Add(new TMP_Dropdown.OptionData("1 Opponent"));
        opponentsDropdown.options.Add(new TMP_Dropdown.OptionData("2 Opponents"));
        opponentsDropdown.options.Add(new TMP_Dropdown.OptionData("3 Opponents"));
        opponentsDropdown.value = 0;
    }

    void PopulateDifficultyDropdown()
    {
        difficultyDropdown.ClearOptions();
        difficultyDropdown.options.Add(new TMP_Dropdown.OptionData("Easy"));
        difficultyDropdown.options.Add(new TMP_Dropdown.OptionData("Medium"));
        difficultyDropdown.options.Add(new TMP_Dropdown.OptionData("Hard"));
        difficultyDropdown.options.Add(new TMP_Dropdown.OptionData("Insane"));
        difficultyDropdown.value = 1; // Default Medium
    }

    void UpdatePlayButton()
    {
        playButton.interactable = levelDropdown.value > 0;
    }

    public void OpenSoloPanel()
    {
        soloPanel.SetActive(true);
        UpdatePlayButton();
    }

    public void ClosePanel()
    {
        soloPanel.SetActive(false);
    }

    void StartSoloGame()
    {
        currentLevel = allLevels[levelDropdown.value];

        // Save selections to GameManager or PlayerPrefs
        PlayerPrefs.SetInt("Solo_Opponents", opponentsDropdown.value + 1);
        PlayerPrefs.SetInt("Solo_Difficulty", difficultyDropdown.value);
        PlayerPrefs.SetInt("Solo_Level", currentLevel.levelNumber);
        PlayerPrefs.Save();

        // Load Game scene + setup solo
        SceneManager.LoadScene("GameScene");
    }

    // Called by GameManager on scene load
    public static void SetupSoloGame()
    {
        int numOpponents = PlayerPrefs.GetInt("Solo_Opponents", 1);
        int difficulty = PlayerPrefs.GetInt("Solo_Difficulty", 1);
        int levelNum = PlayerPrefs.GetInt("Solo_Level", 1);
    }
}