using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    private const string MixerSFXParam = "SFXVolume";
    private const string MixerMusicParam = "MusicVolume";

    public float SFXVolume { get; private set; } = 0.5f;
    public float MusicVolume { get; private set; } = 0.5f;
    public bool Notifications { get; private set; } = true;
    public string CurrentLocale { get; private set; } = "en";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSettings();

        // Listen for scene changes to reapply audio
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Apply after all Awake() calls finished — AudioManager is guaranteed ready
        ApplySettings();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reapply every time a new scene loads
        ApplySettings();
    }

    // --- Setters called by SettingsPanel UI ---

    public void SetSFX(float value)
    {
        SFXVolume = value;
        ApplyAudio();
        AudioManager.Instance?.SetSFXVolume(value);
    }

    public void SetMusic(float value)
    {
        MusicVolume = value;
        ApplyAudio();
        AudioManager.Instance?.SetMusicVolume(value);
    }

    public void SetNotifications(bool value)
    {
        Notifications = value;
    }

    public void SetLanguage(string localeCode)
    {
        CurrentLocale = localeCode;
        StartCoroutine(ApplyLocaleCoroutine(localeCode));
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("SFX", SFXVolume);
        PlayerPrefs.SetFloat("Music", MusicVolume);
        PlayerPrefs.SetInt("Notifications", Notifications ? 1 : 0);
        PlayerPrefs.SetString("Language", CurrentLocale);
        PlayerPrefs.Save();
        Debug.Log("Settings saved.");
    }

    // --- Internal ---

    private void LoadSettings()
    {
        SFXVolume = PlayerPrefs.GetFloat("SFX", 0.7f);
        MusicVolume = PlayerPrefs.GetFloat("Music", 0.5f);
        Notifications = PlayerPrefs.GetInt("Notifications", 1) == 1;
        CurrentLocale = PlayerPrefs.GetString("Language", "en");
    }

    private void ApplySettings()
    {
        ApplyAudio();
        StartCoroutine(ApplyLocaleCoroutine(CurrentLocale));
    }

    private void ApplyAudio()
    {
        if (audioMixer != null)
        {
            if (!audioMixer.SetFloat(MixerSFXParam, SliderToDB(SFXVolume)))
                Debug.LogWarning($"AudioMixer: '{MixerSFXParam}' not found.");
            if (!audioMixer.SetFloat(MixerMusicParam, SliderToDB(MusicVolume)))
                Debug.LogWarning($"AudioMixer: '{MixerMusicParam}' not found.");
        }

        // Always sync AudioManager regardless of mixer
        AudioManager.Instance?.SetSFXVolume(SFXVolume);
        AudioManager.Instance?.SetMusicVolume(MusicVolume);
    }

    private IEnumerator ApplyLocaleCoroutine(string code)
    {
        yield return LocalizationSettings.InitializationOperation;
        var locale = LocalizationSettings.AvailableLocales.GetLocale(code);
        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;
        else
            Debug.LogWarning($"[SettingsManager] Locale '{code}' not found.");
    }

    private static float SliderToDB(float value)
    {
        return value <= 0f ? -80f : Mathf.Log10(value) * 20f;
    }
}