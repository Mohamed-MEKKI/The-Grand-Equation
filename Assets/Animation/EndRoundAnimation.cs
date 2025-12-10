using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Handles the end-of-round animation with advanced DOTween features.
/// Uses sequences, staggered animations, and callback chains.
/// </summary>
public class EndRoundAnimation : MonoBehaviour
{
    #region Fields
    [Header("UI References")]
    [SerializeField] private CanvasGroup flash;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private Image[] decorativeElements; // Optional decorative images

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.8f;
    [SerializeField] private float scaleInDuration = 0.6f;
    [SerializeField] private float scaleOutDuration = 0.6f;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float targetScale = 1.3f;
    [SerializeField] private float maxFlashAlpha = 0.8f;
    [SerializeField] private float staggerDelay = 0.1f;
    [SerializeField] private bool enablePulseEffect = true;
    [SerializeField] private bool enableRotationEffect = true;

    private bool isPlaying = false;
    private Sequence animationSequence;
    private Tween pulseTween;
    #endregion

    #region Public Methods
    /// <summary>
    /// Plays the end-of-round animation for the specified round number.
    /// </summary>
    /// <param name="roundNumber">The round number that just ended.</param>
    public void Play(int roundNumber)
    {
        if (isPlaying)
        {
            Debug.LogWarning("EndRoundAnimation: Animation already playing!");
            return;
        }

        if (!ValidateComponents())
        {
            Debug.LogError("EndRoundAnimation: Missing required components! Cannot play animation.");
            return;
        }

        CreateAnimationSequence(roundNumber);
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

        if (pulseTween != null && pulseTween.IsActive())
        {
            pulseTween.Kill();
        }

        isPlaying = false;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Creates the main DOTween sequence with all animations.
    /// </summary>
    private void CreateAnimationSequence(int round)
    {
        isPlaying = true;

        // Initialize animation state
        roundText.text = $"ROUND {round} ENDED";
        flash.alpha = 0f;
        roundText.transform.localScale = Vector3.zero;
        roundText.transform.rotation = Quaternion.identity;

        // Initialize decorative elements
        if (decorativeElements != null && decorativeElements.Length > 0)
        {
            foreach (var element in decorativeElements)
            {
                if (element != null)
                {
                    element.transform.localScale = Vector3.zero;
                    element.color = new Color(element.color.r, element.color.g, element.color.b, 0f);
                }
            }
        }

        // Activate the game object
        gameObject.SetActive(true);

        // Create main sequence
        animationSequence = DOTween.Sequence();
        animationSequence.SetAutoKill(false);
        animationSequence.SetRecyclable(true);

        // Step 1: Flash fade in
        animationSequence.Append(flash.DOFade(maxFlashAlpha, fadeInDuration)
            .SetEase(Ease.OutQuad));

        // Step 2: Round text scale in with bounce
        animationSequence.Join(roundText.transform.DOScale(targetScale, scaleInDuration)
            .SetEase(Ease.OutBack));

        // Step 3: Round text rotation effect
        if (enableRotationEffect)
        {
            animationSequence.Join(roundText.transform.DOPunchRotation(
                new Vector3(0, 0, 10f),
                scaleInDuration * 0.5f,
                3,
                0.3f)
                .SetEase(Ease.OutQuad));
        }

        // Step 4: Staggered decorative elements animation
        if (decorativeElements != null && decorativeElements.Length > 0)
        {
            animationSequence.AppendCallback(() => {
                AnimateDecorativeElements();
            });
        }

        // Step 5: Pulse effect during display
        if (enablePulseEffect)
        {
            animationSequence.AppendCallback(() => {
                StartPulseEffect();
            });
        }

        // Step 6: Wait for display duration
        animationSequence.AppendInterval(displayDuration);

        // Step 7: Fade out flash
        animationSequence.Append(flash.DOFade(0f, fadeOutDuration)
            .SetEase(Ease.InQuad));

        // Step 8: Scale out text
        animationSequence.Join(roundText.transform.DOScale(0f, scaleOutDuration)
            .SetEase(Ease.InBack));

        // Step 9: Fade out decorative elements
        if (decorativeElements != null && decorativeElements.Length > 0)
        {
            animationSequence.JoinCallback(() => {
                foreach (var element in decorativeElements)
                {
                    if (element != null)
                    {
                        element.DOFade(0f, fadeOutDuration);
                        element.transform.DOScale(0f, scaleOutDuration);
                    }
                }
            });
        }

        // Step 10: OnComplete callback
        animationSequence.OnComplete(() => {
            gameObject.SetActive(false);
            isPlaying = false;
            NotifyRoundEnd();
        });

        // Play the sequence
        animationSequence.Play();
    }

    /// <summary>
    /// Animates decorative elements with a stagger effect.
    /// </summary>
    private void AnimateDecorativeElements()
    {
        if (decorativeElements == null) return;

        for (int i = 0; i < decorativeElements.Length; i++)
        {
            if (decorativeElements[i] == null) continue;

            float delay = i * staggerDelay;
            
            // Scale animation
            decorativeElements[i].transform.DOScale(1f, 0.4f)
                .SetDelay(delay)
                .SetEase(Ease.OutBack);

            // Fade animation
            decorativeElements[i].DOFade(1f, 0.4f)
                .SetDelay(delay)
                .SetEase(Ease.OutQuad);

            // Rotation animation
            decorativeElements[i].transform.DORotate(
                new Vector3(0, 0, 360f),
                0.6f,
                RotateMode.FastBeyond360)
                .SetDelay(delay)
                .SetEase(Ease.OutQuad);
        }
    }

    /// <summary>
    /// Starts a continuous pulse effect on the round text.
    /// </summary>
    private void StartPulseEffect()
    {
        if (roundText == null) return;

        // Create a pulsing scale effect
        pulseTween = roundText.transform.DOScale(targetScale * 1.1f, 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetRecyclable(true);
    }

    /// <summary>
    /// Validates that all required components are assigned.
    /// </summary>
    private bool ValidateComponents()
    {
        if (flash == null)
        {
            Debug.LogError("EndRoundAnimation: 'flash' CanvasGroup is not assigned!");
            return false;
        }

        if (roundText == null)
        {
            Debug.LogError("EndRoundAnimation: 'roundText' TextMeshProUGUI is not assigned!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Notifies GameManager that the round has ended.
    /// </summary>
    private void NotifyRoundEnd()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("EndRoundAnimation: GameManager.Instance is null. Cannot notify round end.");
            return;
        }

        // Note: StartNextRound() doesn't exist in GameManager yet
        // This is a placeholder for future implementation
        // Uncomment when the method is added to GameManager:
        // GameManager.Instance.StartNextRound();
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

        if (pulseTween != null && pulseTween.IsActive())
        {
            pulseTween.Pause();
        }
    }

    private void OnEnable()
    {
        // Resume animations when enabled
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Play();
        }

        if (pulseTween != null && pulseTween.IsActive())
        {
            pulseTween.Play();
        }
    }
    #endregion
}
