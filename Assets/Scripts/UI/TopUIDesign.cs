using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel du haut pour ELMAKINA
/// Contient: Timer (gauche), Score/Info (centre), Pause (droite)
/// Ce script crée automatiquement tous les éléments UI
/// </summary>
public class TopPanelUI : MonoBehaviour
{
    [Header("?? References (Auto-créées)")]
    public GameObject timerPanel;
    public GameObject pauseButton;
    public GameObject centerInfoPanel;

    [Header("?? Components")]
    public GameTimer gameTimer;
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI opponentScoreText;

    void Start()
    {
        // Vérifie que tout est créé
        if (timerPanel == null || pauseButton == null)
        {
            Debug.LogError("? Utilise 'Create Top Panel' dans l'Inspector (clic droit sur script)");
            return;
        }

        Debug.Log("? TopPanelUI initialisé");
    }

    /// <summary>
    /// Met à jour le score du joueur
    /// </summary>
    public void UpdatePlayerScore(int score)
    {
        if (playerScoreText != null)
        {
            playerScoreText.text = score.ToString();
        }
    }

    /// <summary>
    /// Met à jour le score de l'adversaire
    /// </summary>
    public void UpdateOpponentScore(int score)
    {
        if (opponentScoreText != null)
        {
            opponentScoreText.text = score.ToString();
        }
    }

    /// <summary>
    /// Appelé quand le bouton pause est cliqué
    /// </summary>
    public void OnPauseClicked()
    {
        if (gameTimer != null)
        {
            if (gameTimer.isPaused)
            {
                gameTimer.ResumeTimer();
                Debug.Log("?? Jeu repris");
                // Ici: Reprendre le jeu
            }
            else
            {
                gameTimer.PauseTimer();
                Debug.Log("?? Jeu en pause");
                // Ici: Mettre le jeu en pause
            }
        }

        // Appelle le GameManager si tu en as un
        // GameManager.Instance.TogglePause();
    }

#if UNITY_EDITOR
    [ContextMenu("Create Top Panel")]
    public void CreateTopPanel()
    {
        Debug.Log("?? Création du panel du haut...");

        // Trouve le Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("? Ce GameObject doit être dans un Canvas!");
            return;
        }

        // Setup ce GameObject comme le panel principal
        RectTransform mainRT = GetComponent<RectTransform>();
        if (mainRT == null)
        {
            mainRT = gameObject.AddComponent<RectTransform>();
        }

        // Positionne en haut de l'écran
        mainRT.anchorMin = new Vector2(0, 1);
        mainRT.anchorMax = new Vector2(1, 1);
        mainRT.pivot = new Vector2(0.5f, 1);
        mainRT.anchoredPosition = new Vector2(0, 0);
        mainRT.sizeDelta = new Vector2(0, 80); // Hauteur 80px

        // Background noir transparent
        Image bgImage = gameObject.GetComponent<Image>();
        if (bgImage == null)
        {
            bgImage = gameObject.AddComponent<Image>();
        }
        bgImage.color = new Color(0.02f, 0.02f, 0.04f, 0.85f);

        // Bordure en bas
        GameObject borderBottom = CreateUIObject("BorderBottom", transform);
        RectTransform borderRT = borderBottom.GetComponent<RectTransform>();
        borderRT.anchorMin = new Vector2(0, 0);
        borderRT.anchorMax = new Vector2(1, 0);
        borderRT.pivot = new Vector2(0.5f, 0);
        borderRT.anchoredPosition = Vector2.zero;
        borderRT.sizeDelta = new Vector2(0, 2);

        Image borderImg = borderBottom.AddComponent<Image>();
        borderImg.color = new Color(0.9f, 0.1f, 0.1f, 0.8f); // Rouge sang

        // === TIMER PANEL (Gauche) ===
        CreateTimerPanel();

        // === CENTER INFO PANEL ===
        CreateCenterPanel();

        // === PAUSE BUTTON (Droite) ===
        CreatePauseButton();

