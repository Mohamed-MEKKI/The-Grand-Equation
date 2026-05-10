using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameEventLog : MonoBehaviour
{
    public static GameEventLog Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Behavior")]
    [SerializeField] private int maxLines = 100;
    [SerializeField] private bool autoScrollToBottom = true;
    [SerializeField] private bool prependTimestamp = false;
    [SerializeField] private float messageLifetime = 5f; // ← new

    private readonly List<string> _lines = new List<string>(128);
    private readonly StringBuilder _sb = new StringBuilder(4096);

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Append(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        foreach (var line in SplitLines(message))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string entry = prependTimestamp ? FormatWithTimestamp(line) : line;
            _lines.Add(entry);

            // Start expiry timer for this specific line
            StartCoroutine(ExpireLine(entry, messageLifetime));
        }

        TrimToMaxLines();
        RefreshText();
        TryAutoScroll();
    }

    public void Clear()
    {
        _lines.Clear();
        RefreshText();
        TryAutoScroll();
    }

    public static void AppendGlobal(string message) => Instance?.Append(message);
    public static void ClearGlobal() => Instance?.Clear();

    /// <summary>
    /// Waits for messageLifetime seconds then removes the specific line.
    /// </summary>
    private IEnumerator ExpireLine(string line, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        _lines.Remove(line); // removes first occurrence of this line
        RefreshText();
        TryAutoScroll();
    }

    private void RefreshText()
    {
        if (logText == null) return;

        _sb.Clear();
        for (int i = 0; i < _lines.Count; i++)
        {
            if (i > 0) _sb.Append('\n');
            _sb.Append(_lines[i]);
        }

        logText.text = _sb.ToString();
    }

    private void TryAutoScroll()
    {
        if (!autoScrollToBottom || scrollRect == null) return;

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    private void TrimToMaxLines()
    {
        if (maxLines < 1) maxLines = 1;

        int overflow = _lines.Count - maxLines;
        if (overflow > 0)
            _lines.RemoveRange(0, overflow);
    }

    private static IEnumerable<string> SplitLines(string text)
    {
        return text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
    }

    private static string FormatWithTimestamp(string line)
    {
        return $"[{DateTime.Now:HH:mm:ss}] {line}";
    }
}

