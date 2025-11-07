using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MenuManager : MonoBehaviour
{
    //public GameObject settingsPanel;
    //public Slider volumeSlider;
    //public Toggle fullscreenToggle;
    //public AudioMixer audioMixer;

    void Start()
    {
        // Load saved settings
        //volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
        //fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        //Screen.fullScreen = fullscreenToggle.isOn;
    }

    // --- Button Actions ---
    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene"); // Your game scene
    }
    /*
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Volume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("Volume", volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }*/

    public void QuitGame()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }
}