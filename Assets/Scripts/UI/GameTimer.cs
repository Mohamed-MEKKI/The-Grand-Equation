using UnityEngine;
using TMPro;
using System;


public class GameTimer : MonoBehaviour
{
    [Header("⏱️ Configuration")]
    [Tooltip("Temps maximum en secondes (0 = illimité)")]
    public float maxTime = 0f; // 0 = compte à l'infini

    [Tooltip("Décompte (compte à rebours) ou compte normal")]
    public bool countDown = false;

    [Header("🎨 UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI labelText; // "TEMPS" ou "TIME"

    [Header("⚙️ État")]
    public bool isRunning = true;
    public bool isPaused = false;

    // Variables internes
    private float currentTime = 0f;
    private Color normalColor = Color.white;
    private Color warningColor = new Color(1f, 0.5f, 0f, 1f); // Orange
    private Color dangerColor = new Color(0.9f, 0.1f, 0.1f, 1f); // Rouge sang

    void Start()
    {
        if (timerText == null)
        {
            Debug.LogError("❌ ERREUR: timerText n'est pas assigné dans l'Inspector!");
            return;
        }

        // Initialise le temps
        if (countDown && maxTime > 0)
        {
            currentTime = maxTime;
        }
        else
        {
            currentTime = 0f;
        }

        UpdateDisplay();
        Debug.Log($"✅ GameTimer initialisé - Mode: {(countDown ? "Décompte" : "Chronomètre")}");
    }

    void Update()
    {
        if (!isRunning || isPaused)
            return;

        // Update le temps
        if (countDown)
        {
            currentTime -= Time.deltaTime;

            // Temps écoulé
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                isRunning = false;
                OnTimeUp();
            }
        }
        else
        {
            currentTime += Time.deltaTime;

            // Si temps max défini
            if (maxTime > 0 && currentTime >= maxTime)
            {
                currentTime = maxTime;
                isRunning = false;
                OnTimeUp();
            }
        }

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (timerText == null)
            return;

        // Format: MM:SS
        TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime);
        string timeString = string.Format("{0:00}:{1:00}",
            Mathf.FloorToInt((float)timeSpan.TotalMinutes),
            timeSpan.Seconds);

        timerText.text = timeString;

        // Change la couleur selon le temps restant (en mode décompte)
        if (countDown && maxTime > 0)
        {
            float percentage = currentTime / maxTime;

            if (percentage <= 0.1f) // 10% restant
            {
                timerText.color = dangerColor;
                // Effet de pulsation
                float pulse = Mathf.PingPong(Time.time * 2f, 1f);
                timerText.color = Color.Lerp(dangerColor, Color.white, pulse * 0.3f);
            }
            else if (percentage <= 0.25f) // 25% restant
            {
                timerText.color = warningColor;
            }
            else
            {
                timerText.color = normalColor;
            }
        }
    }

    void OnTimeUp()
    {
        Debug.Log("⏰ Temps écoulé!");
        // Ici tu peux appeler un événement ou une fonction
        // Exemple: GameManager.Instance.OnTimerFinished();
    }

    // === MÉTHODES PUBLIQUES ===
    public void StartTimer()
    {
        isRunning = true;
        isPaused = false;

        if (countDown && maxTime > 0)
        {
            currentTime = maxTime;
        }
        else
        {
            currentTime = 0f;
        }

        Debug.Log("▶️ Timer démarré");
    }

    public void PauseTimer()
    {
        isPaused = true;
        Debug.Log("⏸️ Timer en pause");
    }


    public void ResumeTimer()
    {
        isPaused = false;
        Debug.Log("▶️ Timer repris");
    }

    /// <summary>
    /// Arrête le timer
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
        isPaused = false;
        Debug.Log("⏹️ Timer arrêté");
    }

    /// <summary>
    /// Reset le timer à 0 (ou maxTime si décompte)
    /// </summary>
    public void ResetTimer()
    {
        if (countDown && maxTime > 0)
        {
            currentTime = maxTime;
        }
        else
        {
            currentTime = 0f;
        }

        UpdateDisplay();
        Debug.Log("🔄 Timer réinitialisé");
    }

    /// <summary>
    /// Ajoute du temps (utile pour bonus)
    /// </summary>
    public void AddTime(float seconds)
    {
        if (countDown)
        {
            currentTime += seconds;
            if (maxTime > 0 && currentTime > maxTime)
                currentTime = maxTime;
        }
        else
        {
            currentTime += seconds;
        }

        Debug.Log($"➕ {seconds}s ajoutées");
    }

    public void RemoveTime(float seconds)
    {
        currentTime -= seconds;
        if (currentTime < 0f)
            currentTime = 0f;

        Debug.Log($"➖ {seconds}s retirées");
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public string GetFormattedTime()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime);
        return string.Format("{0:00}:{1:00}",
            Mathf.FloorToInt((float)timeSpan.TotalMinutes),
            timeSpan.Seconds);
    }
}
