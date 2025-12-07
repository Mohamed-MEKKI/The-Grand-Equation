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
        if (bankPanel == null)
        {
            Debug.LogError("BankDesign: bankPanel is not assigned! Please assign the main panel Image in the Inspector.");
            return;
        }

        // Main panel - Bank building appearance (stone/marble texture effect)
        // Darker, more solid bank building colors
        Color panelTop = new Color(0.20f, 0.18f, 0.16f, 1f); // Warm stone top
        Color panelBottom = new Color(0.12f, 0.10f, 0.08f, 1f); // Darker stone bottom
        CreateGradient(bankPanel, panelTop, panelBottom);

        // Panel border - Thick, solid bank building border
        Outline panelOutline = bankPanel.gameObject.GetComponent<Outline>();
        if (panelOutline == null)
            panelOutline = bankPanel.gameObject.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.35f, 0.30f, 0.25f, 1f); // Stone border
        panelOutline.effectDistance = new Vector2(4, 4); // Thicker border for solid appearance

        // Add deep shadow for building depth
        Shadow panelShadow = bankPanel.gameObject.GetComponent<Shadow>();
        if (panelShadow == null)
            panelShadow = bankPanel.gameObject.AddComponent<Shadow>();
        panelShadow.effectColor = new Color(0f, 0f, 0f, 0.8f); // Deeper shadow
        panelShadow.effectDistance = new Vector2(0, -6); // More depth

        // Decorative borders - Bank counter/teller window style (brass/copper)
        Color brassAccent = new Color(0.85f, 0.65f, 0.35f, 0.9f); // Rich brass/copper for bank counter
        
        if (topBorder != null)
        {
            // Top border - Bank counter top (solid brass bar)
            topBorder.color = brassAccent;
            CreateGradient(topBorder,
                new Color(0.75f, 0.55f, 0.25f, 0.9f), // Darker brass
                brassAccent,
                new Color(0.75f, 0.55f, 0.25f, 0.9f));
        }
        
        if (bottomBorder != null)
        {
            // Bottom border - Bank foundation/base (solid stone)
            bottomBorder.color = new Color(0.30f, 0.25f, 0.20f, 0.95f); // Dark stone foundation
            CreateGradient(bottomBorder,
                new Color(0.25f, 0.20f, 0.15f, 0.95f),
                new Color(0.30f, 0.25f, 0.20f, 0.95f),
                new Color(0.25f, 0.20f, 0.15f, 0.95f));
        }

        // Header dots - Bank window/ventilation elements (brass fixtures)
        Color fixtureColor = new Color(0.80f, 0.60f, 0.30f, 1f); // Brass fixture color
        
        if (leftDot != null)
        {
            leftDot.color = fixtureColor;
            Outline leftDotOutline = leftDot.gameObject.GetComponent<Outline>();
            if (leftDotOutline == null) leftDotOutline = leftDot.gameObject.AddComponent<Outline>();
            leftDotOutline.effectColor = new Color(0.60f, 0.45f, 0.20f, 0.8f); // Darker brass outline
            leftDotOutline.effectDistance = new Vector2(2, 2);
            
            // Add shadow for 3D fixture effect
            Shadow leftDotShadow = leftDot.gameObject.GetComponent<Shadow>();
            if (leftDotShadow == null) leftDotShadow = leftDot.gameObject.AddComponent<Shadow>();
            leftDotShadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            leftDotShadow.effectDistance = new Vector2(2, -2);
        }
        
        if (rightDot != null)
        {
            rightDot.color = fixtureColor;
            Outline rightDotOutline = rightDot.gameObject.GetComponent<Outline>();
            if (rightDotOutline == null) rightDotOutline = rightDot.gameObject.AddComponent<Outline>();
            rightDotOutline.effectColor = new Color(0.60f, 0.45f, 0.20f, 0.8f);
            rightDotOutline.effectDistance = new Vector2(2, 2);
            
            Shadow rightDotShadow = rightDot.gameObject.GetComponent<Shadow>();
            if (rightDotShadow == null) rightDotShadow = rightDot.gameObject.AddComponent<Shadow>();
            rightDotShadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            rightDotShadow.effectDistance = new Vector2(2, -2);
        }

        // Title styling - Bank sign appearance (brass/engraved letters)
        if (titleText != null)
        {
            titleText.color = new Color(0.90f, 0.75f, 0.40f, 1f); // Brass/gold bank sign color
            if (titleText.GetComponent<Outline>() == null)
            {
                Outline titleOutline = titleText.gameObject.AddComponent<Outline>();
                titleOutline.effectColor = new Color(0.50f, 0.35f, 0.15f, 1f); // Dark brass outline for engraved effect
                titleOutline.effectDistance = new Vector2(3, 3);
            }
            
            // Add shadow for 3D engraved sign effect
            Shadow titleShadow = titleText.gameObject.GetComponent<Shadow>();
            if (titleShadow == null) titleShadow = titleText.gameObject.AddComponent<Shadow>();
            titleShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
            titleShadow.effectDistance = new Vector2(2, -2);
        }
        
        if (subtitleText != null)
        {
            subtitleText.color = new Color(0.75f, 0.65f, 0.50f, 1f); // Lighter brass for subtitle
            if (subtitleText.GetComponent<Outline>() == null)
            {
                Outline subtitleOutline = subtitleText.gameObject.AddComponent<Outline>();
                subtitleOutline.effectColor = new Color(0.40f, 0.30f, 0.15f, 0.9f); // Darker brass outline
                subtitleOutline.effectDistance = new Vector2(2, 2);
            }
        }

        // Safe door - Realistic bank vault door (heavy steel)
        if (safeDoor != null)
        {
            // Dark steel vault door with metallic gradient
            Color safeTop = new Color(0.25f, 0.25f, 0.28f, 1f); // Lighter steel at top
            Color safeBottom = new Color(0.15f, 0.15f, 0.18f, 1f); // Darker steel at bottom
            CreateGradient(safeDoor, safeTop, safeBottom);

            // Thick steel border - Bank vault door frame
            Outline safeBorder = safeDoor.gameObject.GetComponent<Outline>();
            if (safeBorder == null) safeBorder = safeDoor.gameObject.AddComponent<Outline>();
            safeBorder.effectColor = new Color(0.40f, 0.40f, 0.45f, 1f); // Steel frame
            safeBorder.effectDistance = new Vector2(6, 6); // Very thick for heavy vault door
            
            // Deep inset shadow - Vault door is recessed
            Shadow safeShadow = safeDoor.gameObject.GetComponent<Shadow>();
            if (safeShadow == null) safeShadow = safeDoor.gameObject.AddComponent<Shadow>();
            safeShadow.effectColor = new Color(0f, 0f, 0f, 0.9f); // Very dark shadow
            safeShadow.effectDistance = new Vector2(0, -8); // Deep inset
        }

        // Lock mechanism - Bank vault combination dial (brass/gold)
        if (lockMechanism != null)
        {
            // Brass combination dial with metallic shine
            Color lockTop = new Color(0.88f, 0.68f, 0.25f, 1f); // Bright brass top
            Color lockBottom = new Color(0.70f, 0.50f, 0.15f, 1f); // Darker brass bottom
            CreateGradient(lockMechanism, lockTop, lockBottom);

            // Lock border - Brass dial rim
            Outline lockBorder = lockMechanism.gameObject.GetComponent<Outline>();
            if (lockBorder == null) lockBorder = lockMechanism.gameObject.AddComponent<Outline>();
            lockBorder.effectColor = new Color(0.60f, 0.45f, 0.15f, 1f); // Dark brass rim
            lockBorder.effectDistance = new Vector2(4, 4);
            
            // Shadow for 3D dial effect
            Shadow lockShadow = lockMechanism.gameObject.GetComponent<Shadow>();
            if (lockShadow == null) lockShadow = lockMechanism.gameObject.AddComponent<Shadow>();
            lockShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
            lockShadow.effectDistance = new Vector2(0, -3);
        }

        // Lock center - Vault dial center (dark with brass ring)
        if (lockCenter != null)
        {
            lockCenter.color = new Color(0.08f, 0.08f, 0.10f, 1f); // Very dark center
            Outline lockCenterBorder = lockCenter.gameObject.GetComponent<Outline>();
            if (lockCenterBorder == null) lockCenterBorder = lockCenter.gameObject.AddComponent<Outline>();
            lockCenterBorder.effectColor = new Color(0.75f, 0.55f, 0.20f, 1f); // Brass ring
            lockCenterBorder.effectDistance = new Vector2(3, 3);
        }

        // Lock marks - Combination dial numbers/marks (brass)
        foreach (Image mark in lockMarks)
        {
            if (mark != null)
            {
                mark.color = new Color(0.85f, 0.65f, 0.30f, 1f); // Bright brass marks
                Outline markOutline = mark.gameObject.GetComponent<Outline>();
                if (markOutline == null) markOutline = mark.gameObject.AddComponent<Outline>();
                markOutline.effectColor = new Color(0.50f, 0.35f, 0.10f, 0.8f); // Darker brass outline
                markOutline.effectDistance = new Vector2(1, 1);
            }
        }

        // Bolts - Heavy steel vault door bolts
        foreach (Image bolt in bolts)
        {
            if (bolt != null)
            {
                bolt.color = new Color(0.35f, 0.35f, 0.40f, 1f); // Dark steel bolts
                Outline boltOutline = bolt.gameObject.GetComponent<Outline>();
                if (boltOutline == null) boltOutline = bolt.gameObject.AddComponent<Outline>();
                boltOutline.effectColor = new Color(0.50f, 0.50f, 0.55f, 0.9f); // Lighter steel rim
                boltOutline.effectDistance = new Vector2(2, 2);
                
                // Shadow for 3D bolt effect
                Shadow boltShadow = bolt.gameObject.GetComponent<Shadow>();
                if (boltShadow == null) boltShadow = bolt.gameObject.AddComponent<Shadow>();
                boltShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
                boltShadow.effectDistance = new Vector2(1, -2);
            }
        }

        // Handle - Bank vault door handle (heavy brass)
        if (handle != null)
        {
            // Brass handle with metallic gradient
            Color handleLeft = new Color(0.75f, 0.55f, 0.20f, 1f); // Darker brass
            Color handleRight = new Color(0.90f, 0.70f, 0.30f, 1f); // Brighter brass
            CreateGradientHorizontal(handle, handleLeft, handleRight);

            Outline handleBorder = handle.gameObject.GetComponent<Outline>();
            if (handleBorder == null) handleBorder = handle.gameObject.AddComponent<Outline>();
            handleBorder.effectColor = new Color(0.55f, 0.40f, 0.12f, 1f); // Dark brass border
            handleBorder.effectDistance = new Vector2(3, 3);
            
            // Shadow for 3D handle
            Shadow handleShadow = handle.gameObject.GetComponent<Shadow>();
            if (handleShadow == null) handleShadow = handle.gameObject.AddComponent<Shadow>();
            handleShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            handleShadow.effectDistance = new Vector2(0, -4);
        }

        // Glow effect - Subtle bank vault lighting (warm, dim)
        if (glowEffect != null)
        {
            CreateGradient(glowEffect,
                new Color(0.85f, 0.70f, 0.40f, 0.06f), // Warm brass glow top
                new Color(0.75f, 0.60f, 0.30f, 0.10f), // Brighter center
                new Color(0, 0, 0, 0)); // Fade to transparent
        }

        // Info panels - Bank teller window/info boards (wood/brass)
        Color panelColor = new Color(0.18f, 0.15f, 0.12f, 0.85f); // Dark wood panel color
        
        if (depositsPanel != null)
        {
            depositsPanel.color = panelColor;
            Outline depositsOutline = depositsPanel.gameObject.GetComponent<Outline>();
            if (depositsOutline == null) depositsOutline = depositsPanel.gameObject.AddComponent<Outline>();
            depositsOutline.effectColor = new Color(0.70f, 0.55f, 0.30f, 0.6f); // Brass frame
            depositsOutline.effectDistance = new Vector2(3, 3);
            
            Shadow depositsShadow = depositsPanel.gameObject.GetComponent<Shadow>();
            if (depositsShadow == null) depositsShadow = depositsPanel.gameObject.AddComponent<Shadow>();
            depositsShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            depositsShadow.effectDistance = new Vector2(0, -2);
        }
        
        if (withdrawalsPanel != null)
        {
            withdrawalsPanel.color = panelColor;
            Outline withdrawalsOutline = withdrawalsPanel.gameObject.GetComponent<Outline>();
            if (withdrawalsOutline == null) withdrawalsOutline = withdrawalsPanel.gameObject.AddComponent<Outline>();
            withdrawalsOutline.effectColor = new Color(0.70f, 0.55f, 0.30f, 0.6f); // Brass frame
            withdrawalsOutline.effectDistance = new Vector2(3, 3);
            
            Shadow withdrawalsShadow = withdrawalsPanel.gameObject.GetComponent<Shadow>();
            if (withdrawalsShadow == null) withdrawalsShadow = withdrawalsPanel.gameObject.AddComponent<Shadow>();
            withdrawalsShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            withdrawalsShadow.effectDistance = new Vector2(0, -2);
        }
        
        Debug.Log("BankDesign: Setup completed successfully!");
    }

    void CreateGradient(Image img, Color top, Color bottom)
    {
        if (img == null)
        {
            Debug.LogWarning("BankDesign: Cannot create gradient - Image is null!");
            return;
        }

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
        if (img == null)
        {
            Debug.LogWarning("BankDesign: Cannot create gradient - Image is null!");
            return;
        }

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
        if (img == null)
        {
            Debug.LogWarning("BankDesign: Cannot create gradient - Image is null!");
            return;
        }

        Texture2D texture = new Texture2D(128, 1);
        for (int i = 0; i < 128; i++)
        {
            texture.SetPixel(i, 0, Color.Lerp(left, right, i / 128f));
        }
        texture.Apply();
        img.sprite = Sprite.Create(texture, new Rect(0, 0, 128, 1), Vector2.one * 0.5f);
    }
}