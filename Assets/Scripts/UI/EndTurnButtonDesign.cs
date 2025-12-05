using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Script de design STYLÉ pour le bouton "Fin du Tour" 
/// Design mystérieux avec effets de pulsation et glow rouge sang
/// </summary>
public class EndTurnButtonDesign : MonoBehaviour
{
    [Header("Références UI - Structure Complexe")]
    public Button endTurnButton;
    public Image outerGlow;          // Cercle extérieur qui pulse
    public Image buttonBackground;    // Background principal
    public Image innerGlow;          // Glow intérieur
    public Image centerIcon;         // Icône centrale (optionnelle)
    public TextMeshProUGUI buttonText;
    public Button button;

    [Header("Animation Settings")]
    public float pulseSpeed = 1.5f;
    public float pulseIntensity = 0.3f;
    public bool enablePulseAnimation = true;

    private Vector3 originalScale;
    private Color glowColor = new Color(0.86f, 0.15f, 0.15f, 0.4f);

    void Start()
    {
        SetupButtonDesign();
        if (enablePulseAnimation)
            StartCoroutine(PulseAnimation());
    }
    private void Awake()
    {
        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {

            // 100% SAFE — check everything exists
            if (TurnTimer.Instance != null)
            {
                TurnTimer.Instance.StopTimer();
                Debug.Log("time stopped");
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndPlayerTurn();
                Debug.Log("player turn ended");
            }
        });
    }

    void SetupButtonDesign()
    {
        originalScale = transform.localScale;

        // ===== OUTER GLOW - Halo rouge qui pulse =====
        if (outerGlow != null)
        {
            outerGlow.color = new Color(0.86f, 0.15f, 0.15f, 0.3f);

            // Outline épais pour effet de glow
            Outline outerOutline = outerGlow.gameObject.GetComponent<Outline>();
            if (outerOutline == null)
                outerOutline = outerGlow.gameObject.AddComponent<Outline>();

            outerOutline.effectColor = new Color(0.86f, 0.15f, 0.15f, 0.6f);
            outerOutline.effectDistance = new Vector2(8, 8);

            // Shadow pour profondeur extrême
            Shadow outerShadow = outerGlow.gameObject.GetComponent<Shadow>();
            if (outerShadow == null)
                outerShadow = outerGlow.gameObject.AddComponent<Shadow>();

            outerShadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outerShadow.effectDistance = new Vector2(0, 8);
        }

        // ===== BUTTON BACKGROUND - Gradient noir vers rouge =====
        if (buttonBackground != null)
        {
            buttonBackground.color = new Color(0.04f, 0.04f, 0.06f, 1f); // Noir profond

            // Border rouge sang brillant
            Outline bgOutline = buttonBackground.gameObject.GetComponent<Outline>();
            if (bgOutline == null)
                bgOutline = buttonBackground.gameObject.AddComponent<Outline>();

            bgOutline.effectColor = new Color(0.86f, 0.15f, 0.15f, 1f); // Rouge sang pur
            bgOutline.effectDistance = new Vector2(4, 4);

            // Double shadow pour profondeur
            Shadow bgShadow = buttonBackground.gameObject.GetComponent<Shadow>();
            if (bgShadow == null)
                bgShadow = buttonBackground.gameObject.AddComponent<Shadow>();

            bgShadow.effectColor = new Color(0.86f, 0.15f, 0.15f, 0.5f); // Shadow rouge
            bgShadow.effectDistance = new Vector2(0, 6);
        }

        // ===== INNER GLOW - Lueur intérieure =====
        if (innerGlow != null)
        {
            innerGlow.color = new Color(0.86f, 0.15f, 0.15f, 0.2f);

            Outline innerOutline = innerGlow.gameObject.GetComponent<Outline>();
            if (innerOutline == null)
                innerOutline = innerGlow.gameObject.AddComponent<Outline>();

            innerOutline.effectColor = new Color(1f, 0.3f, 0.3f, 0.8f);
            innerOutline.effectDistance = new Vector2(5, 5);
        }

        // ===== CENTER ICON - Icône mystérieuse (optionnelle) =====
        if (centerIcon != null)
        {
            centerIcon.color = new Color(0.86f, 0.15f, 0.15f, 0.8f);

            Outline iconOutline = centerIcon.gameObject.GetComponent<Outline>();
            if (iconOutline == null)
                iconOutline = centerIcon.gameObject.AddComponent<Outline>();

            iconOutline.effectColor = Color.white;
            iconOutline.effectDistance = new Vector2(2, 2);
        }

        // ===== TEXTE - Stylé avec double outline =====
        if (buttonText != null)
        {
            buttonText.text = "FIN DU TOUR";
            buttonText.color = Color.white;
            buttonText.fontSize = 28;
            buttonText.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.characterSpacing = 5; // Espacement des lettres pour effet élégant

            // Outline noir épais
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
            colors.highlightedColor = new Color(1f, 0.9f, 0.9f, 1f); // Légèrement rouge
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
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;

            // Pulse sur le outer glow
            if (outerGlow != null)
            {
                Color glowColorPulsed = glowColor;
                glowColorPulsed.a = glowColor.a + pulse * 0.3f;
                outerGlow.color = glowColorPulsed;

                // Scale pulse
                outerGlow.transform.localScale = Vector3.one * (1f + pulse * 0.2f);
            }

            // Pulse subtil sur le bouton principal
            if (buttonBackground != null && endTurnButton != null && endTurnButton.interactable)
            {
                transform.localScale = originalScale * (1f + pulse * 0.05f);
            }

            yield return null;
        }
    }

    /// <summary>
    /// Active/Désactive le bouton avec effet visuel
    /// </summary>
    public void SetButtonInteractable(bool interactable)
    {
        if (endTurnButton != null)
        {
            endTurnButton.interactable = interactable;

            // Désactive l'animation quand le bouton est disabled
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
    /// Animation de clic (appelée depuis OnClick du Button)
    /// </summary>
    public void OnButtonClicked()
    {
        StartCoroutine(ClickAnimation());
    }

    IEnumerator ClickAnimation()
    {
        // Scale down rapide
        float duration = 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float scale = Mathf.Lerp(1f, 0.9f, elapsed / duration);
            transform.localScale = originalScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Scale up rapide
        elapsed = 0f;
        while (elapsed < duration)
        {
            float scale = Mathf.Lerp(0.9f, 1f, elapsed / duration);
            transform.localScale = originalScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
