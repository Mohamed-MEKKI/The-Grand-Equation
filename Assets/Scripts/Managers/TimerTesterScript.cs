using UnityEngine;

/// <summary>
/// Script de test pour le TopPanel
/// Attache ce script à n'importe quel GameObject pour tester le panel
/// Utilise les touches du clavier pour tester toutes les fonctionnalités
/// </summary>
public class TopPanelTester : MonoBehaviour
{
    [Header("🎮 References")]
    [Tooltip("Glisse TopPanel ici depuis la Hierarchy")]
    public TopPanelUI topPanel;

    [Header("📊 État du Test")]
    public int playerScore = 0;
    public int opponentScore = 0;
    public bool showHelp = true;

    void Start()
    {
        if (topPanel == null)
        {
            Debug.LogError("❌ Assigne TopPanel dans l'Inspector!");

            // Essaie de le trouver automatiquement
            topPanel = FindObjectOfType<TopPanelUI>();
            if (topPanel != null)
            {
                Debug.Log("✅ TopPanel trouvé automatiquement!");
            }
        }

        if (topPanel != null)
        {
            Debug.Log("🧪 TopPanelTester actif - Appuie sur H pour voir les commandes");

            // Init les scores
            topPanel.UpdatePlayerScore(playerScore);
            topPanel.UpdateOpponentScore(opponentScore);
        }
    }

    void Update()
    {
        if (topPanel == null)
            return;

        // H = Help (affiche les commandes)
        if (Input.GetKeyDown(KeyCode.H))
        {
            ShowHelp();
        }

        // === CONTRÔLE DU TIMER ===

        // P = Pause/Resume
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (topPanel.gameTimer.isPaused)
            {
                topPanel.gameTimer.ResumeTimer();
                Debug.Log("▶️ Timer repris");
            }
            else
            {
                topPanel.gameTimer.PauseTimer();
                Debug.Log("⏸️ Timer pausé");
            }
        }

        // R = Reset timer
        if (Input.GetKeyDown(KeyCode.R))
        {
            topPanel.gameTimer.ResetTimer();
            Debug.Log("🔄 Timer réinitialisé");
        }

