using UnityEngine;
using UnityEngine.UI;

// Attach this to every settings panel GameObject in every scene.
// It reads from SettingsManager when shown and writes back on change.
public class SettingsPanel : MonoBehaviour
{
    [Header("Sliders")]
    public Slider sfxSlider;
    public Slider musicSlider;

    [Header("Toggles")]
    public Toggle notificationsToggle;
    public Toggle fullscreenToggle;

    [Header("Button")]
    public Button saveButton;

    private void OnEnable()
    {
        var sm = SettingsManager.Instance;
        if (sm == null) return;

        // Populate UI from current settings without triggering callbacks
        if (sfxSlider          != null) sfxSlider.SetValueWithoutNotify(sm.SFXVolume);
        if (musicSlider        != null) musicSlider.SetValueWithoutNotify(sm.MusicVolume);
        if (notificationsToggle != null) notificationsToggle.SetIsOnWithoutNotify(sm.Notifications);
        if (fullscreenToggle   != null) fullscreenToggle.SetIsOnWithoutNotify(sm.Fullscreen);

        if (sfxSlider          != null) sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        if (musicSlider        != null) musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (notificationsToggle != null) notificationsToggle.onValueChanged.AddListener(OnNotificationsChanged);
        if (fullscreenToggle   != null) fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        if (saveButton         != null) saveButton.onClick.AddListener(OnSave);
    }

    private void OnDisable()
    {
        if (sfxSlider          != null) sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
        if (musicSlider        != null) musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        if (notificationsToggle != null) notificationsToggle.onValueChanged.RemoveListener(OnNotificationsChanged);
        if (fullscreenToggle   != null) fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        if (saveButton         != null) saveButton.onClick.RemoveListener(OnSave);

        SettingsManager.Instance?.SaveSettings();
    }

    private void OnSFXChanged(float value)          => SettingsManager.Instance?.SetSFX(value);
    private void OnMusicChanged(float value)        => SettingsManager.Instance?.SetMusic(value);
    private void OnNotificationsChanged(bool value) => SettingsManager.Instance?.SetNotifications(value);
    private void OnFullscreenChanged(bool value)    => SettingsManager.Instance?.SetFullscreen(value);
    private void OnSave()                           => SettingsManager.Instance?.SaveSettings();
}
