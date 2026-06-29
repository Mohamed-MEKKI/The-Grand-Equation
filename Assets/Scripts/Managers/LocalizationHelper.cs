using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public static class LocalizationHelper
{
    private const string Table = "StringTable_FullGame_Scene";

    public static string Loc(string key)
    {
        var localizedString = new LocalizedString(Table, key);
        // WaitForCompletion forces synchronous load if table is preloaded
        var op = localizedString.GetLocalizedStringAsync();
        if (!op.IsDone)
            op.WaitForCompletion();
        return string.IsNullOrEmpty(op.Result) ? key : op.Result;
    }

    public static string Loc(string key, params object[] args)
    {
        var localizedString = new LocalizedString(Table, key);
        var op = localizedString.GetLocalizedStringAsync(args);
        if (!op.IsDone)
            op.WaitForCompletion();
        return string.IsNullOrEmpty(op.Result) ? key : op.Result;
    }

    // Use this coroutine version for non-preloaded tables to avoid blocking
    public static IEnumerator LocAsync(string key, System.Action<string> onResult)
    {
        var localizedString = new LocalizedString(Table, key);
        var op = localizedString.GetLocalizedStringAsync();
        yield return op;
        onResult?.Invoke(string.IsNullOrEmpty(op.Result) ? key : op.Result);
    }
}