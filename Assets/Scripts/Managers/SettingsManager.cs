using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer mainMixer;

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen; 

    }

    public void setVolume(float volume)
    {
        mainMixer.SetFloat("volume", volume);
    }
}