        Debug.Log("? Top Panel créé ! Appuie sur PLAY pour tester");
    }

    void CreateTimerPanel()
    {
        // Supprime l'ancien
        if (timerPanel != null)
        {
            DestroyImmediate(timerPanel);
        }

        timerPanel = CreateUIObject("TimerPanel", transform);
        RectTransform panelRT = timerPanel.GetComponent<RectTransform>();

        // Position: Haut gauche
        panelRT.anchorMin = new Vector2(0, 0.5f);
        panelRT.anchorMax = new Vector2(0, 0.5f);
        panelRT.pivot = new Vector2(0, 0.5f);
        panelRT.anchoredPosition = new Vector2(20, 0);
        panelRT.sizeDelta = new Vector2(180, 60);

        // Background
        Image panelBg = timerPanel.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.1f, 0.9f);

        Outline panelOutline = timerPanel.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.9f, 0.1f, 0.1f, 0.6f);
        panelOutline.effectDistance = new Vector2(2, 2);

        // Icône Timer (??)
        GameObject iconObj = CreateUIObject("Icon", timerPanel.transform);
        RectTransform iconRT = iconObj.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0, 0.5f);
        iconRT.anchorMax = new Vector2(0, 0.5f);
        iconRT.pivot = new Vector2(0, 0.5f);
        iconRT.anchoredPosition = new Vector2(10, 0);
        iconRT.sizeDelta = new Vector2(30, 30);

        TextMeshProUGUI iconTxt = iconObj.AddComponent<TextMeshProUGUI>();
        iconTxt.text = "??";
        iconTxt.fontSize = 24;
        iconTxt.alignment = TextAlignmentOptions.Center;

        // Label "TEMPS"
        GameObject labelObj = CreateUIObject("Label", timerPanel.transform);
        RectTransform labelRT = labelObj.GetComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0, 1);
        labelRT.anchorMax = new Vector2(1, 1);
        labelRT.pivot = new Vector2(0.5f, 1);
        labelRT.anchoredPosition = new Vector2(0, -5);
        labelRT.sizeDelta = new Vector2(-10, 18);

        TextMeshProUGUI labelTxt = labelObj.AddComponent<TextMeshProUGUI>();
        labelTxt.text = "TEMPS";
        labelTxt.fontSize = 12;
        labelTxt.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        labelTxt.alignment = TextAlignmentOptions.Center;
        labelTxt.fontStyle = FontStyles.Bold;

        // Timer Text
        GameObject timerObj = CreateUIObject("TimerText", timerPanel.transform);
        RectTransform timerRT = timerObj.GetComponent<RectTransform>();
        timerRT.anchorMin = new Vector2(0, 0);
        timerRT.anchorMax = new Vector2(1, 0);
        timerRT.pivot = new Vector2(0.5f, 0);
        timerRT.anchoredPosition = new Vector2(0, 5);
        timerRT.sizeDelta = new Vector2(-10, 35);

        TextMeshProUGUI timerTxt = timerObj.AddComponent<TextMeshProUGUI>();
        timerTxt.text = "00:00";
        timerTxt.fontSize = 28;
        timerTxt.color = Color.white;
        timerTxt.alignment = TextAlignmentOptions.Center;
        timerTxt.fontStyle = FontStyles.Bold;

        AddTextOutline(timerObj, Color.black);

        // Ajoute le component GameTimer
        gameTimer = timerPanel.AddComponent<GameTimer>();
        gameTimer.timerText = timerTxt;
        gameTimer.labelText = labelTxt;
        gameTimer.isRunning = true;
        gameTimer.countDown = false; // Chronomètre normal
        gameTimer.maxTime = 0f; // Illimité
    }

    void CreateCenterPanel()
    {
        // Supprime l'ancien
        if (centerInfoPanel != null)
        {
            DestroyImmediate(centerInfoPanel);
        }

        centerInfoPanel = CreateUIObject("CenterInfoPanel", transform);
        RectTransform panelRT = centerInfoPanel.GetComponent<RectTransform>();

        // Position: Centre
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = new Vector2(300, 60);

        // Background
        Image panelBg = centerInfoPanel.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.1f, 0.9f);

        Outline panelOutline = centerInfoPanel.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.9f, 0.1f, 0.1f, 0.6f);
        panelOutline.effectDistance = new Vector2(2, 2);

        // Player Score (Gauche)
        GameObject playerScoreObj = CreateUIObject("PlayerScore", centerInfoPanel.transform);
        RectTransform playerRT = playerScoreObj.GetComponent<RectTransform>();
        playerRT.anchorMin = new Vector2(0, 0.5f);
        playerRT.anchorMax = new Vector2(0.4f, 0.5f);
        playerRT.pivot = new Vector2(0.5f, 0.5f);
        playerRT.anchoredPosition = Vector2.zero;
        playerRT.sizeDelta = Vector2.zero;

        playerScoreText = playerScoreObj.AddComponent<TextMeshProUGUI>();
        playerScoreText.text = "0";
        playerScoreText.fontSize = 32;
        playerScoreText.color = Color.white;
        playerScoreText.alignment = TextAlignmentOptions.Center;
        playerScoreText.fontStyle = FontStyles.Bold;

        AddTextOutline(playerScoreObj, Color.black);

        // VS (Centre)
        GameObject vsObj = CreateUIObject("VS", centerInfoPanel.transform);
        RectTransform vsRT = vsObj.GetComponent<RectTransform>();
        vsRT.anchorMin = new Vector2(0.4f, 0.5f);
        vsRT.anchorMax = new Vector2(0.6f, 0.5f);
        vsRT.pivot = new Vector2(0.5f, 0.5f);
        vsRT.anchoredPosition = Vector2.zero;
        vsRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI vsTxt = vsObj.AddComponent<TextMeshProUGUI>();
        vsTxt.text = "VS";
        vsTxt.fontSize = 18;
        vsTxt.color = new Color(0.9f, 0.1f, 0.1f, 1f);
        vsTxt.alignment = TextAlignmentOptions.Center;
        vsTxt.fontStyle = FontStyles.Bold;

        AddTextOutline(vsObj, Color.black);

        // Opponent Score (Droite)
        GameObject opponentScoreObj = CreateUIObject("OpponentScore", centerInfoPanel.transform);
        RectTransform opponentRT = opponentScoreObj.GetComponent<RectTransform>();
        opponentRT.anchorMin = new Vector2(0.6f, 0.5f);
        opponentRT.anchorMax = new Vector2(1f, 0.5f);
        opponentRT.pivot = new Vector2(0.5f, 0.5f);
        opponentRT.anchoredPosition = Vector2.zero;
        opponentRT.sizeDelta = Vector2.zero;

        opponentScoreText = opponentScoreObj.AddComponent<TextMeshProUGUI>();
        opponentScoreText.text = "0";
        opponentScoreText.fontSize = 32;
        opponentScoreText.color = Color.white;
        opponentScoreText.alignment = TextAlignmentOptions.Center;
        opponentScoreText.fontStyle = FontStyles.Bold;

        AddTextOutline(opponentScoreObj, Color.black);
    }

    void CreatePauseButton()
    {
        // Supprime l'ancien
        if (pauseButton != null)
        {
            DestroyImmediate(pauseButton);
        }

        pauseButton = CreateUIObject("PauseButton", transform);
        RectTransform btnRT = pauseButton.GetComponent<RectTransform>();

        // Position: Haut droite
        btnRT.anchorMin = new Vector2(1, 0.5f);
        btnRT.anchorMax = new Vector2(1, 0.5f);
        btnRT.pivot = new Vector2(1, 0.5f);
        btnRT.anchoredPosition = new Vector2(-20, 0);
        btnRT.sizeDelta = new Vector2(60, 60);

        // Background
        Image btnBg = pauseButton.AddComponent<Image>();
        btnBg.color = new Color(0.08f, 0.08f, 0.1f, 0.9f);

        // Button component
        Button btn = pauseButton.AddComponent<Button>();
        btn.onClick.AddListener(OnPauseClicked);

        // Transition colors
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 1f);
        colors.highlightedColor = new Color(0.9f, 0.1f, 0.1f, 1f);
        colors.pressedColor = new Color(0.7f, 0.05f, 0.05f, 1f);
        colors.selectedColor = new Color(0.9f, 0.1f, 0.1f, 1f);
        btn.colors = colors;

        // Outline
        Outline btnOutline = pauseButton.AddComponent<Outline>();
        btnOutline.effectColor = new Color(0.9f, 0.1f, 0.1f, 0.6f);
        btnOutline.effectDistance = new Vector2(2, 2);

        // Icône Pause (?? ou ??)
        GameObject iconObj = CreateUIObject("Icon", pauseButton.transform);
        RectTransform iconRT = iconObj.GetComponent<RectTransform>();
        SetFullRect(iconRT);

        TextMeshProUGUI iconTxt = iconObj.AddComponent<TextMeshProUGUI>();
        iconTxt.text = "??";
        iconTxt.fontSize = 32;
        iconTxt.color = Color.white;
        iconTxt.alignment = TextAlignmentOptions.Center;
        iconTxt.raycastTarget = false; // Ne bloque pas les clics du bouton

        AddTextOutline(iconObj, Color.black);
    }

    GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    void SetFullRect(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    void AddTextOutline(GameObject textObj, Color color)
    {
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(2, 2);
    }
#endif
}