        // S = Start/Stop timer
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (topPanel.gameTimer.isRunning)
            {
                topPanel.gameTimer.StopTimer();
                Debug.Log("⏹️ Timer arrêté");
            }
            else
            {
                topPanel.gameTimer.StartTimer();
                Debug.Log("▶️ Timer démarré");
            }
        }

        // + = Ajouter 10 secondes
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Equals))
        {
            topPanel.gameTimer.AddTime(10f);
            Debug.Log("➕ +10 secondes");
        }

        // - = Retirer 10 secondes
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.Underscore))
        {
            topPanel.gameTimer.RemoveTime(10f);
            Debug.Log("➖ -10 secondes");
        }

        // === CONTRÔLE DES SCORES ===

        // 1 = Player score +1
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            playerScore++;
            topPanel.UpdatePlayerScore(playerScore);
            Debug.Log($"⭐ Score Joueur: {playerScore}");
        }

        // 2 = Opponent score +1
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            opponentScore++;
            topPanel.UpdateOpponentScore(opponentScore);
            Debug.Log($"💀 Score Adversaire: {opponentScore}");
        }

        // 3 = Player score -1
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            playerScore = Mathf.Max(0, playerScore - 1);
            topPanel.UpdatePlayerScore(playerScore);
            Debug.Log($"⭐ Score Joueur: {playerScore}");
        }

        // 4 = Opponent score -1
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            opponentScore = Mathf.Max(0, opponentScore - 1);
            topPanel.UpdateOpponentScore(opponentScore);
            Debug.Log($"💀 Score Adversaire: {opponentScore}");
        }

        // 0 = Reset scores
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            playerScore = 0;
            opponentScore = 0;
            topPanel.UpdatePlayerScore(playerScore);
            topPanel.UpdateOpponentScore(opponentScore);
            Debug.Log("🔄 Scores réinitialisés");
        }

        // === TESTS SPÉCIAUX ===

        // T = Toggle mode timer (chronomètre <-> décompte)
        if (Input.GetKeyDown(KeyCode.T))
        {
            topPanel.gameTimer.countDown = !topPanel.gameTimer.countDown;
            topPanel.gameTimer.ResetTimer();
            string mode = topPanel.gameTimer.countDown ? "Décompte" : "Chronomètre";
            Debug.Log($"🔄 Mode: {mode}");
        }

        // M = Change max time (180s / 300s / 600s / illimité)
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (topPanel.gameTimer.maxTime == 0f)
            {
                topPanel.gameTimer.maxTime = 180f; // 3 min
                Debug.Log("⏱️ Max Time: 3 minutes");
            }
            else if (topPanel.gameTimer.maxTime == 180f)
            {
                topPanel.gameTimer.maxTime = 300f; // 5 min
                Debug.Log("⏱️ Max Time: 5 minutes");
            }
            else if (topPanel.gameTimer.maxTime == 300f)
            {
                topPanel.gameTimer.maxTime = 600f; // 10 min
                Debug.Log("⏱️ Max Time: 10 minutes");
            }
            else
            {
                topPanel.gameTimer.maxTime = 0f; // Illimité
                Debug.Log("⏱️ Max Time: Illimité");
            }

            topPanel.gameTimer.ResetTimer();
        }

        // I = Info (affiche l'état actuel)
        if (Input.GetKeyDown(KeyCode.I))
        {
            ShowInfo();
        }

        // D = Demo (lance une démo automatique)
        if (Input.GetKeyDown(KeyCode.D))
        {
            StartDemo();
        }
    }

    void ShowHelp()
    {
        Debug.Log(@"
╔═══════════════════════════════════════════════════╗
║         🎮 COMMANDES DE TEST - TOP PANEL         ║
╠═══════════════════════════════════════════════════╣
║  TIMER:                                           ║
║    P = Pause/Resume                              ║
║    R = Reset                                     ║
║    S = Start/Stop                                ║
║    + = Ajouter 10 secondes                       ║
║    - = Retirer 10 secondes                       ║
║    T = Toggle mode (Chronomètre/Décompte)        ║
║    M = Change temps max (3/5/10 min/illimité)    ║
║                                                   ║
║  SCORES:                                          ║
║    1 = Score Joueur +1                           ║
║    2 = Score Adversaire +1                       ║
║    3 = Score Joueur -1                           ║
║    4 = Score Adversaire -1                       ║
║    0 = Reset tous les scores                     ║
║                                                   ║
║  INFOS:                                           ║
║    I = Afficher l'état actuel                    ║
║    D = Lancer une démo automatique               ║
║    H = Afficher cette aide                       ║
╚═══════════════════════════════════════════════════╝
        ");
    }

    void ShowInfo()
    {
        if (topPanel == null || topPanel.gameTimer == null)
        {
            Debug.LogError("❌ TopPanel ou GameTimer non trouvé!");
            return;
        }

        string mode = topPanel.gameTimer.countDown ? "Décompte" : "Chronomètre";
        string maxTimeStr = topPanel.gameTimer.maxTime > 0
            ? $"{topPanel.gameTimer.maxTime}s"
            : "Illimité";
        string status = topPanel.gameTimer.isPaused
            ? "PAUSÉ"
            : (topPanel.gameTimer.isRunning ? "EN COURS" : "ARRÊTÉ");

        Debug.Log($@"
╔═══════════════════════════════════════════════════╗
║              📊 ÉTAT ACTUEL DU PANEL             ║
╠═══════════════════════════════════════════════════╣
║  TIMER:                                           ║
║    Temps actuel: {topPanel.gameTimer.GetFormattedTime(),-25}    ║
║    Mode: {mode,-39}    ║
║    Temps max: {maxTimeStr,-36}    ║
║    Status: {status,-39}    ║
║                                                   ║
║  SCORES:                                          ║
║    Joueur: {playerScore,-39}    ║
║    Adversaire: {opponentScore,-35}    ║
╚═══════════════════════════════════════════════════╝
        ");
    }

    void StartDemo()
    {
        Debug.Log("🎬 Démo lancée!");
        StartCoroutine(DemoCoroutine());
    }

    System.Collections.IEnumerator DemoCoroutine()
    {
        Debug.Log("1️⃣ Reset tout...");
        topPanel.gameTimer.ResetTimer();
        topPanel.gameTimer.StartTimer();
        playerScore = 0;
        opponentScore = 0;
        topPanel.UpdatePlayerScore(0);
        topPanel.UpdateOpponentScore(0);

        yield return new WaitForSeconds(2f);

        Debug.Log("2️⃣ Le joueur marque 3 points...");
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.5f);
            playerScore++;
            topPanel.UpdatePlayerScore(playerScore);
            Debug.Log($"⭐ Score Joueur: {playerScore}");
        }

        yield return new WaitForSeconds(1f);

        Debug.Log("3️⃣ L'adversaire marque 2 points...");
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSeconds(0.5f);
            opponentScore++;
            topPanel.UpdateOpponentScore(opponentScore);
            Debug.Log($"💀 Score Adversaire: {opponentScore}");
        }

        yield return new WaitForSeconds(1f);

        Debug.Log("4️⃣ Bonus de temps +15s...");
        topPanel.gameTimer.AddTime(15f);

        yield return new WaitForSeconds(2f);

        Debug.Log("5️⃣ Pause du jeu...");
        topPanel.gameTimer.PauseTimer();

        yield return new WaitForSeconds(2f);

        Debug.Log("6️⃣ Reprise du jeu...");
        topPanel.gameTimer.ResumeTimer();

        yield return new WaitForSeconds(2f);

        Debug.Log("✅ Démo terminée! Le timer continue...");
    }

    // Affiche les contrôles à l'écran (GUI)
    void OnGUI()
    {
        if (!showHelp || topPanel == null)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 12;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;
        style.padding = new RectOffset(10, 10, 10, 10);

        string helpText =
            "<b>COMMANDES TEST</b>\n\n" +
            "<b>Timer:</b> P=Pause R=Reset S=Start/Stop +=Add10s -=Remove10s\n" +
            "<b>Scores:</b> 1=Player+1 2=Opponent+1 3=Player-1 4=Opponent-1 0=Reset\n" +
            "<b>Infos:</b> I=Info D=Demo H=Help T=ToggleMode M=MaxTime\n\n" +
            $"<color=#ffff00>Score: {playerScore} VS {opponentScore}</color>";

        GUI.Box(new Rect(10, 10, 500, 110), helpText, style);
    }
}
