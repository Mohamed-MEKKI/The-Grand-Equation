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
        // Main panel gradient
        Color panelTop = new Color(0.12f, 0.12f, 0.16f);
        Color panelBottom = new Color(0.07f, 0.07f, 0.11f);
        CreateGradient(bankPanel, panelTop, panelBottom);

        // Panel border
        Outline panelOutline = bankPanel.gameObject.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.18f, 0.18f, 0.22f);
        panelOutline.effectDistance = new Vector2(2, 2);

        // Decorative borders
        topBorder.color = new Color(0.85f, 0.67f, 0.13f, 0.3f); // Yellow-600/30
        CreateGradient(topBorder,
            new Color(0, 0, 0, 0),
            new Color(0.85f, 0.67f, 0.13f, 0.3f),
            new Color(0, 0, 0, 0));
        bottomBorder.color = topBorder.color;

        // Header dots
        leftDot.color = new Color(0.92f, 0.77f, 0.13f); // Yellow-500
        rightDot.color = new Color(0.92f, 0.77f, 0.13f);

        // Title styling
        titleText.color = new Color(0.82f, 0.82f, 0.86f); // Slate-300
        subtitleText.color = new Color(0.4f, 0.4f, 0.47f); // Slate-500

        // Safe door gradient
        Color safeTop = new Color(0.18f, 0.18f, 0.22f);
        Color safeBottom = new Color(0.12f, 0.12f, 0.16f);
        CreateGradient(safeDoor, safeTop, safeBottom);

        // Safe border
        Outline safeBorder = safeDoor.gameObject.AddComponent<Outline>();
        safeBorder.effectColor = new Color(0.27f, 0.27f, 0.33f);
        safeBorder.effectDistance = new Vector2(4, 4);

        // Lock mechanism gradient
        Color lockTop = new Color(0.85f, 0.67f, 0.13f);
        Color lockBottom = new Color(0.75f, 0.57f, 0.08f);
        CreateGradient(lockMechanism, lockTop, lockBottom);

        // Lock border
        Outline lockBorder = lockMechanism.gameObject.AddComponent<Outline>();
        lockBorder.effectColor = new Color(0.55f, 0.42f, 0.05f);
        lockBorder.effectDistance = new Vector2(4, 4);

        // Lock center
        lockCenter.color = new Color(0.07f, 0.07f, 0.11f);
        Outline lockCenterBorder = lockCenter.gameObject.AddComponent<Outline>();
        lockCenterBorder.effectColor = new Color(0.45f, 0.32f, 0.04f);
        lockCenterBorder.effectDistance = new Vector2(2, 2);

        // Lock marks
        foreach (Image mark in lockMarks)
        {
            mark.color = new Color(0.45f, 0.32f, 0.04f);
        }

        // Bolts
        foreach (Image bolt in bolts)
        {
            bolt.color = new Color(0.4f, 0.4f, 0.47f);
        }

        // Handle gradient
        Color handleLeft = new Color(0.75f, 0.57f, 0.08f);
        Color handleRight = new Color(0.85f, 0.67f, 0.13f);
        CreateGradientHorizontal(handle, handleLeft, handleRight);

        Outline handleBorder = handle.gameObject.AddComponent<Outline>();
        handleBorder.effectColor = new Color(0.55f, 0.42f, 0.05f);
        handleBorder.effectDistance = new Vector2(2, 2);

        // Glow effect
        CreateGradient(glowEffect,
            new Color(0.92f, 0.77f, 0.13f, 0.05f),
            new Color(0, 0, 0, 0));

        // Info panels
        depositsPanel.color = new Color(0.12f, 0.12f, 0.16f, 0.5f);
        withdrawalsPanel.color = new Color(0.12f, 0.12f, 0.16f, 0.5f);
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