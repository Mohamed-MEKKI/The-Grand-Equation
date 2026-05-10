using UnityEngine;
using UnityEngine.UI;

public class AutoButtonSounds : MonoBehaviour
{
    public AudioSource sfxSource;
    public AudioClip clickSound;

    void Start()
    {
        // 1. Find every single button in your entire scene
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();

        foreach (Button btn in allButtons)
        {
            // 2. Add the sound to every button automatically
            btn.onClick.AddListener(() => {
                Debug.Log("A button was actually clicked!"); // <--- ADD THIS
                sfxSource.PlayOneShot(clickSound);
            });
        }
    }
}