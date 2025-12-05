using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoPanelDesign : MonoBehaviour
{
    [Header("Panel Settings")]
    public bool isPlayer = true; // true = Joueur, false = Adversaire

    [Header("UI References")]
    public Image panelBackground;
    public Image avatarBackground;
    public Image avatarBorder;
    public Image[] statDividers;
    public Image CoinIcon;
    public Image shieldIcon;

    void Start()
    {
        SetupDesign();
    }

    void SetupDesign()
    {
        // Panel background - Enhanced with richer color
        panelBackground.color = new Color(0.10f, 0.10f, 0.14f, 0.88f); // Richer, more visible

        // Panel border - Enhanced with better visibility
        Outline panelBorder = panelBackground.gameObject.GetComponent<Outline>();
        if (panelBorder == null)
            panelBorder = panelBackground.gameObject.AddComponent<Outline>();

        panelBorder.effectColor = new Color(0.86f, 0.15f, 0.15f, 0.5f); // Brighter red border
        panelBorder.effectDistance = new Vector2(2, 2); // Thicker border
        
        // Add shadow for depth
        Shadow panelShadow = panelBackground.gameObject.GetComponent<Shadow>();
        if (panelShadow == null)
            panelShadow = panelBackground.gameObject.AddComponent<Shadow>();
        panelShadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        panelShadow.effectDistance = new Vector2(0, -4);

        // Avatar colors - Enhanced with better contrast
        Color avatarColor = isPlayer
            ? new Color(0.92f, 0.20f, 0.25f, 1f) // Brighter red-600 pour joueur
            : new Color(0.55f, 0.12f, 0.12f, 1f); // Lighter red-900 pour adversaire

        avatarBackground.color = avatarColor;
        avatarBorder.color = isPlayer 
            ? new Color(0.95f, 0.30f, 0.35f, 1f) // Brighter for player
            : new Color(0.70f, 0.15f, 0.15f, 1f); // Lighter for opponent

        // Avatar border outline - Enhanced
        Outline avatarBorderOutline = avatarBorder.gameObject.GetComponent<Outline>();
        if (avatarBorderOutline == null)
            avatarBorderOutline = avatarBorder.gameObject.AddComponent<Outline>();

        avatarBorderOutline.effectColor = new Color(avatarColor.r, avatarColor.g, avatarColor.b, 0.8f);
        avatarBorderOutline.effectDistance = new Vector2(3, 3); // Thicker outline
        
        // Add glow effect to avatar
        Shadow avatarShadow = avatarBorder.gameObject.GetComponent<Shadow>();
        if (avatarShadow == null)
            avatarShadow = avatarBorder.gameObject.AddComponent<Shadow>();
        avatarShadow.effectColor = new Color(avatarColor.r, avatarColor.g, avatarColor.b, 0.5f);
        avatarShadow.effectDistance = new Vector2(0, 3);

        // Stats dividers - Enhanced visibility
        foreach (Image divider in statDividers)
        {
            if (divider != null)
            {
                divider.color = new Color(0.86f, 0.15f, 0.15f, 0.6f); // Brighter, more visible
                
                // Add subtle outline to dividers
                Outline dividerOutline = divider.gameObject.GetComponent<Outline>();
                if (dividerOutline == null)
                    dividerOutline = divider.gameObject.AddComponent<Outline>();
                dividerOutline.effectColor = new Color(0.45f, 0.09f, 0.09f, 0.4f);
                dividerOutline.effectDistance = new Vector2(1, 1);
            }
        }

        // Stats icons - Enhanced with glow effects
        if (CoinIcon != null)
        {
            CoinIcon.color = new Color(0.98f, 0.35f, 0.40f, 1f); // Brighter red-500
            
            // Add glow to coin icon
            Outline coinOutline = CoinIcon.gameObject.GetComponent<Outline>();
            if (coinOutline == null)
                coinOutline = CoinIcon.gameObject.AddComponent<Outline>();
            coinOutline.effectColor = new Color(0.98f, 0.35f, 0.40f, 0.6f);
            coinOutline.effectDistance = new Vector2(2, 2);
        }
        
        if (shieldIcon != null)
        {
            shieldIcon.color = new Color(0.45f, 0.78f, 1f, 1f); // Brighter blue-400
            
            // Add glow to shield icon
            Outline shieldOutline = shieldIcon.gameObject.GetComponent<Outline>();
            if (shieldOutline == null)
                shieldOutline = shieldIcon.gameObject.AddComponent<Outline>();
            shieldOutline.effectColor = new Color(0.45f, 0.78f, 1f, 0.6f);
            shieldOutline.effectDistance = new Vector2(2, 2);
        }
    }
}