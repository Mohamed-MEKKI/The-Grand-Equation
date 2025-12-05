using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BankDesign : MonoBehaviour
{
    [Header("Main Panel")]
    public Image bankPanel;
    public Image topBorder;
    public Image bottomBorder;

    [Header("Header")]
    public Image leftDot;
    public Image rightDot;
    public TextMeshPro titleText;
    public TextMeshPro subtitleText;

    [Header("Safe/Coffre")]
    public Image safeDoor;
    public Image lockMechanism;
    public Image lockCenter;
    public Image[] lockMarks; // 4 marques
    public Image[] bolts; // 4 boulons
    public Image handle;
    public Image glowEffect;

    [Header("Info Panels")]
    public Image depositsPanel;
    public Image withdrawalsPanel;

    void Start()
    {
        SetupBankDesign();
    }

    void SetupBankDesign()
    {
        // Main panel gradient - Enhanced with richer tones
        Color panelTop = new Color(0.15f, 0.15f, 0.20f, 1f);
        Color panelBottom = new Color(0.08f, 0.08f, 0.12f, 1f);
        CreateGradient(bankPanel, panelTop, panelBottom);

        // Panel border - Multi-layered for depth
        Outline panelOutline = bankPanel.gameObject.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.25f, 0.25f, 0.30f, 0.8f);
        panelOutline.effectDistance = new Vector2(3, 3);

        // Add shadow for depth
        Shadow panelShadow = bankPanel.gameObject.GetComponent<Shadow>();
        if (panelShadow == null)
            panelShadow = bankPanel.gameObject.AddComponent<Shadow>();
        panelShadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        panelShadow.effectDistance = new Vector2(0, -4);

        // Decorative borders - Enhanced gold gradient with better opacity
        Color goldAccent = new Color(0.95f, 0.75f, 0.20f, 0.4f); // Brighter, more vibrant gold
        topBorder.color = goldAccent;
        CreateGradient(topBorder,
            new Color(0, 0, 0, 0),
            goldAccent,
            new Color(0, 0, 0, 0));
        bottomBorder.color = goldAccent;
        CreateGradient(bottomBorder,
            new Color(0, 0, 0, 0),
            goldAccent,
            new Color(0, 0, 0, 0));

        // Header dots - Enhanced with glow effect
        Color dotColor = new Color(0.98f, 0.82f, 0.25f, 1f); // Brighter gold
        leftDot.color = dotColor;
        rightDot.color = dotColor;
        
        // Add glow to dots
        Outline leftDotOutline = leftDot.gameObject.GetComponent<Outline>();
        if (leftDotOutline == null) leftDotOutline = leftDot.gameObject.AddComponent<Outline>();
        leftDotOutline.effectColor = new Color(0.95f, 0.75f, 0.20f, 0.6f);
        leftDotOutline.effectDistance = new Vector2(3, 3);
        
        Outline rightDotOutline = rightDot.gameObject.GetComponent<Outline>();
        if (rightDotOutline == null) rightDotOutline = rightDot.gameObject.AddComponent<Outline>();
        rightDotOutline.effectColor = new Color(0.95f, 0.75f, 0.20f, 0.6f);
        rightDotOutline.effectDistance = new Vector2(3, 3);

        // Title styling - Enhanced contrast and readability
        titleText.color = new Color(0.95f, 0.95f, 0.98f, 1f); // Near white for better contrast
        if (titleText.GetComponent<Outline>() == null)
        {
            Outline titleOutline = titleText.gameObject.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            titleOutline.effectDistance = new Vector2(2, 2);
        }
        
        subtitleText.color = new Color(0.65f, 0.65f, 0.72f, 1f); // Lighter for better readability
        if (subtitleText.GetComponent<Outline>() == null)
        {
            Outline subtitleOutline = subtitleText.gameObject.AddComponent<Outline>();
            subtitleOutline.effectColor = new Color(0f, 0f, 0f, 0.6f);
            subtitleOutline.effectDistance = new Vector2(1, 1);
        }

        // Safe door gradient - Enhanced metallic look
        Color safeTop = new Color(0.22f, 0.22f, 0.28f, 1f);
        Color safeBottom = new Color(0.14f, 0.14f, 0.18f, 1f);
        CreateGradient(safeDoor, safeTop, safeBottom);

        // Safe border - Enhanced with multiple effects
        Outline safeBorder = safeDoor.gameObject.GetComponent<Outline>();
        if (safeBorder == null) safeBorder = safeDoor.gameObject.AddComponent<Outline>();
        safeBorder.effectColor = new Color(0.35f, 0.35f, 0.42f, 0.9f);
        safeBorder.effectDistance = new Vector2(5, 5);
        
        Shadow safeShadow = safeDoor.gameObject.GetComponent<Shadow>();
        if (safeShadow == null) safeShadow = safeDoor.gameObject.AddComponent<Shadow>();
        safeShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        safeShadow.effectDistance = new Vector2(0, -6);

        // Lock mechanism gradient - Enhanced gold with better contrast
        Color lockTop = new Color(0.92f, 0.72f, 0.18f, 1f);
        Color lockBottom = new Color(0.78f, 0.60f, 0.12f, 1f);
        CreateGradient(lockMechanism, lockTop, lockBottom);

        // Lock border - Enhanced
        Outline lockBorder = lockMechanism.gameObject.GetComponent<Outline>();
        if (lockBorder == null) lockBorder = lockMechanism.gameObject.AddComponent<Outline>();
        lockBorder.effectColor = new Color(0.65f, 0.50f, 0.10f, 0.9f);
        lockBorder.effectDistance = new Vector2(5, 5);

        // Lock center - Enhanced contrast
        lockCenter.color = new Color(0.10f, 0.10f, 0.14f, 1f);
        Outline lockCenterBorder = lockCenter.gameObject.GetComponent<Outline>();
        if (lockCenterBorder == null) lockCenterBorder = lockCenter.gameObject.AddComponent<Outline>();
        lockCenterBorder.effectColor = new Color(0.55f, 0.42f, 0.08f, 0.9f);
        lockCenterBorder.effectDistance = new Vector2(3, 3);

        // Lock marks - Enhanced visibility
        foreach (Image mark in lockMarks)
        {
            if (mark != null)
            {
                mark.color = new Color(0.55f, 0.42f, 0.08f, 1f);
                Outline markOutline = mark.gameObject.GetComponent<Outline>();
                if (markOutline == null) markOutline = mark.gameObject.AddComponent<Outline>();
                markOutline.effectColor = new Color(0.35f, 0.27f, 0.05f, 0.6f);
                markOutline.effectDistance = new Vector2(1, 1);
            }
        }

        // Bolts - Enhanced metallic look
        foreach (Image bolt in bolts)
        {
            if (bolt != null)
            {
                bolt.color = new Color(0.50f, 0.50f, 0.58f, 1f);
                Outline boltOutline = bolt.gameObject.GetComponent<Outline>();
                if (boltOutline == null) boltOutline = bolt.gameObject.AddComponent<Outline>();
                boltOutline.effectColor = new Color(0.25f, 0.25f, 0.30f, 0.6f);
                boltOutline.effectDistance = new Vector2(2, 2);
            }
        }

        // Handle gradient - Enhanced
        Color handleLeft = new Color(0.80f, 0.62f, 0.15f, 1f);
        Color handleRight = new Color(0.92f, 0.72f, 0.18f, 1f);
        CreateGradientHorizontal(handle, handleLeft, handleRight);

        Outline handleBorder = handle.gameObject.GetComponent<Outline>();
        if (handleBorder == null) handleBorder = handle.gameObject.AddComponent<Outline>();
        handleBorder.effectColor = new Color(0.65f, 0.50f, 0.10f, 0.9f);
        handleBorder.effectDistance = new Vector2(3, 3);

        // Glow effect - Enhanced with better gradient
        CreateGradient(glowEffect,
            new Color(0.98f, 0.82f, 0.25f, 0.08f),
            new Color(0.95f, 0.75f, 0.20f, 0.12f),
            new Color(0, 0, 0, 0));

        // Info panels - Enhanced with better visibility
        Color panelColor = new Color(0.14f, 0.14f, 0.18f, 0.65f);
        depositsPanel.color = panelColor;
        withdrawalsPanel.color = panelColor;
        
        // Add subtle borders to info panels
        Outline depositsOutline = depositsPanel.gameObject.GetComponent<Outline>();
        if (depositsOutline == null) depositsOutline = depositsPanel.gameObject.AddComponent<Outline>();
        depositsOutline.effectColor = new Color(0.95f, 0.75f, 0.20f, 0.3f);
        depositsOutline.effectDistance = new Vector2(2, 2);
        
        Outline withdrawalsOutline = withdrawalsPanel.gameObject.GetComponent<Outline>();
        if (withdrawalsOutline == null) withdrawalsOutline = withdrawalsPanel.gameObject.AddComponent<Outline>();
        withdrawalsOutline.effectColor = new Color(0.95f, 0.75f, 0.20f, 0.3f);
        withdrawalsOutline.effectDistance = new Vector2(2, 2);
    }

    void CreateGradient(Image img, Color top, Color bottom)
    {
        Texture2D texture = new Texture2D(1, 128);
        for (int i = 0; i < 128; i++)
        {
            texture.SetPixel(0, i, Color.Lerp(bottom, top, i / 128f));
        }
        texture.Apply();
        img.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 128), Vector2.one * 0.5f);
    }

    void CreateGradient(Image img, Color left, Color middle, Color right)
    {
        Texture2D texture = new Texture2D(256, 1);
        for (int i = 0; i < 256; i++)
        {
            float t = i / 256f;
            Color color = t < 0.5f
                ? Color.Lerp(left, middle, t * 2f)
                : Color.Lerp(middle, right, (t - 0.5f) * 2f);
            texture.SetPixel(i, 0, color);
        }
        texture.Apply();
        img.sprite = Sprite.Create(texture, new Rect(0, 0, 256, 1), Vector2.one * 0.5f);
    }

    void CreateGradientHorizontal(Image img, Color left, Color right)
    {
        Texture2D texture = new Texture2D(128, 1);
        for (int i = 0; i < 128; i++)
        {
            texture.SetPixel(i, 0, Color.Lerp(left, right, i / 128f));
        }
        texture.Apply();
        img.sprite = Sprite.Create(texture, new Rect(0, 0, 128, 1), Vector2.one * 0.5f);
    }
}