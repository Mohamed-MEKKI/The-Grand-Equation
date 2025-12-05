using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// [DEPRECATED] Use CardHover.cs instead for better integration with CardDisplay system.
/// This class is kept for backward compatibility but should not be used in new code.
/// </summary>
[System.Obsolete("Use CardHover.cs instead. This class will be removed in a future version.")]
public class CardHoverSystem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("⚙️ Données de la Carte")]
    public string cardName = "Lame Sanglante";
    public string power1Name = "Frappe Mortelle";
    public string power1Description = "Inflige 4 dégâts à l'adversaire et vole 1 PV";
    public string power2Name = "Soif de Sang";
    public string power2Description = "Régénère 2 PV à chaque attaque réussie";

    [Header("📐 Position du Panneau")]
    public float offsetX = 250f; // Distance à droite de la carte
    public float offsetY = 0f;   // Hauteur

    [Header("🎨 Ne touche pas (Auto)")]
    public GameObject hoverPanel;

    private CanvasGroup canvasGroup;
    private bool isHovering = false;

    // Références textes
    private TextMeshProUGUI txtCardName;
    private TextMeshProUGUI txtPower1Name;
    private TextMeshProUGUI txtPower1Desc;
    private TextMeshProUGUI txtPower2Name;
    private TextMeshProUGUI txtPower2Desc;

    void Start()
    {
        if (hoverPanel == null)
        {
            Debug.LogError("❌ Clique sur 'Create Hover Panel' dans l'Inspector (clic droit sur script)");
            return;
        }

        // Cache les références
        FindReferences();

        // Cache au départ
        hoverPanel.SetActive(false);
    }

    void FindReferences()
    {
        canvasGroup = hoverPanel.GetComponent<CanvasGroup>();

        // Trouve les textes
        Transform[] allChildren = hoverPanel.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.name == "TxtCardName") txtCardName = child.GetComponent<TextMeshProUGUI>();
            if (child.name == "TxtPower1Name") txtPower1Name = child.GetComponent<TextMeshProUGUI>();
            if (child.name == "TxtPower1Desc") txtPower1Desc = child.GetComponent<TextMeshProUGUI>();
            if (child.name == "TxtPower2Name") txtPower2Name = child.GetComponent<TextMeshProUGUI>();
            if (child.name == "TxtPower2Desc") txtPower2Desc = child.GetComponent<TextMeshProUGUI>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverPanel == null) return;
        isHovering = true;
        ShowPanel();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverPanel == null) return;
        isHovering = false;
        HidePanel();
    }

    void ShowPanel()
    {
        // Update textes
        if (txtCardName) txtCardName.text = cardName.ToUpper();
        if (txtPower1Name) txtPower1Name.text = "⚔ " + power1Name.ToUpper();
        if (txtPower1Desc) txtPower1Desc.text = power1Description;
        if (txtPower2Name) txtPower2Name.text = "⚔ " + power2Name.ToUpper();
        if (txtPower2Desc) txtPower2Desc.text = power2Description;

        hoverPanel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    void HidePanel()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = t / 0.2f;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = 1f - (t / 0.15f);
            yield return null;
        }
        hoverPanel.SetActive(false);
    }

    public void UpdatePowers(string name, string p1Name, string p1Desc, string p2Name, string p2Desc)
    {
        cardName = name;
        power1Name = p1Name;
        power1Description = p1Desc;
        power2Name = p2Name;
        power2Description = p2Desc;
    }

