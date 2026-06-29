using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Clips")]
    public AudioClip coinSound;
    public AudioClip bulletSound;
    public AudioClip sirenSound;
    public AudioClip areaSound;
    public AudioClip swapSound;
    public AudioClip stabSound;

    private const string MixerSFXParam = "SFXVolume";
    private const string MixerMusicParam = "MusicVolume";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureSources();
        PrimePipeline();
    }

    // Called before every PlaySFX/SetVolume to guarantee sources exist
    private void EnsureSources()
    {
        if (sfxSource == null || !sfxSource)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            if (audioMixer != null)
            {
                // Re-assign mixer group if available
                var sfxGroup = audioMixer.FindMatchingGroups("SFX");
                if (sfxGroup.Length > 0)
                    sfxSource.outputAudioMixerGroup = sfxGroup[0];
            }
            Debug.LogWarning("AudioManager: sfxSource was null — recreated automatically.");
        }

        if (musicSource == null || !musicSource)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            if (audioMixer != null)
            {
                var musicGroup = audioMixer.FindMatchingGroups("Music");
                if (musicGroup.Length > 0)
                    musicSource.outputAudioMixerGroup = musicGroup[0];
            }
            Debug.LogWarning("AudioManager: musicSource was null — recreated automatically.");
        }
    }

    private void PrimePipeline()
    {
        EnsureSources();
        if (coinSound == null) return;
        float vol = sfxSource.volume;
        sfxSource.volume = 0f;
        sfxSource.PlayOneShot(coinSound);
        sfxSource.volume = vol;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        EnsureSources();
        sfxSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        EnsureSources();
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        EnsureSources();
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        EnsureSources();
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        EnsureSources();
        if (!musicSource.isPlaying)
            musicSource.UnPause();
    }

    public void SetSFXVolume(float volume)
    {
        EnsureSources();
        if (audioMixer != null)
            audioMixer.SetFloat(MixerSFXParam, SliderToDB(volume));
        sfxSource.volume = volume;
    }

    public void SetMusicVolume(float volume)
    {
        EnsureSources();
        if (audioMixer != null)
            audioMixer.SetFloat(MixerMusicParam, SliderToDB(volume));
        musicSource.volume = volume;
    }

    private static float SliderToDB(float value)
    {
        return value <= 0f ? -80f : Mathf.Log10(value) * 20f;
    }
}