using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class LanguageSystem : MonoBehaviour
{
    public static LanguageSystem Instance { get; private set; }

    [System.Serializable]
    public class LanguageEntry
    {
        public string key;           // e.g. "ui_start_game"
        public string english;
        public string spanish;
        public string french;
        public string german;
        public string italian;
        public string portuguese;
        public string russian;
        public string japanese;
        public string chineseSimplified;
        // Add more languages here later
    }

    [Header("All texts in the game")]
    public List<LanguageEntry> entries = new List<LanguageEntry>();

    private Dictionary<string, string> currentDict = new Dictionary<string, string>();
    private SystemLanguage currentLanguage = SystemLanguage.English;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadLanguage(SettingsManager.Instance.settings.currentLanguage);
    }

    public void LoadLanguage(SystemLanguage lang)
    {
        currentLanguage = lang;
        currentDict.Clear();

        foreach (var entry in entries)
        {
            string text = lang switch
            {
                SystemLanguage.Spanish => entry.spanish,
                SystemLanguage.French => entry.french,
                SystemLanguage.German => entry.german,
                SystemLanguage.Italian => entry.italian,
                SystemLanguage.Portuguese => entry.portuguese,
                SystemLanguage.Russian => entry.russian,
                SystemLanguage.Japanese => entry.japanese,
                SystemLanguage.ChineseSimplified => entry.chineseSimplified,
                _ => entry.english
            };

            if (!string.IsNullOrEmpty(text))
                currentDict[entry.key] = text;
        }

        // Update ALL text in scene right now
        RefreshAllTextInScene();
    }

    public string Get(string key)
    {
        if (currentDict.TryGetValue(key, out string value))
            return value;

        return $"[{key}]"; // missing text indicator
    }

    // Call this after changing language
    public void RefreshAllTextInScene()
    {
        var allTMP = Object.FindObjectsOfType<TextMeshProUGUI>(true);
        var allLegacy = Object.FindObjectsOfType<Text>(true);

        foreach (var t in allTMP)
        {
            if (!string.IsNullOrEmpty(t.text) && t.text.StartsWith("#"))
                t.text = Get(t.text.Substring(1));
        }

        foreach (var t in allLegacy)
        {
            if (!string.IsNullOrEmpty(t.text) && t.text.StartsWith("#"))
                t.text = Get(t.text.Substring(1));
        }
    }
}