using UnityEngine;
using UnityEngine.UI;

// Attach once to a persistent manager object. Finds only scene buttons and never re-registers them.
public class AutoButtonSounds : MonoBehaviour
{
    public AudioSource sfxSource;
    public AudioClip clickSound;

    private void Start()
    {
        RegisterSceneButtons();
    }

    private void RegisterSceneButtons()
    {
        if (sfxSource == null || clickSound == null) return;

        foreach (Button btn in FindObjectsByType<Button>(FindObjectsSortMode.None))
            btn.onClick.AddListener(PlayClick);
    }

    private void PlayClick()
    {
        if (sfxSource != null && clickSound != null)
            sfxSource.PlayOneShot(clickSound);
    }
}
