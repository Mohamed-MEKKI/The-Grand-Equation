using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnTimer : MonoBehaviour
{
    public static TurnTimer Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public Image timerFill;

    [Header("Settings")]
    public float turnTime = 60f;

    private float remainingTime;
    private bool isRunning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Use this when starting a NEW turn
    public void StartNewTurn()
    {
        remainingTime = turnTime;
        isRunning = true;
        UpdateVisuals();
        Debug.Log($"Turn started: {turnTime}s");
    }

    public void StopTimer()
    {
        isRunning = false;
        UpdateVisuals();
    }

    // Use this when you want to fully reset without starting
    public void ResetTimer()
    {
        remainingTime = turnTime;
        isRunning = false;
        UpdateVisuals();
        Debug.Log("Timer fully reset (stopped)");
    }

    // Optional: Pause/resume
    public void PauseTimer() => isRunning = false;
    public void ResumeTimer() => isRunning = true;

    public bool IsRunning => isRunning;
    public float RemainingTime => remainingTime;

    private void Update()
    {
        if (!isRunning) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;
            UpdateVisuals();

            Debug.Log("Time's up!");
            GameManager.Instance?.EndPlayerTurn();
        }
        else
        {
            UpdateVisuals();
        }
    }

    void UpdateVisuals()
    {
        if (timerText != null)
        {
            int secs = Mathf.FloorToInt(remainingTime);
            timerText.text = secs.ToString("00"); // nicer: 09, 08...
            timerText.color = remainingTime < 10f ? Color.red : Color.white;
        }

        if (timerFill != null)
        {
            timerFill.fillAmount = Mathf.Clamp01(remainingTime / turnTime);
        }
    }

    // Optional: visual debug in editor
    private void OnValidate()
    {
        if (turnTime <= 0) turnTime = 60f;
    }
}