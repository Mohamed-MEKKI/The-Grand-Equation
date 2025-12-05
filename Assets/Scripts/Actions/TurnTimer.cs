using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnTimer : MonoBehaviour
{
    public static TurnTimer Instance { get; private set; }   // ← MUST BE public static

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public Image timerFill;

    public float turnTime = 60f;
    private float remainingTime;
    private bool isRunning = false;

    private void Awake()
    {
        // THIS IS THE CRITICAL PART
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;                    // ← THIS LINE WAS MISSING OR WRONG
        DontDestroyOnLoad(gameObject);      // Optional: survive scene reloads
    }

    public void StartTimer()
    {
        remainingTime = turnTime;
        isRunning = true;
        UpdateVisuals();
    }

    public void StopTimer() => isRunning = false;

    private void Update()
    {
        if (!isRunning) return;
        remainingTime -= Time.deltaTime;
        UpdateVisuals();

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;
            GameManager.Instance.EndPlayerTurn();
        }
    }

    void UpdateVisuals()
    {
        int secs = Mathf.FloorToInt(remainingTime);
        if (timerText) timerText.text = secs.ToString();
        if (timerFill) timerFill.fillAmount = remainingTime / turnTime;
        if (timerText) timerText.color = remainingTime < 10f ? Color.red : Color.white;
    }
}