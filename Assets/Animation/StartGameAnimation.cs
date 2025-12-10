using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Handles the game start animation with advanced DOTween features.
/// Uses sequences, text effects, and complex animation chains.
/// </summary>
public class StartGameAnimation : MonoBehaviour
{
    #region Fields
    [Header("UI References")]
    [SerializeField] private CanvasGroup blackScreen;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText; // Optional subtitle
    [SerializeField] private Image[] backgroundElements; // Optional background decorations

    [Header("Audio")]
    [SerializeField] private AudioSource epicMusic;

    [Header("Animation Settings")]
    [SerializeField] private float initialWaitDuration = 0.5f;
    [SerializeField] private float titleScaleDuration = 1.2f;
    [SerializeField] private float titleFadeDuration = 1.2f;
    [SerializeField] private float titleDisplayDuration = 2f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float targetTitleScale = 1.5f;
    [SerializeField] private string gameTitle = "COUP";
    [SerializeField] private string gameSubtitle = ""; // Optional subtitle text
    [SerializeField] private bool enableTypewriterEffect = false;
    [SerializeField] private bool enableGlowEffect = true;
    [SerializeField] private float glowIntensity = 1.5f;

    private bool hasPlayed = false;
    private Sequence mainSequence;
    private Tween glowTween;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        if (!hasPlayed)
        {
            PlayStartAnimation();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Plays the start animation sequence.
    /// </summary>
    public void PlayStartAnimation()
    {
        if (hasPlayed)
        {
            Debug.LogWarning("StartGameAnimation: Animation already played!");
            return;
        }

        if (!ValidateComponents())
        {
            Debug.LogError("StartGameAnimation: Missing required components! Cannot play animation.");
            return;
        }

        hasPlayed = true;
        CreateAnimationSequence();
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
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Creates the main DOTween sequence with all animations.
    /// </summary>
    private void CreateAnimationSequence()
    {
        // Initialize animation state
        blackScreen.alpha = 1f;
        titleText.text = gameTitle;
        titleText.transform.localScale = Vector3.zero;

        // Set initial alpha for title
        Color titleColor = titleText.color;
        titleColor.a = 0f;
        titleText.color = titleColor;

        // Initialize subtitle if exists
        if (subtitleText != null)
        {
            subtitleText.text = gameSubtitle;
            Color subtitleColor = subtitleText.color;
            subtitleColor.a = 0f;
            subtitleText.color = subtitleColor;
            subtitleText.transform.localScale = Vector3.zero;
        }

        // Initialize background elements
        if (backgroundElements != null && backgroundElements.Length > 0)
        {
            foreach (var element in backgroundElements)
            {
                if (element != null)
                {
                    element.color = new Color(element.color.r, element.color.g, element.color.b, 0f);
                    element.transform.localScale = Vector3.zero;
                }
            }
        }

        // Create main sequence
        mainSequence = DOTween.Sequence();
        mainSequence.SetAutoKill(false);
        mainSequence.SetRecyclable(true);

        // Step 1: Initial wait
        mainSequence.AppendInterval(initialWaitDuration);

        // Step 2: Play epic music
        mainSequence.AppendCallback(() => {
            if (epicMusic != null)
            {
                epicMusic.Play();
            }
            else
            {
                Debug.LogWarning("StartGameAnimation: Epic music AudioSource is null, no sound will play.");
            }
        });

        // Step 3: Animate background elements (if any)
        if (backgroundElements != null && backgroundElements.Length > 0)
        {
            mainSequence.AppendCallback(() => {
                AnimateBackgroundElements();
            });
        }

        // Step 4: Title scale in with elastic bounce
        mainSequence.Append(titleText.transform.DOScale(targetTitleScale, titleScaleDuration)
            .SetEase(Ease.OutElastic));

        // Step 5: Title fade in
        mainSequence.Join(titleText.DOFade(1f, titleFadeDuration)
            .SetEase(Ease.InQuad));

        // Step 6: Title rotation effect
        mainSequence.Join(titleText.transform.DOPunchRotation(
            new Vector3(0, 0, 15f),
            titleScaleDuration * 0.5f,
            5,
            0.5f)
            .SetEase(Ease.OutQuad));

        // Step 7: Start glow effect
        if (enableGlowEffect)
        {
            mainSequence.AppendCallback(() => {
                StartGlowEffect();
            });
        }

        // Step 8: Subtitle animation (if exists)
        if (subtitleText != null && !string.IsNullOrEmpty(gameSubtitle))
        {
            mainSequence.AppendCallback(() => {
                AnimateSubtitle();
            });
        }

        // Step 9: Wait for display duration
        mainSequence.AppendInterval(titleDisplayDuration);

        // Step 10: Fade out everything
        mainSequence.Append(blackScreen.DOFade(0f, fadeOutDuration)
            .SetEase(Ease.InQuad));

        mainSequence.Join(titleText.DOFade(0f, fadeOutDuration)
            .SetEase(Ease.InQuad));

        if (subtitleText != null)
        {
            mainSequence.Join(subtitleText.DOFade(0f, fadeOutDuration)
                .SetEase(Ease.InQuad));
        }

        // Step 11: Scale out title
        mainSequence.Join(titleText.transform.DOScale(0f, fadeOutDuration * 0.8f)
            .SetEase(Ease.InBack));

        // Step 12: OnComplete callback
        mainSequence.OnComplete(() => {
            gameObject.SetActive(false);
            NotifyGameStart();
        });

        // Play the sequence
        mainSequence.Play();
    }

    /// <summary>
    /// Animates background elements with a stagger effect.
    /// </summary>
    private void AnimateBackgroundElements()
    {
        if (backgroundElements == null) return;

        for (int i = 0; i < backgroundElements.Length; i++)
        {
            if (backgroundElements[i] == null) continue;

            float delay = i * 0.15f;

            // Fade in
            backgroundElements[i].DOFade(0.3f, 0.8f)
                .SetDelay(delay)
                .SetEase(Ease.OutQuad);

            // Scale in
            backgroundElements[i].transform.DOScale(1f, 0.6f)
                .SetDelay(delay)
                .SetEase(Ease.OutBack);

            // Continuous rotation
            backgroundElements[i].transform.DORotate(
                new Vector3(0, 0, 360f),
                3f,
                RotateMode.FastBeyond360)
                .SetDelay(delay)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }
    }

    /// <summary>
    /// Animates the subtitle text.
    /// </summary>
    private void AnimateSubtitle()
    {
        if (subtitleText == null) return;

        // Scale in
        subtitleText.transform.DOScale(1f, 0.5f)
            .SetEase(Ease.OutBack);

        // Fade in
        subtitleText.DOFade(1f, 0.5f)
            .SetEase(Ease.InQuad);

        // Typewriter effect (if enabled) - using custom coroutine since DOText doesn't work with TextMeshProUGUI
        if (enableTypewriterEffect && !string.IsNullOrEmpty(gameSubtitle))
        {
            StartCoroutine(TypewriterEffect(subtitleText, gameSubtitle, 1f));
        }
    }

    /// <summary>
    /// Custom typewriter effect coroutine for TextMeshProUGUI.
    /// </summary>
    private IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string fullText, float duration)
    {
        if (textComponent == null || string.IsNullOrEmpty(fullText))
            yield break;

        textComponent.text = "";
        int totalCharacters = fullText.Length;
        float delayPerCharacter = duration / totalCharacters;

        for (int i = 0; i <= totalCharacters; i++)
        {
            textComponent.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(delayPerCharacter);
        }
    }

    /// <summary>
    /// Starts a continuous glow effect on the title text.
    /// </summary>
    private void StartGlowEffect()
    {
        if (titleText == null) return;

        // Create a pulsing glow effect using color animation
        Color originalColor = titleText.color;
        Color glowColor = new Color(
            Mathf.Min(originalColor.r * glowIntensity, 1f),
            Mathf.Min(originalColor.g * glowIntensity, 1f),
            Mathf.Min(originalColor.b * glowIntensity, 1f),
            1f);

        glowTween = titleText.DOColor(glowColor, 1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetRecyclable(true);
    }

    /// <summary>
    /// Validates that all required components are assigned.
    /// </summary>
    private bool ValidateComponents()
    {
        if (blackScreen == null)
        {
            Debug.LogError("StartGameAnimation: 'blackScreen' CanvasGroup is not assigned!");
            return false;
        }

        if (titleText == null)
        {
            Debug.LogError("StartGameAnimation: 'titleText' TextMeshProUGUI is not assigned!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Notifies GameManager that the game can start.
    /// </summary>
    private void NotifyGameStart()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("StartGameAnimation: GameManager.Instance is null. Game will start automatically via GameManager.Start().");
            return;
        }

        // Note: StartGame() doesn't exist in GameManager yet
        // GameManager already starts automatically in its Start() method
        // This is a placeholder for future implementation if needed:
        // GameManager.Instance.StartGame();
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
