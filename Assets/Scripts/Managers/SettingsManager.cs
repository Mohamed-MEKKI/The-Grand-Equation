using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    private const string MixerSFXParam   = "SFXVolume";
    private const string MixerMusicParam = "MusicVolume";

    public float SFXVolume     { get; private set; } = 0.7f;
    public float MusicVolume   { get; private set; } = 0.5f;
    public bool  Notifications { get; private set; } = true;
    public bool  Fullscreen    { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
            ApplySettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- Setters called by SettingsPanel UI ---

    public void SetSFX(float value)
    {
        SFXVolume = value;
        ApplyAudio();
    }

    public void SetMusic(float value)
    {
        MusicVolume = value;
        ApplyAudio();
    }

    public void SetFullscreen(bool value)
    {
        Fullscreen = value;
        Screen.fullScreen = value;
    }

    public void SetNotifications(bool value)
    {
        Notifications = value;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("SFX",         SFXVolume);
        PlayerPrefs.SetFloat("Music",       MusicVolume);
        PlayerPrefs.SetInt("Notifications", Notifications ? 1 : 0);
        PlayerPrefs.SetInt("Fullscreen",    Fullscreen ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("Settings saved.");
    }

    // --- Internal ---

    private void LoadSettings()
    {
        SFXVolume     = PlayerPrefs.GetFloat("SFX",         0.7f);
        MusicVolume   = PlayerPrefs.GetFloat("Music",       0.5f);
        Notifications = PlayerPrefs.GetInt("Notifications", 1) == 1;
        Fullscreen    = PlayerPrefs.GetInt("Fullscreen",    0) == 1;
    }

    private void ApplySettings()
    {
        ApplyAudio();
        Screen.fullScreen = Fullscreen;
    }

    private void ApplyAudio()
    {
        if (audioMixer != null)
        {
            if (!audioMixer.SetFloat(MixerSFXParam, SliderToDB(SFXVolume)))
                Debug.LogWarning($"AudioMixer: exposed parameter '{MixerSFXParam}' not found. Expose it in the AudioMixer asset.");
            if (!audioMixer.SetFloat(MixerMusicParam, SliderToDB(MusicVolume)))
                Debug.LogWarning($"AudioMixer: exposed parameter '{MixerMusicParam}' not found. Expose it in the AudioMixer asset.");
        }
        else
        {
            AudioListener.volume = SFXVolume;
        }
    }

    private static float SliderToDB(float value)
    {
        return Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
    }
}
