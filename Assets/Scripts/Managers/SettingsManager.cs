using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio")]
    public Slider sfxSlider;
    public Slider musicSlider;

    [Header("Toggles")]
    public Toggle notificationsToggle;
    public Toggle fullscreenToggle;

    [Header("Button")]
    public Button saveButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Load saved settings
        sfxSlider.value = PlayerPrefs.GetFloat("SFX", 0.7f);
        musicSlider.value = PlayerPrefs.GetFloat("Music", 0.5f);
        notificationsToggle.isOn = PlayerPrefs.GetInt("Notifications", 1) == 1;
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 0) == 1;

        // Apply immediately
        ApplySettings();

        // Listeners
        sfxSlider.onValueChanged.AddListener(_ => ApplySettings());
        musicSlider.onValueChanged.AddListener(_ => ApplySettings());
        fullscreenToggle.onValueChanged.AddListener(_ => ApplySettings());
        saveButton.onClick.AddListener(SaveSettings);
    }

    void ApplySettings()
    {
        AudioListener.volume = sfxSlider.value;
        Screen.fullScreen = fullscreenToggle.isOn;
        // Hook musicSlider into your AudioSource if you have one
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("SFX", sfxSlider.value);
        PlayerPrefs.SetFloat("Music", musicSlider.value);
        PlayerPrefs.SetInt("Notifications", notificationsToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Settings saved!");
    }
}