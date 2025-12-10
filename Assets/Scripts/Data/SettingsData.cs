using UnityEngine;

[CreateAssetMenu(fileName = "SettingsData", menuName = "Settings/Data")]
public class SettingsData : ScriptableObject
{
    [Range(0f, 1f)]
    public float brightness = 1f;
    public bool musicMuted = false;
    [Range(0f, 1f)]
    public float musicVolume = 1f;

    public SystemLanguage currentLanguage = SystemLanguage.English;

    // Future-proof
    public ThemeType theme = ThemeType.Light;

    public enum ThemeType { Light, Dark, Retro, Cyberpunk }
}
