using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Script de design STYL� pour le bouton "Fin du Tour" 
/// Design myst�rieux avec effets de pulsation et glow rouge sang
/// </summary>
public class EndTurnButtonDesign : MonoBehaviour
{
    [Header("R�f�rences UI - Structure Complexe")]
    public Button endTurnButton;
    public Image outerGlow;          // Cercle ext�rieur qui pulse
    public Image buttonBackground;    // Background principal
    public Image innerGlow;          // Glow int�rieur
    public Image centerIcon;         // Ic�ne centrale (optionnelle)
    public TextMeshProUGUI buttonText;
    public Button button;

    [Header("Animation Settings")]
    public float pulseSpeed = 1.2f; // Slightly slower for smoother effect
    public float pulseIntensity = 0.25f; // More subtle pulse
    public bool enablePulseAnimation = true;

    private Vector3 originalScale;
    private Color glowColor = new Color(0.92f, 0.20f, 0.20f, 0.45f); // Brighter, more vibrant glow

    void Start()
    {
        SetupButtonDesign();
        if (enablePulseAnimation)
            StartCoroutine(PulseAnimation());
    }
    private void Awake()
    {
        // Try to get button from public field first, then from component
        Button btn = endTurnButton != null ? endTurnButton : 
                    (button != null ? button : GetComponent<Button>());
        
        if (btn == null)
        {
            Debug.LogError("EndTurnButtonDesign: No Button component found! Make sure this script is on a GameObject with a Button component, or assign 'endTurnButton' or 'button' in the Inspector.");
            return;
        }

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {

            // 100% SAFE � check everything exists
            // Timer reset and turn switching is handled by GameManager.EndTurn()
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndPlayerTurn();
                Debug.Log("player turn ended");
            }
            else
            {
                Debug.LogWarning("GameManager.Instance is null! Cannot end player turn.");
            }
        });

        // Store reference for later use
        if (endTurnButton == null)
            endTurnButton = btn;
    }

    void SetupButtonDesign()
    {
        originalScale = transform.localScale;

        // ===== OUTER GLOW - Halo rouge qui pulse =====
        if (outerGlow != null)
        {
            outerGlow.color = new Color(0.86f, 0.15f, 0.15f, 0.3f);

            // Outline �pais pour effet de glow
            Outline outerOutline = outerGlow.gameObject.GetComponent<Outline>();
            if (outerOutline == null)
                outerOutline = outerGlow.gameObject.AddComponent<Outline>();

            outerOutline.effectColor = new Color(0.86f, 0.15f, 0.15f, 0.6f);
            outerOutline.effectDistance = new Vector2(8, 8);

            // Shadow pour profondeur extr�me
            Shadow outerShadow = outerGlow.gameObject.GetComponent<Shadow>();
            if (outerShadow == null)
                outerShadow = outerGlow.gameObject.AddComponent<Shadow>();

            outerShadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outerShadow.effectDistance = new Vector2(0, 8);
        }

        // ===== BUTTON BACKGROUND - Enhanced gradient noir vers rouge =====
        if (buttonBackground != null)
        {
            buttonBackground.color = new Color(0.06f, 0.06f, 0.10f, 1f); // Slightly lighter for better visibility

            // Border rouge sang brillant - Enhanced
            Outline bgOutline = buttonBackground.gameObject.GetComponent<Outline>();
            if (bgOutline == null)
                bgOutline = buttonBackground.gameObject.AddComponent<Outline>();

            bgOutline.effectColor = new Color(0.95f, 0.25f, 0.25f, 1f); // Brighter red border
            bgOutline.effectDistance = new Vector2(5, 5); // Thicker border

            // Double shadow pour profondeur - Enhanced
            Shadow bgShadow = buttonBackground.gameObject.GetComponent<Shadow>();
            if (bgShadow == null)
                bgShadow = buttonBackground.gameObject.AddComponent<Shadow>();

            bgShadow.effectColor = new Color(0.92f, 0.20f, 0.20f, 0.6f); // Brighter shadow
            bgShadow.effectDistance = new Vector2(0, 8); // More depth
        }

        // ===== INNER GLOW - Lueur int�rieure =====
        if (innerGlow != null)
        {
            innerGlow.color = new Color(0.92f, 0.20f, 0.20f, 0.25f); // Brighter inner glow

            Outline innerOutline = innerGlow.gameObject.GetComponent<Outline>();
            if (innerOutline == null)
                innerOutline = innerGlow.gameObject.AddComponent<Outline>();

            innerOutline.effectColor = new Color(1f, 0.35f, 0.35f, 0.9f); // Brighter outline
            innerOutline.effectDistance = new Vector2(6, 6); // Larger glow
        }

        // ===== CENTER ICON - Ic�ne myst�rieuse (optionnelle) =====
        if (centerIcon != null)
        {
            centerIcon.color = new Color(0.95f, 0.30f, 0.30f, 0.9f); // Brighter icon

            Outline iconOutline = centerIcon.gameObject.GetComponent<Outline>();
            if (iconOutline == null)
                iconOutline = centerIcon.gameObject.AddComponent<Outline>();

            iconOutline.effectColor = new Color(1f, 1f, 1f, 0.95f); // Brighter white outline
            iconOutline.effectDistance = new Vector2(3, 3); // Thicker outline
            
            // Add glow to icon
            Shadow iconShadow = centerIcon.gameObject.GetComponent<Shadow>();
            if (iconShadow == null)
                iconShadow = centerIcon.gameObject.AddComponent<Shadow>();
            iconShadow.effectColor = new Color(0.95f, 0.30f, 0.30f, 0.5f);
            iconShadow.effectDistance = new Vector2(0, 2);
        }

        // ===== TEXTE - Styl� avec double outline =====
        if (buttonText != null)
        {
            buttonText.text = "END TURN";
            buttonText.color = Color.white;
            buttonText.fontSize = 28;
            buttonText.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.characterSpacing = 5; // Espacement des lettres pour effet �l�gant

            // Outline noir �pais
            Outline textOutline = buttonText.gameObject.GetComponent<Outline>();
            if (textOutline == null)
                textOutline = buttonText.gameObject.AddComponent<Outline>();

            textOutline.effectColor = new Color(0f, 0f, 0f, 1f);
            textOutline.effectDistance = new Vector2(3, 3);

            // Shadow rouge pour effet glow sur le texte
            Shadow textShadow = buttonText.gameObject.GetComponent<Shadow>();
            if (textShadow == null)
                textShadow = buttonText.gameObject.AddComponent<Shadow>();

            textShadow.effectColor = new Color(0.86f, 0.15f, 0.15f, 0.6f);
            textShadow.effectDistance = new Vector2(0, 3);
        }

        // ===== BUTTON STATES - Transitions de couleur =====
        if (endTurnButton != null)
        {
            ColorBlock colors = endTurnButton.colors;

            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.9f, 0.9f, 1f); // L�g�rement rouge
            colors.pressedColor = new Color(0.86f, 0.15f, 0.15f, 1f); // Rouge sang
            colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
            colors.fadeDuration = 0.15f;

            endTurnButton.colors = colors;
            endTurnButton.transition = Selectable.Transition.ColorTint;
        }
    }

    /// <summary>
    /// Animation de pulsation continue pour effet vivant
    /// </summary>
    IEnumerator PulseAnimation()
    {
        while (enablePulseAnimation)
        {
            // Smoother pulse using smoothstep for more natural animation
            float time = Time.time * pulseSpeed;
            float pulse = Mathf.Sin(time) * pulseIntensity;
            float smoothPulse = Mathf.SmoothStep(-pulseIntensity, pulseIntensity, (Mathf.Sin(time) + 1f) * 0.5f) * 0.5f;

            // Pulse sur le outer glow - Enhanced with smoother transitions
            if (outerGlow != null)
            {
                Color glowColorPulsed = glowColor;
                glowColorPulsed.a = glowColor.a + smoothPulse * 0.4f; // More dynamic alpha change
                outerGlow.color = Color.Lerp(outerGlow.color, glowColorPulsed, Time.deltaTime * 5f); // Smooth interpolation

                // Scale pulse - Smoother
                float scale = 1f + smoothPulse * 0.25f;
                outerGlow.transform.localScale = Vector3.Lerp(
                    outerGlow.transform.localScale, 
                    Vector3.one * scale, 
                    Time.deltaTime * 5f
                );
            }

            // Pulse subtil sur le bouton principal - Enhanced
            if (buttonBackground != null && endTurnButton != null && endTurnButton.interactable)
            {
                float scale = 1f + smoothPulse * 0.08f; // Slightly more noticeable
                transform.localScale = Vector3.Lerp(
                    transform.localScale,
                    originalScale * scale,
                    Time.deltaTime * 5f
                );
            }

            yield return null;
        }
    }

    /// <summary>
    /// Active/D�sactive le bouton avec effet visuel
    /// </summary>
    public void SetButtonInteractable(bool interactable)
    {
        if (endTurnButton != null)
        {
            endTurnButton.interactable = interactable;

            // D�sactive l'animation quand le bouton est disabled
            enablePulseAnimation = interactable;

            if (!interactable)
            {
                transform.localScale = originalScale;
                StopAllCoroutines();
            }
            else
            {
                StartCoroutine(PulseAnimation());
            }
        }
    }

    /// <summary>
    /// Animation de clic (appel�e depuis OnClick du Button)
    /// </summary>
    public void OnButtonClicked()
    {
        StartCoroutine(ClickAnimation());
    }

    IEnumerator ClickAnimation()
    {
        // Enhanced scale down with smooth easing
        float duration = 0.12f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easeOut = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease out
            float scale = Mathf.Lerp(1f, 0.88f, easeOut); // Slightly more scale down
            transform.localScale = originalScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Enhanced scale up with bounce effect
        elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easeOut = 1f - Mathf.Pow(1f - t, 2f); // Quadratic ease out
            float scale = Mathf.Lerp(0.88f, 1.05f, easeOut); // Slight overshoot for bounce
            transform.localScale = originalScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to normal with smooth transition
        elapsed = 0f;
        float returnDuration = 0.08f;
        while (elapsed < returnDuration)
        {
            float t = elapsed / returnDuration;
            float scale = Mathf.Lerp(1.05f, 1f, t);
            transform.localScale = originalScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
