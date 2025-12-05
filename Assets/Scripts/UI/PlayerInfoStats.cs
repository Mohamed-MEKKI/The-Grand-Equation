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
        // Panel background
        panelBackground.color = new Color(0.07f, 0.07f, 0.11f, 0.8f);

        // Panel border
        Outline panelBorder = panelBackground.gameObject.GetComponent<Outline>();
        if (panelBorder == null)
            panelBorder = panelBackground.gameObject.AddComponent<Outline>();

        panelBorder.effectColor = new Color(0.45f, 0.09f, 0.09f, 0.3f);
        panelBorder.effectDistance = new Vector2(1, 1);

        // Avatar colors
        Color avatarColor = isPlayer
            ? new Color(0.86f, 0.11f, 0.18f) // Red-600 pour joueur
            : new Color(0.45f, 0.09f, 0.09f); // Red-900 pour adversaire

        avatarBackground.color = avatarColor;
        avatarBorder.color = new Color(0.86f, 0.11f, 0.18f);

        // Avatar border outline
        Outline avatarBorderOutline = avatarBorder.gameObject.GetComponent<Outline>();
        if (avatarBorderOutline == null)
            avatarBorderOutline = avatarBorder.gameObject.AddComponent<Outline>();

        avatarBorderOutline.effectColor = avatarColor;
        avatarBorderOutline.effectDistance = new Vector2(2, 2);

        // Stats dividers
        foreach (Image divider in statDividers)
        {
            if (divider != null)
                divider.color = new Color(0.45f, 0.09f, 0.09f, 0.5f);
        }

        // Stats icons
        CoinIcon.color = new Color(0.94f, 0.27f, 0.33f); // Red-500
        shieldIcon.color = new Color(0.38f, 0.71f, 0.98f); // Blue-400
    }
}