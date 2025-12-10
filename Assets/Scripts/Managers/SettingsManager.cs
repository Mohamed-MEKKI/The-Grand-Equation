using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Manages all game settings including audio, display, language, and theme.
/// Uses singleton pattern and persists settings using PlayerPrefs.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    #region Singleton
    public static SettingsManager Instance { get; private set; }
    #endregion

    #region Constants
    private const string SETTINGS_SAVE_KEY = "GameSettings";
    private const string AUDIO_MIXER_MUSIC_VOLUME_PARAM = "MusicVolume";
    private const float MIN_AUDIO_VOLUME_DB = -80f;
    private const float MAX_AUDIO_VOLUME_DB = 0f;
    private const float DEFAULT_BRIGHTNESS = 1f;
    private const float DEFAULT_MUSIC_VOLUME = 1f;
    #endregion

    #region Fields
    [Header("References")]
    [SerializeField] public SettingsData settings;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private GameObject settingsPanel;

    [Header("Brightness Overlay (optional)")]
    [SerializeField] private Image brightnessOverlay; // Black panel with alpha

    // Language mapping data
    private readonly SystemLanguage[] supportedLanguages = new SystemLanguage[]
    {
        SystemLanguage.English,
        SystemLanguage.Spanish,
        SystemLanguage.French,
        SystemLanguage.German,
        SystemLanguage.Italian,
        SystemLanguage.Portuguese,
        SystemLanguage.Russian,
        SystemLanguage.Japanese,
        SystemLanguage.ChineseSimplified
    };

    private readonly string[] languageNames = new string[]
    {
        "English",
        "Español",
        "Français",
        "Deutsch",
        "Italiano",
        "Português",
        "Русский",
        "日本語",
        "简体中文"
    };
    #endregion

    #region Initialization
    private void Awake()
    {
        InitializeSingleton();
        ValidateReferences();
        LoadSettings();
        ApplyAllSettings();
        SetupUI();
    }

    /// <summary>
    /// Initializes the singleton instance.
    /// </summary>
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Validates that required references are assigned.
    /// </summary>
    private void ValidateReferences()
    {
        if (settings == null)
        {
            Debug.LogError("SettingsManager: SettingsData is not assigned in the Inspector!");
        }

        if (audioMixer == null)
        {
            Debug.LogWarning("SettingsManager: AudioMixer is not assigned. Audio settings will not work.");
        }
    }
    #endregion

    #region UI Setup
    /// <summary>
    /// Sets up UI event listeners and populates dropdowns.
    /// </summary>
    private void SetupUI()
    {
        SetupBrightnessSlider();
        SetupMusicControls();
        SetupLanguageDropdown();
        RefreshUI();
    }

    /// <summary>
    /// Sets up the brightness slider event listener.
    /// </summary>
    private void SetupBrightnessSlider()
    {
        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
        }
    }

    /// <summary>
    /// Sets up music control event listeners.
    /// </summary>
    private void SetupMusicControls()
    {
        if (musicToggle != null)
        {
            musicToggle.onValueChanged.AddListener(ToggleMusic);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }
    }

    /// <summary>
    /// Sets up the language dropdown with options and event listener.
    /// </summary>
    private void SetupLanguageDropdown()
    {
        if (languageDropdown == null) return;

        languageDropdown.onValueChanged.AddListener(SetLanguage);
        PopulateLanguageDropdown();
    }

    /// <summary>
    /// Refreshes all UI elements to reflect current settings.
    /// </summary>
    private void RefreshUI()
    {
        if (settings == null) return;

        if (brightnessSlider != null)
        {
            brightnessSlider.value = Mathf.Clamp01(settings.brightness);
        }

        if (musicToggle != null)
        {
            musicToggle.isOn = settings.musicMuted;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = Mathf.Clamp01(settings.musicVolume);
        }
    }
    #endregion

    #region Public UI Methods
    /// <summary>
    /// Opens the settings panel.
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("SettingsManager: Settings panel is not assigned!");
        }
    }

    /// <summary>
    /// Closes the settings panel.
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
    #endregion

    #region Brightness Settings
    /// <summary>
    /// Sets the screen brightness value.
    /// </summary>
    /// <param name="value">Brightness value between 0 and 1.</param>
    public void SetBrightness(float value)
    {
        if (settings == null) return;

        value = Mathf.Clamp01(value);
        settings.brightness = value;

        ApplyBrightness(value);
        SaveSettings();
    }

    /// <summary>
    /// Applies the brightness setting to the overlay.
    /// </summary>
    private void ApplyBrightness(float value)
    {
        if (brightnessOverlay != null)
        {
            brightnessOverlay.color = new Color(0, 0, 0, 1f - value);
        }
    }
    #endregion

    #region Audio Settings
    /// <summary>
    /// Toggles music on/off.
    /// </summary>
    /// <param name="muted">True to mute music, false to unmute.</param>
    public void ToggleMusic(bool muted)
    {
        if (settings == null) return;

        settings.musicMuted = muted;
        ApplyMusicVolume();
        SaveSettings();
    }

    /// <summary>
    /// Sets the music volume.
    /// </summary>
    /// <param name="volume">Volume value between 0 and 1.</param>
    public void SetMusicVolume(float volume)
    {
        if (settings == null) return;

        settings.musicVolume = Mathf.Clamp01(volume);
        ApplyMusicVolume();
        SaveSettings();
    }

    /// <summary>
    /// Applies the current music volume to the audio mixer.
    /// </summary>
    private void ApplyMusicVolume()
    {
        if (audioMixer == null || settings == null) return;

        float volumeDb = settings.musicMuted 
            ? MIN_AUDIO_VOLUME_DB 
            : Mathf.Lerp(MIN_AUDIO_VOLUME_DB, MAX_AUDIO_VOLUME_DB, settings.musicVolume);

        bool success = audioMixer.SetFloat(AUDIO_MIXER_MUSIC_VOLUME_PARAM, volumeDb);
        if (!success)
        {
            Debug.LogWarning($"SettingsManager: Failed to set audio mixer parameter '{AUDIO_MIXER_MUSIC_VOLUME_PARAM}'. Make sure it exists in your Audio Mixer.");
        }
    }
    #endregion

    #region Language Settings
    /// <summary>
    /// Populates the language dropdown with supported languages.
    /// </summary>
    private void PopulateLanguageDropdown()
    {
        if (languageDropdown == null || settings == null) return;

        languageDropdown.ClearOptions();

        var options = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < languageNames.Length; i++)
        {
            options.Add(new TMP_Dropdown.OptionData(languageNames[i]));
        }

        languageDropdown.AddOptions(options);

        // Set current language selection
        int currentIndex = GetLanguageIndex(settings.currentLanguage);
        languageDropdown.value = Mathf.Max(0, currentIndex);
    }

    /// <summary>
    /// Sets the game language based on dropdown selection.
    /// </summary>
    /// <param name="index">Index of the selected language.</param>
    public void SetLanguage(int index)
    {
        if (settings == null) return;

        if (index < 0 || index >= supportedLanguages.Length)
        {
            Debug.LogWarning($"SettingsManager: Invalid language index {index}. Defaulting to English.");
            settings.currentLanguage = SystemLanguage.English;
        }
        else
        {
            settings.currentLanguage = supportedLanguages[index];
        }

        ApplyLanguage();
        SaveSettings();
    }

    /// <summary>
    /// Applies the current language setting.
    /// </summary>
    private void ApplyLanguage()
    {
        // Language system not yet implemented
        // TODO: Implement LanguageSystem to handle language changes
        // For now, the language preference is saved but not applied
        Debug.Log($"Language changed to: {settings.currentLanguage}");
    }

    /// <summary>
    /// Gets the index of a language in the supported languages array.
    /// </summary>
    private int GetLanguageIndex(SystemLanguage language)
    {
        for (int i = 0; i < supportedLanguages.Length; i++)
        {
            if (supportedLanguages[i] == language)
            {
                return i;
            }
        }
        return 0; // Default to English
    }
    #endregion

    #region Theme Settings
    /// <summary>
    /// Sets the game theme.
    /// </summary>
    /// <param name="theme">The theme type to apply.</param>
    public void SetTheme(SettingsData.ThemeType theme)
    {
        if (settings == null) return;

        settings.theme = theme;
        ApplyTheme();
        SaveSettings();
    }

    /// <summary>
    /// Applies the current theme setting.
    /// </summary>
    private void ApplyTheme()
    {
        if (settings == null) return;

        // Example: change background, card colors, etc.
        Color mainColor = settings.theme switch
        {
            SettingsData.ThemeType.Dark => new Color(0.1f, 0.1f, 0.1f),
            SettingsData.ThemeType.Retro => new Color(0.8f, 0.7f, 0.5f),
            SettingsData.ThemeType.Cyberpunk => new Color(0.2f, 0f, 0.3f),
            _ => Color.white
        };

        // TODO: Apply theme colors to UI panels, cards, etc.
        // This is a placeholder for future theme implementation
    }
    #endregion

    #region Settings Application
    /// <summary>
    /// Applies all current settings to the game.
    /// </summary>
    private void ApplyAllSettings()
    {
        if (settings == null) return;

        ApplyBrightness(settings.brightness);
        ApplyMusicVolume();
        ApplyTheme();
        ApplyLanguage();
    }
    #endregion

    #region Save/Load
    /// <summary>
    /// Saves current settings to PlayerPrefs.
    /// </summary>
    public void SaveSettings()
    {
        if (settings == null)
        {
            Debug.LogWarning("SettingsManager: Cannot save settings - SettingsData is null!");
            return;
        }

        try
        {
            string json = JsonUtility.ToJson(settings);
            PlayerPrefs.SetString(SETTINGS_SAVE_KEY, json);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SettingsManager: Failed to save settings: {e.Message}");
        }
    }

    /// <summary>
    /// Loads settings from PlayerPrefs.
    /// </summary>
    public void LoadSettings()
    {
        if (settings == null)
        {
            Debug.LogWarning("SettingsManager: Cannot load settings - SettingsData is null!");
            return;
        }

        if (PlayerPrefs.HasKey(SETTINGS_SAVE_KEY))
        {
            try
            {
                string json = PlayerPrefs.GetString(SETTINGS_SAVE_KEY);
                JsonUtility.FromJsonOverwrite(json, settings);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SettingsManager: Failed to load settings: {e.Message}. Using defaults.");
                ResetToDefaults();
            }
        }
    }
    #endregion

    #region Reset
    /// <summary>
    /// Resets all settings to their default values.
    /// </summary>
    public void ResetToDefaults()
    {
        if (settings == null)
        {
            Debug.LogError("SettingsManager: Cannot reset settings - SettingsData is null!");
            return;
        }

        settings.brightness = DEFAULT_BRIGHTNESS;
        settings.musicMuted = false;
        settings.musicVolume = DEFAULT_MUSIC_VOLUME;
        settings.currentLanguage = SystemLanguage.English;
        settings.theme = SettingsData.ThemeType.Light;

        ApplyAllSettings();
        RefreshUI();
        SaveSettings();
    }
    #endregion
}
