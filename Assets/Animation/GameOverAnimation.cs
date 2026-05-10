using TMPro;
using UnityEngine;
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
    [SerializeField] private GameObject matchResultCanvas;
    [SerializeField] private TextMeshProUGUI matchResultWinnerText;
    [SerializeField] private TextMeshProUGUI matchResultScoreText;

    [Header("Audio")]
    [SerializeField] private AudioSource victoryMusic;
    [SerializeField] private AudioSource defeatMusic;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.8f;
    [SerializeField] private float scaleAnimationDuration = 1.2f;
    [SerializeField] private float scaleOutDuration = 0.6f;
    [SerializeField] private float displayDuration = 4f;
    [SerializeField] private float targetScale = 1.4f;
    [SerializeField] private float glowPulseSpeed = 2f;
    [SerializeField] private float rotationAmount = 5f;
    [SerializeField] private bool enableGlowEffect = true;
    [SerializeField] private bool enableShakeEffect = true;

    public static GameOverAnimation Instance { get; private set; }
    private bool isPlaying = false;
    private Sequence animationSequence;
    private Tween glowTween;
    #endregion

    #region Initialization
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
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
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }

        if (glowTween != null && glowTween.IsActive())
        {
            glowTween.Kill();
        }

        isPlaying = false;
    }

    /// <summary>
    /// Hides the static match result canvas.
    /// </summary>
    public void HideMatchResultCanvas()
    {
        if (matchResultCanvas != null)
            matchResultCanvas.SetActive(false);
    }

    /// <summary>
    /// Shows the static match result canvas with winner and final score.
    /// </summary>
    public void ShowMatchResultCanvas(bool playerWon, int playerWins, int opponentWins)
    {
        // Ensure this component and parent hierarchy are active before toggling child UI.
        EnsureHierarchyActive();

        if (matchResultWinnerText != null)
            matchResultWinnerText.text = playerWon ? "PLAYER WINS" : "OPPONENT WINS";

        if (matchResultScoreText != null)
            matchResultScoreText.text = $"{playerWins} - {opponentWins}";

        if (matchResultCanvas != null)
        {
            // If the result canvas lives under an inactive parent object, force that chain active as well.
            Transform current = matchResultCanvas.transform;
            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                    current.gameObject.SetActive(true);
                current = current.parent;
            }

            matchResultCanvas.SetActive(true);
            CanvasGroup cg = matchResultCanvas.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }

            Debug.Log($"GameOverAnimation: Showing match result ({playerWins}-{opponentWins}).");
        }
        else
            Debug.LogWarning("GameOverAnimation: matchResultCanvas is not assigned.");
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Plays the game over animation with the specified parameters using DOTween sequences.
    /// </summary>
    private void PlayAnimation(string title, Color color, AudioSource sfx, string resultMessage)
    {
        if (isPlaying)
        {
            Debug.LogWarning("GameOverAnimation: Animation already playing!");
            return;
        }

        if (!ValidateComponents())
        {
            Debug.LogError("GameOverAnimation: Missing required components! Cannot play animation.");
            return;
        }

        StopAnimation();
        EnsureHierarchyActive();
        gameObject.SetActive(true);

        CreateAnimationSequence(title, color, sfx, resultMessage);
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

    }

    /// <summary>
    /// Creates the main DOTween sequence with all animations.
    /// </summary>
    private void CreateAnimationSequence(string title, Color baseColor, AudioSource sfx, string resultMessage)
    {
        isPlaying = true;
        InitializeAnimationState(title, baseColor, resultMessage);

        animationSequence = DOTween.Sequence();
        animationSequence.SetAutoKill(false);
        animationSequence.SetRecyclable(true);
        animationSequence.SetUpdate(true);

        // Step 1: Overlay fade in
        animationSequence.Append(screen.DOFade(1f, fadeInDuration)
            .SetEase(Ease.OutQuad));

        // Step 2: Title scale in
        animationSequence.Join(bigText.transform.DOScale(targetScale, scaleAnimationDuration)
            .SetEase(Ease.OutElastic)
            .OnStart(() => {
                if (sfx != null)
                {
                    sfx.Play();
                }
            }));

        // Step 3: Title punch rotation
        if (enableShakeEffect)
        {
            animationSequence.Join(bigText.transform.DOPunchRotation(
                new Vector3(0, 0, rotationAmount), 
                scaleAnimationDuration * 0.5f, 
                5, 
                0.5f)
                .SetEase(Ease.OutQuad));
        }

        // Step 4: Result text reveal
        animationSequence.AppendCallback(() => {
            resultText.DOFade(1f, 0.8f)
                .SetEase(Ease.InQuad)
                .SetDelay(0.3f)
                .SetUpdate(true);
        });

        // Step 5: Glow pulse during hold
        if (enableGlowEffect)
        {
            animationSequence.AppendCallback(() => {
                StartGlowPulse(baseColor);
            });
        }

        // Step 6: Hold
        animationSequence.AppendInterval(displayDuration);

        // Step 7: Fade out + title scale out (same rhythm as EndRoundAnimation)
        animationSequence.Append(screen.DOFade(0f, fadeOutDuration)
            .SetEase(Ease.InQuad));
        animationSequence.Join(bigText.transform.DOScale(0f, scaleOutDuration)
            .SetEase(Ease.InBack));
        animationSequence.Join(resultText.DOFade(0f, scaleOutDuration)
            .SetEase(Ease.InQuad));

        // Step 8: Complete
        animationSequence.OnComplete(() => {
            isPlaying = false;
            Debug.Log("GameOverAnimation: Animation sequence completed.");
        });

        animationSequence.Play();
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
            .SetRecyclable(true)
            .SetUpdate(true);
    }

    /// <summary>
    /// Activates this object and all parents so the animation can render.
    /// </summary>
    private void EnsureHierarchyActive()
    {
        Transform current = transform;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                current.gameObject.SetActive(true);
            }

            current = current.parent;
        }
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
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Pause();
        }

        if (glowTween != null && glowTween.IsActive())
        {
            glowTween.Pause();
        }
    }

    private void OnEnable()
    {
        // Resume animations when enabled
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Play();
        }

        if (glowTween != null && glowTween.IsActive())
        {
            glowTween.Play();
        }
    }
    #endregion
}
