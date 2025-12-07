using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public float maxTime = 100f; // Maximum time for the timer
    private float countDown; // Current countdown value
    public TextMeshProUGUI timerText; // Reference to TimerText
    public TextMeshProUGUI labelText; // Reference to TimerLabel
    public bool isRunning = true; // Is the timer running?
    public bool isPaused = false; // Is the timer paused?

    [SerializeField] private AudioSource tickAudioSource; // assign in Inspector (preferred)
    [SerializeField] private AudioSource alarmAudioSource; // assign in Inspector (preferred)

    private int lastWholeSecond; // To track ticks
    private bool alarmMuted = true; // start muted by code (can also mute in Inspector)

    private void Start()
    {
        // Fallback: automatically find and assign if not set in Inspector
        if (tickAudioSource == null || alarmAudioSource == null)
        {
            AudioSource[] audioSources = GetComponents<AudioSource>();
            if (audioSources.Length >= 2)
            {
                tickAudioSource ??= audioSources[0];
                alarmAudioSource ??= audioSources[1];
            }
            else
            {
                Debug.LogError("Timer GameObject requires two AudioSource components or assign them in the Inspector!");
            }
        }
            
        // Defensive: stop any accidental playback at start
        if (tickAudioSource != null) tickAudioSource.Stop();
        if (alarmAudioSource != null)
        {
            alarmAudioSource.Stop();
            alarmAudioSource.mute = alarmMuted; // start muted
        }

        countDown = maxTime;
        lastWholeSecond = Mathf.FloorToInt(countDown);
        UpdateTimerText();
    }

    private void Update()
    {
        if (isRunning && !isPaused)
        {
            countDown -= Time.deltaTime;
            int currentSecond = Mathf.FloorToInt(countDown);

            // Play tick sound once per second when we cross a whole-second boundary,
            // but do NOT play a tick for the 0 second (prevent overlap with alarm).
            if (currentSecond < lastWholeSecond && currentSecond > 0 && tickAudioSource != null)
            {
                tickAudioSource.Play();
                Debug.Log($"Tick played for second {currentSecond}");
                lastWholeSecond = currentSecond;
            }

            // Finish when countdown reaches or goes below zero (handle overshoot)
            if (countDown <= 0f)
            {
                countDown = 0f;
                isRunning = false;
                TimerFinished();
            }

            UpdateTimerText();
        }
    }

    private void UpdateTimerText()
    {
        // Check if timerText is assigned before accessing it
        if (timerText == null)
        {
            return;
        }
        
        float displayTime = Mathf.Max(0f, countDown);
        int minutes = Mathf.FloorToInt(displayTime / 60f);
        int seconds = Mathf.FloorToInt(displayTime % 60f);
        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    private void TimerFinished()
    {
        // Check if labelText is assigned before accessing it
        if (labelText != null)
        {
            labelText.text = "Time's Up!";
        }

        // Defensive: stop the ticking sound before playing the alarm to avoid overlap.
        if (tickAudioSource != null && tickAudioSource.isPlaying)
        {
            tickAudioSource.Stop();
        }

        if (alarmAudioSource != null)
        {
            // Ensure alarm is unmuted right before playing
            UnmuteAlarm();
            alarmAudioSource.Stop(); // ensure clean start
            alarmAudioSource.Play();
            Debug.Log("Alarm played");
        }
    }

    // Public control for muting/unmuting the alarm at runtime
    public void MuteAlarm()
    {
        alarmMuted = true;
        if (alarmAudioSource != null) alarmAudioSource.mute = true;
    }

    public void UnmuteAlarm()
    {
        alarmMuted = false;
        if (alarmAudioSource != null) alarmAudioSource.mute = false;
    }

    public void ToggleAlarmMute()
    {
        alarmMuted = !alarmMuted;
        if (alarmAudioSource != null) alarmAudioSource.mute = alarmMuted;
    }

    public void PauseTimer()
    {
        isPaused = true;
    }

    public void ResumeTimer()
    {
        isPaused = false;
    }

    public void ResetTimer()
    {
        countDown = maxTime;
        isRunning = true;
        isPaused = false;
        lastWholeSecond = Mathf.FloorToInt(countDown);
        UpdateTimerText();
    }
}