#if UNITY_EDITOR
    [ContextMenu("Create Hover Panel")]
    public void CreateHoverPanel()
    {
        // Supprime l'ancien
        if (hoverPanel != null)
        {
            DestroyImmediate(hoverPanel);
        }

        // === PANNEAU PRINCIPAL ===
        hoverPanel = new GameObject("HoverPanel");
        hoverPanel.transform.SetParent(transform);
        hoverPanel.transform.localPosition = Vector3.zero;
        hoverPanel.transform.localScale = Vector3.one;

        RectTransform panelRT = hoverPanel.AddComponent<RectTransform>();
        // Ancré au centre de la carte
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0f, 0.5f); // Pivot à gauche pour s'étendre vers la droite
        panelRT.anchoredPosition = new Vector2(offsetX, offsetY);
        panelRT.sizeDelta = new Vector2(400f, 450f); // Largeur x Hauteur

        CanvasGroup cg = hoverPanel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false; // Ne bloque pas les clics

        // === BACKGROUND ===
        GameObject bg = CreateUIObject("Background", hoverPanel.transform);
        RectTransform bgRT = bg.GetComponent<RectTransform>();
        SetFullRect(bgRT);

        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.02f, 0.02f, 0.04f, 0.98f); // Noir très foncé

        // Bordure rouge
        Outline outline = bg.AddComponent<Outline>();
        outline.effectColor = new Color(0.9f, 0.1f, 0.1f, 1f); // Rouge sang
        outline.effectDistance = new Vector2(4, 4);

        // Shadow
        Shadow shadow = bg.AddComponent<Shadow>();
        shadow.effectColor = new Color(0.9f, 0.1f, 0.1f, 0.5f);
        shadow.effectDistance = new Vector2(0, 8);

        // === TITRE (NOM DE LA CARTE) ===
        GameObject titleObj = CreateUIObject("TxtCardName", hoverPanel.transform);
        RectTransform titleRT = titleObj.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 1);
        titleRT.anchorMax = new Vector2(1, 1);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0, -20);
        titleRT.sizeDelta = new Vector2(-40, 60);

        TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "CARTE MYSTÉRIEUSE";
        titleTxt.fontSize = 26;
        titleTxt.color = Color.white;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.enableWordWrapping = false;

        AddTextOutline(titleObj, Color.black);

        // === SÉPARATEUR ===
        GameObject sep = CreateUIObject("Separator", hoverPanel.transform);
        RectTransform sepRT = sep.GetComponent<RectTransform>();
        sepRT.anchorMin = new Vector2(0.1f, 1);
        sepRT.anchorMax = new Vector2(0.9f, 1);
        sepRT.pivot = new Vector2(0.5f, 1f);
        sepRT.anchoredPosition = new Vector2(0, -90);
        sepRT.sizeDelta = new Vector2(0, 2);

        Image sepImg = sep.AddComponent<Image>();
        sepImg.color = new Color(0.9f, 0.1f, 0.1f, 0.6f);

        // === POWER 1 ===
        CreatePowerBlock("Power1", hoverPanel.transform, -110, "TxtPower1Name", "TxtPower1Desc");

        // === POWER 2 ===
        CreatePowerBlock("Power2", hoverPanel.transform, -280, "TxtPower2Name", "TxtPower2Desc");

        Debug.Log("✅ Panneau créé ! Appuie sur PLAY et survole la carte !");
    }

    void CreatePowerBlock(string name, Transform parent, float yPos, string nameTextName, string descTextName)
    {
        // Container
        GameObject container = CreateUIObject(name + "Container", parent);
        RectTransform containerRT = container.GetComponent<RectTransform>();
        containerRT.anchorMin = new Vector2(0, 1);
        containerRT.anchorMax = new Vector2(1, 1);
        containerRT.pivot = new Vector2(0.5f, 1f);
        containerRT.anchoredPosition = new Vector2(0, yPos);
        containerRT.sizeDelta = new Vector2(-40, 150);

        // Background du pouvoir
        Image containerBg = container.AddComponent<Image>();
        containerBg.color = new Color(0.08f, 0.08f, 0.1f, 0.7f);

        Outline containerOutline = container.AddComponent<Outline>();
        containerOutline.effectColor = new Color(0.9f, 0.1f, 0.1f, 0.4f);
        containerOutline.effectDistance = new Vector2(2, 2);

        // Nom du pouvoir
        GameObject nameObj = CreateUIObject(nameTextName, container.transform);
        RectTransform nameRT = nameObj.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0, 1);
        nameRT.anchorMax = new Vector2(1, 1);
        nameRT.pivot = new Vector2(0.5f, 1f);
        nameRT.anchoredPosition = new Vector2(0, -15);
        nameRT.sizeDelta = new Vector2(-20, 40);

        TextMeshProUGUI nameTxt = nameObj.AddComponent<TextMeshProUGUI>();
        nameTxt.text = "⚔ POUVOIR";
        nameTxt.fontSize = 20;
        nameTxt.color = new Color(0.95f, 0.15f, 0.15f, 1f);
        nameTxt.fontStyle = FontStyles.Bold;
        nameTxt.alignment = TextAlignmentOptions.Left;

        AddTextOutline(nameObj, Color.black);

        // Description
        GameObject descObj = CreateUIObject(descTextName, container.transform);
        RectTransform descRT = descObj.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0, 1);
        descRT.anchorMax = new Vector2(1, 1);
        descRT.pivot = new Vector2(0.5f, 1f);
        descRT.anchoredPosition = new Vector2(0, -60);
        descRT.sizeDelta = new Vector2(-20, 80);

        TextMeshProUGUI descTxt = descObj.AddComponent<TextMeshProUGUI>();
        descTxt.text = "Description du pouvoir ici...";
        descTxt.fontSize = 16;
        descTxt.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        descTxt.fontStyle = FontStyles.Normal;
        descTxt.alignment = TextAlignmentOptions.TopLeft;
        descTxt.enableWordWrapping = true;

        AddTextOutline(descObj, new Color(0, 0, 0, 0.8f));
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
        outline.effectDistance = new Vector2(1.5f, 1.5f);
    }
#endif
}
