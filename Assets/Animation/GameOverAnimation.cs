using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Handles the game over screen animation (victory/defeat).
/// Uses advanced DOTween features: sequences, callbacks, loops, and complex animations.
/// </summary>
public class GameOverAnimation : MonoBehaviour
{
    #region Fields
    [Header("UI References")]
    [SerializeField] private CanvasGroup screen;
    [SerializeField] private TextMeshProUGUI bigText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Image backgroundImage; // Optional background image

    [Header("Audio")]
    [SerializeField] private AudioSource victoryMusic;
    [SerializeField] private AudioSource defeatMusic;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float scaleAnimationDuration = 1.2f;
    [SerializeField] private float displayDuration = 4f;
    [SerializeField] private float targetScale = 1.4f;
    [SerializeField] private float glowPulseSpeed = 2f;
    [SerializeField] private float rotationAmount = 5f;
    [SerializeField] private bool enableGlowEffect = true;
    [SerializeField] private bool enableShakeEffect = true;

    public static GameOverAnimation Instance { get; private set; }
    private bool isPlaying = false;
    private Sequence mainSequence;
    private Tween glowTween;
    #endregion

    #region Public Methods
    /// <summary>
    /// Shows the victory screen animation with advanced DOTween features.
    /// </summary>
    public void ShowVictory()
    {
        if (isPlaying)
        {
            Debug.LogWarning("GameOverAnimation: Animation already playing!");
            return;
        }

        PlayAnimation("YOU WIN!", Color.green, victoryMusic, "All opponent cards eliminated");
    }

    /// <summary>
    /// Shows the defeat screen animation with advanced DOTween features.
    /// </summary>
    public void ShowDefeat()
    {
        if (isPlaying)
        {
            Debug.LogWarning("GameOverAnimation: Animation already playing!");
            return;
        }

        PlayAnimation("YOU LOSE...", Color.red, defeatMusic, "You have no cards left");
    }

    /// <summary>
    /// Stops the current animation and resets the state.
    /// </summary>
    public void StopAnimation()
    {
        if (mainSequence != null && mainSequence.IsActive())
        {
            mainSequence.Kill();
        }

        if (glowTween != null && glowTween.IsActive())
        {
            glowTween.Kill();
        }

        isPlaying = false;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Plays the game over animation with the specified parameters using DOTween sequences.
    /// </summary>
    private void PlayAnimation(string title, Color color, AudioSource sfx, string resultMessage)
    {
        if (!ValidateComponents())
        {
            Debug.LogError("GameOverAnimation: Missing required components! Cannot play animation.");
            return;
        }

        // Kill any existing animations
        StopAnimation();

        // Initialize animation state
        InitializeAnimationState(title, color, resultMessage);

        // Activate the game object
        gameObject.SetActive(true);

        // Create main animation sequence
        CreateMainSequence(sfx, color);
    }

    /// <summary>
    /// Initializes all UI elements to their starting state.
    /// </summary>
    private void InitializeAnimationState(string title, Color color, string resultMessage)
    {
        screen.alpha = 0f;
        bigText.text = title;
        bigText.color = color;
        resultText.text = resultMessage;
        resultText.alpha = 0f;
        bigText.transform.localScale = Vector3.zero;
        bigText.transform.rotation = Quaternion.identity;

        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(color.r, color.g, color.b, 0f);
        }
    }

    /// <summary>
    /// Creates the main DOTween sequence with all animations.
    /// </summary>
    private void CreateMainSequence(AudioSource sfx, Color baseColor)
    {
        isPlaying = true;

        // Create sequence
        mainSequence = DOTween.Sequence();
        mainSequence.SetAutoKill(false);
        mainSequence.SetRecyclable(true);

        // Step 1: Fade in screen background
        mainSequence.Append(screen.DOFade(1f, fadeInDuration)
            .SetEase(Ease.OutQuad));

        // Step 2: Background color pulse (if background image exists)
        if (backgroundImage != null)
        {
            mainSequence.Join(backgroundImage.DOFade(0.3f, fadeInDuration)
                .SetEase(Ease.OutQuad));
        }

        // Step 3: Big text scale animation with elastic bounce
        mainSequence.Append(bigText.transform.DOScale(targetScale, scaleAnimationDuration)
            .SetEase(Ease.OutElastic)
            .OnStart(() => {
                // Play sound when text starts animating
                if (sfx != null)
                {
                    sfx.Play();
                }
            }));

        // Step 4: Big text rotation shake effect
        if (enableShakeEffect)
        {
            mainSequence.Join(bigText.transform.DOPunchRotation(
                new Vector3(0, 0, rotationAmount), 
                scaleAnimationDuration * 0.5f, 
                5, 
                0.5f)
                .SetEase(Ease.OutQuad));
        }

        // Step 5: Result text fade in with delay
        mainSequence.AppendCallback(() => {
            resultText.DOFade(1f, 0.8f)
                .SetEase(Ease.InQuad)
                .SetDelay(0.3f);
        });

        // Step 6: Continuous glow pulse effect
        if (enableGlowEffect)
        {
            mainSequence.AppendCallback(() => {
                StartGlowPulse(baseColor);
            });
        }

        // Step 7: Wait for display duration
        mainSequence.AppendInterval(displayDuration);

        // Step 8: OnComplete callback
        mainSequence.OnComplete(() => {
            isPlaying = false;
            Debug.Log("GameOverAnimation: Animation sequence completed.");
        });

        // Play the sequence
        mainSequence.Play();
    }

    /// <summary>
    /// Starts a continuous glow pulse effect on the big text.
    /// </summary>
    private void StartGlowPulse(Color baseColor)
    {
        if (bigText == null) return;

        // Create a pulsing glow effect using color animation
        glowTween = bigText.DOColor(
            new Color(baseColor.r * 1.3f, baseColor.g * 1.3f, baseColor.b * 1.3f, 1f),
            glowPulseSpeed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetRecyclable(true);
    }

    /// <summary>
    /// Validates that all required components are assigned.
    /// </summary>
    private bool ValidateComponents()
    {
        if (screen == null)
        {
            Debug.LogError("GameOverAnimation: 'screen' CanvasGroup is not assigned!");
            return false;
        }

        if (bigText == null)
        {
            Debug.LogError("GameOverAnimation: 'bigText' TextMeshProUGUI is not assigned!");
            return false;
        }

        if (resultText == null)
        {
            Debug.LogError("GameOverAnimation: 'resultText' TextMeshProUGUI is not assigned!");
            return false;
        }

        return true;
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        StopAnimation();
    }

    private void OnDisable()
    {
        // Pause animations when disabled
        if (mainSequence != null && mainSequence.IsActive())
        {
            mainSequence.Pause();
        }

        if (glowTween != null && glowTween.IsActive())
        {
            glowTween.Pause();
        }
    }

    private void OnEnable()
    {
        // Resume animations when enabled
        if (mainSequence != null && mainSequence.IsActive())
        {
            mainSequence.Play();
        }

        if (glowTween != null && glowTween.IsActive())
        {
            glowTween.Play();
        }
    }
    #endregion
}
