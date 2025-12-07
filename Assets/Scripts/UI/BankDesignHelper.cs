using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script to automatically create and position bank UI elements
/// Method 1: Right-click on this script in Inspector and select "Setup Bank UI Structure"
/// Method 2: Click the button below in the Inspector
/// </summary>
public class BankDesignHelper : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Click this button to automatically create the bank UI structure")]
    public bool setupBankUI = false;

#if UNITY_EDITOR
    [ContextMenu("Setup Bank UI Structure")]
    public void SetupBankUIStructure()
    {
        // Get or create main bank panel
        GameObject bankPanel = gameObject;
        RectTransform bankRT = bankPanel.GetComponent<RectTransform>();
        if (bankRT == null)
            bankRT = bankPanel.AddComponent<RectTransform>();

        // Setup main panel
        bankRT.anchorMin = new Vector2(0.5f, 0.5f);
        bankRT.anchorMax = new Vector2(0.5f, 0.5f);
        bankRT.pivot = new Vector2(0.5f, 0.5f);
        bankRT.sizeDelta = new Vector2(400, 600);
        bankRT.anchoredPosition = Vector2.zero;

        // Add Image component to main panel
        Image mainImage = bankPanel.GetComponent<Image>();
        if (mainImage == null)
            mainImage = bankPanel.AddComponent<Image>();
        mainImage.color = Color.white;

        // Add BankDesign script
        BankDesign bankDesign = bankPanel.GetComponent<BankDesign>();
        if (bankDesign == null)
            bankDesign = bankPanel.AddComponent<BankDesign>();

        // Create Top Border (Teller Counter)
        GameObject topBorder = CreateUIElement("TopBorder", bankPanel.transform);
        SetupRectTransform(topBorder, 0, 1, 1, 1, 0, -10, 0, 3);
        Image topBorderImg = topBorder.AddComponent<Image>();
        bankDesign.topBorder = topBorderImg;

        // Create Bottom Border (Foundation)
        GameObject bottomBorder = CreateUIElement("BottomBorder", bankPanel.transform);
        SetupRectTransform(bottomBorder, 0, 0, 1, 0, 0, 10, 0, 3);
        Image bottomBorderImg = bottomBorder.AddComponent<Image>();
        bankDesign.bottomBorder = bottomBorderImg;

        // Create Header Container
        GameObject headerContainer = CreateUIElement("Header", bankPanel.transform);
        SetupRectTransform(headerContainer, 0, 1, 1, 1, 0, -30, 0, 60);

        // Create Left Dot
        GameObject leftDot = CreateUIElement("LeftDot", headerContainer.transform);
        SetupRectTransform(leftDot, 0, 0.5f, 0, 0.5f, 20, 0, 20, 20);
        Image leftDotImg = leftDot.AddComponent<Image>();
        leftDotImg.color = Color.white;
        bankDesign.leftDot = leftDotImg;

        // Create Right Dot
        GameObject rightDot = CreateUIElement("RightDot", headerContainer.transform);
        SetupRectTransform(rightDot, 1, 0.5f, 1, 0.5f, -20, 0, 20, 20);
        Image rightDotImg = rightDot.AddComponent<Image>();
        rightDotImg.color = Color.white;
        bankDesign.rightDot = rightDotImg;

        // Create Title Text - Using TextMeshPro (3D text) as expected by BankDesign
        GameObject titleText = CreateUIElement("TitleText", headerContainer.transform);
        SetupRectTransform(titleText, 0.2f, 0.5f, 0.8f, 0.5f, 0, 0, 0, 40);
        
        // Note: BankDesign uses TextMeshPro (3D), but for UI we should use TextMeshProUGUI
        // However, keeping TextMeshPro to match BankDesign expectations
        // If you want UI text, change BankDesign.cs to use TextMeshProUGUI instead
        TextMeshPro titleTMP = titleText.AddComponent<TextMeshPro>();
        titleTMP.text = "BANK";
        titleTMP.fontSize = 32;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.fontStyle = FontStyles.Bold;
        bankDesign.titleText = titleTMP;

        // Create Safe/Vault Container (Center of panel)
        GameObject safeContainer = CreateUIElement("SafeContainer", bankPanel.transform);
        SetupRectTransform(safeContainer, 0.5f, 0.5f, 0.5f, 0.5f, 0, -50, 200, 250);

        // Create Safe Door
        GameObject safeDoor = CreateUIElement("SafeDoor", safeContainer.transform);
        SetupRectTransform(safeDoor, 0.5f, 0.5f, 0.5f, 0.5f, 0, 0, 180, 220);
        Image safeDoorImg = safeDoor.AddComponent<Image>();
        safeDoorImg.color = Color.white;
        bankDesign.safeDoor = safeDoorImg;

        // Create Lock Mechanism (Combination Dial)
        GameObject lockMechanism = CreateUIElement("LockMechanism", safeDoor.transform);
        SetupRectTransform(lockMechanism, 0.5f, 0.5f, 0.5f, 0.5f, 0, 20, 80, 80);
        Image lockMechImg = lockMechanism.AddComponent<Image>();
        lockMechImg.color = Color.white;
        bankDesign.lockMechanism = lockMechImg;

        // Create Lock Center
        GameObject lockCenter = CreateUIElement("LockCenter", lockMechanism.transform);
        SetupRectTransform(lockCenter, 0.5f, 0.5f, 0.5f, 0.5f, 0, 0, 30, 30);
        Image lockCenterImg = lockCenter.AddComponent<Image>();
        lockCenterImg.color = Color.white;
        bankDesign.lockCenter = lockCenterImg;

        // Create Lock Marks (4 marks around the dial)
        Image[] lockMarks = new Image[4];
        float[] markAngles = { 0f, 90f, 180f, 270f };
        for (int i = 0; i < 4; i++)
        {
            GameObject mark = CreateUIElement($"LockMark{i + 1}", lockMechanism.transform);
            float angle = markAngles[i] * Mathf.Deg2Rad;
            float radius = 35f;
            SetupRectTransform(mark, 0.5f, 0.5f, 0.5f, 0.5f, 
                Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 8, 8);
            Image markImg = mark.AddComponent<Image>();
            markImg.color = Color.white;
            lockMarks[i] = markImg;
        }
        bankDesign.lockMarks = lockMarks;

        // Create Bolts (4 bolts on vault door)
        Image[] bolts = new Image[4];
        Vector2[] boltPositions = {
            new Vector2(-70, 80),   // Top Left
            new Vector2(70, 80),    // Top Right
            new Vector2(-70, -80),  // Bottom Left
            new Vector2(70, -80)    // Bottom Right
        };
        for (int i = 0; i < 4; i++)
        {
            GameObject bolt = CreateUIElement($"Bolt{i + 1}", safeDoor.transform);
            SetupRectTransform(bolt, 0.5f, 0.5f, 0.5f, 0.5f, 
                boltPositions[i].x, boltPositions[i].y, 25, 25);
            Image boltImg = bolt.AddComponent<Image>();
            boltImg.color = Color.white;
            bolts[i] = boltImg;
        }
        bankDesign.bolts = bolts;

        // Create Handle
        GameObject handle = CreateUIElement("Handle", safeDoor.transform);
        SetupRectTransform(handle, 0.5f, 0.5f, 0.5f, 0.5f, 0, -60, 60, 20);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        bankDesign.handle = handleImg;

        // Create Glow Effect
        GameObject glowEffect = CreateUIElement("GlowEffect", safeDoor.transform);
        SetupRectTransform(glowEffect, 0, 0, 1, 1, 0, 0, 0, 0);
        Image glowImg = glowEffect.AddComponent<Image>();
        glowImg.color = new Color(1, 1, 1, 0.1f);
        bankDesign.glowEffect = glowImg;

        // Create Info Panels Container
        GameObject infoContainer = CreateUIElement("InfoPanels", bankPanel.transform);
        SetupRectTransform(infoContainer, 0, 0, 1, 0, 0, 20, 0, 100);

        // Create Deposits Panel
        GameObject depositsPanel = CreateUIElement("DepositsPanel", infoContainer.transform);
        SetupRectTransform(depositsPanel, 0, 0, 0.48f, 1, 0, 0, 0, 0);
        Image depositsImg = depositsPanel.AddComponent<Image>();
        depositsImg.color = Color.white;
        bankDesign.depositsPanel = depositsImg;

        // Create Withdrawals Panel
        GameObject withdrawalsPanel = CreateUIElement("WithdrawalsPanel", infoContainer.transform);
        SetupRectTransform(withdrawalsPanel, 0.52f, 0, 1, 1, 0, 0, 0, 0);
        Image withdrawalsImg = withdrawalsPanel.AddComponent<Image>();
        withdrawalsImg.color = Color.white;
        bankDesign.withdrawalsPanel = withdrawalsImg;

        // Assign main panel image
        bankDesign.bankPanel = mainImage;

#if UNITY_EDITOR
        // Mark scene as dirty so changes are saved
        EditorUtility.SetDirty(bankPanel);
        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
#endif

        Debug.Log("✅ Bank UI Structure created! All references are automatically assigned. Run the game to see the styled bank!");
    }
#endif

    // Alternative method - toggle the checkbox in Inspector to trigger setup
    void Update()
    {
#if UNITY_EDITOR
        if (setupBankUI && !Application.isPlaying)
        {
            setupBankUI = false; // Reset flag
            SetupBankUIStructure();
        }
#endif
    }

    GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    void SetupRectTransform(GameObject obj, float minX, float minY, float maxX, float maxY, 
        float posX, float posY, float width, float height)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(minX, minY);
        rt.anchorMax = new Vector2(maxX, maxY);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(posX, posY);
        rt.sizeDelta = new Vector2(width, height);
    }
}

