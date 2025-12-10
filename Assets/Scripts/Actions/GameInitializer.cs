using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializeGame()
    {
        // BLOCKING load — guarantees SettingsManager exists BEFORE anything else
        if (!SceneManager.GetSceneByName("PersistentUI").isLoaded)
        {
            SceneManager.LoadScene("PersistentUI", LoadSceneMode.Additive);
            // ↑ NO Async → waits until fully loaded
        }

        // Extra safety: force-find the instance
        var settingsManagerObj = GameObject.Find("SettingsManager");
        if (settingsManagerObj != null)
        {
            var sm = settingsManagerObj.GetComponent<SettingsManager>();
            if (sm != null)
                sm.GetType().GetField("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                          ?.SetValue(null, sm);
        }
    }
}