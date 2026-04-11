#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Builds the ClassSelectionUI card layout and wires all references automatically.
/// Run once via MCP, then delete.
/// </summary>
public static class BuildClassSelectionUI
{
    public static void Execute()
    {
        // Find the ClassSelectionUI GameObject — must search inactive objects too
        ClassSelectionUI csScript = null;
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            csScript = root.GetComponentInChildren<ClassSelectionUI>(includeInactive: true);
            if (csScript != null) break;
        }

        GameObject csGo;
        if (csScript != null)
        {
            csGo = csScript.gameObject;
        }
        else
        {
            // ClassSelectionUI doesn't exist yet — find the LevelUpPanel and create it
            GameObject luPanel = null;
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var found = root.transform.Find("LevelUpPanel");
                if (found != null) { luPanel = found.gameObject; break; }
                // Deeper search
                var deepFound = FindInChildren(root.transform, "LevelUpPanel");
                if (deepFound != null) { luPanel = deepFound.gameObject; break; }
            }

            if (luPanel == null)
            {
                Debug.LogError("[BuildClassSelectionUI] LevelUpPanel not found in scene.");
                return;
            }

            csGo = new GameObject("ClassSelectionUI");
            csGo.transform.SetParent(luPanel.transform, false);
            var rt = csGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var bgImg = csGo.AddComponent<UnityEngine.UI.Image>();
            bgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        }

        csScript = csGo.GetComponent<ClassSelectionUI>();
        if (csScript == null) csScript = csGo.AddComponent<ClassSelectionUI>();

        // Load class definitions
        var tankDef    = AssetDatabase.LoadAssetAtPath<ClassDefinitionSO>("Assets/Data/Classes/Tank.asset");
        var fighterDef = AssetDatabase.LoadAssetAtPath<ClassDefinitionSO>("Assets/Data/Classes/Fighter.asset");
        var rangerDef  = AssetDatabase.LoadAssetAtPath<ClassDefinitionSO>("Assets/Data/Classes/Ranger.asset");

        // Title label at the top
        var title = MakeText("Title", csGo.transform, "CHOOSE YOUR CLASS", 40);
        var titleRt = title.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.2f, 0.85f);
        titleRt.anchorMax = new Vector2(0.8f, 1.0f);
        titleRt.offsetMin = titleRt.offsetMax = Vector2.zero;

        // --- P1 card row (left half) ---
        var p1Row = MakePanel("P1_Cards", csGo.transform, new Vector2(0.02f, 0.1f), new Vector2(0.48f, 0.82f));
        var p1Layout = p1Row.AddComponent<HorizontalLayoutGroup>();
        p1Layout.spacing = 12f;
        p1Layout.childForceExpandWidth = true;
        p1Layout.childForceExpandHeight = true;
        p1Layout.padding = new RectOffset(8, 8, 8, 8);

        MakeText("P1_Label", csGo.transform, "PLAYER 1", 26,
            new Vector2(0.02f, 0.83f), new Vector2(0.48f, 0.90f));

        // --- P2 card row (right half) ---
        var p2Row = MakePanel("P2_Cards", csGo.transform, new Vector2(0.52f, 0.1f), new Vector2(0.98f, 0.82f));
        var p2Layout = p2Row.AddComponent<HorizontalLayoutGroup>();
        p2Layout.spacing = 12f;
        p2Layout.childForceExpandWidth = true;
        p2Layout.childForceExpandHeight = true;
        p2Layout.padding = new RectOffset(8, 8, 8, 8);

        MakeText("P2_Label", csGo.transform, "PLAYER 2", 26,
            new Vector2(0.52f, 0.83f), new Vector2(0.98f, 0.90f));

        // Build 3 cards per player
        var defs = new[] { tankDef, fighterDef, rangerDef };
        var colors = new[] { new Color(0.3f, 0.5f, 0.9f), new Color(0.2f, 0.8f, 0.3f), new Color(0.9f, 0.6f, 0.1f) };

        var p1Cards = new ClassCardUI[3];
        var p2Cards = new ClassCardUI[3];

        for (int i = 0; i < 3; i++)
        {
            p1Cards[i] = MakeClassCard(defs[i], colors[i], p1Row.transform);
            p2Cards[i] = MakeClassCard(defs[i], colors[i], p2Row.transform);
        }

        // Status labels at bottom
        MakeText("P1_Status", csGo.transform, "", 22,
            new Vector2(0.02f, 0.02f), new Vector2(0.48f, 0.10f));
        MakeText("P2_Status", csGo.transform, "", 22,
            new Vector2(0.52f, 0.02f), new Vector2(0.98f, 0.10f));

        // Wire ClassSelectionUI fields
        var so = new SerializedObject(csScript);

        so.FindProperty("tankDef").objectReferenceValue    = tankDef;
        so.FindProperty("fighterDef").objectReferenceValue = fighterDef;
        so.FindProperty("rangerDef").objectReferenceValue  = rangerDef;

        var p1Arr = so.FindProperty("p1Cards");
        var p2Arr = so.FindProperty("p2Cards");
        p1Arr.arraySize = 3;
        p2Arr.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            p1Arr.GetArrayElementAtIndex(i).objectReferenceValue = p1Cards[i];
            p2Arr.GetArrayElementAtIndex(i).objectReferenceValue = p2Cards[i];
        }

        var p1Status = csGo.transform.Find("P1_Status")?.GetComponent<TextMeshProUGUI>();
        var p2Status = csGo.transform.Find("P2_Status")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("p1Status").objectReferenceValue = p1Status;
        so.FindProperty("p2Status").objectReferenceValue = p2Status;

        so.ApplyModifiedProperties();

        // Activate the ClassSelectionUI so it's visible at game start
        csGo.SetActive(true);

        // Also activate the LevelUpPanel (parent) — it houses class selection at start
        var luPanelActivate = csGo.transform.parent?.gameObject;
        if (luPanelActivate != null) luPanelActivate.SetActive(true);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[BuildClassSelectionUI] Done — ClassSelectionUI fully wired.");
    }

    static ClassCardUI MakeClassCard(ClassDefinitionSO def, Color accentColor, Transform parent)
    {
        string label = def != null ? def.className : "???";

        // Card background
        var card = new GameObject($"Card_{label}");
        card.transform.SetParent(parent, false);
        var bg = card.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.12f, 0.18f, 1f);

        var layout = card.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 8f;
        layout.childForceExpandWidth  = true;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperCenter;

        // Accent bar at top
        var accent = new GameObject("AccentBar");
        accent.transform.SetParent(card.transform, false);
        var accentImg = accent.AddComponent<Image>();
        accentImg.color = accentColor;
        var accentRt = accent.GetComponent<RectTransform>();
        accentRt.sizeDelta = new Vector2(0, 8);
        var accentLayout = accent.AddComponent<LayoutElement>();
        accentLayout.preferredHeight = 8;
        accentLayout.flexibleWidth   = 1;

        // Class name
        var nameGo = MakeText("ClassName", card.transform, label, 28);
        nameGo.GetComponent<TextMeshProUGUI>().color = accentColor;
        var nameLayout = nameGo.AddComponent<LayoutElement>();
        nameLayout.preferredHeight = 36;
        nameLayout.flexibleWidth   = 1;

        // Description
        string desc = def != null ? def.description : "";
        var descGo = MakeText("Description", card.transform, desc, 14);
        descGo.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.8f, 0.8f);
        var descLayout = descGo.AddComponent<LayoutElement>();
        descLayout.preferredHeight = 60;
        descLayout.flexibleWidth   = 1;

        // Separator
        var sep = new GameObject("Sep");
        sep.transform.SetParent(card.transform, false);
        var sepImg = sep.AddComponent<Image>();
        sepImg.color = new Color(0.3f, 0.3f, 0.3f);
        var sepLayout = sep.AddComponent<LayoutElement>();
        sepLayout.preferredHeight = 2;
        sepLayout.flexibleWidth   = 1;

        // Select button
        var btnGo = new GameObject("SelectButton");
        btnGo.transform.SetParent(card.transform, false);
        var btnImg = btnGo.AddComponent<Image>();
        btnImg.color = accentColor * 0.8f;
        var btn = btnGo.AddComponent<Button>();
        var btnCb = btn.colors;
        btnCb.normalColor      = accentColor * 0.7f;
        btnCb.highlightedColor = accentColor;
        btnCb.pressedColor     = accentColor * 0.5f;
        btn.colors = btnCb;
        btn.targetGraphic = btnImg;
        var btnLayout = btnGo.AddComponent<LayoutElement>();
        btnLayout.preferredHeight = 40;
        btnLayout.flexibleWidth   = 1;
        MakeText("BtnLabel", btnGo.transform, "SELECT", 18);

        // Locked overlay (disabled by default)
        var lockGo = new GameObject("LockedOverlay");
        lockGo.transform.SetParent(card.transform, false);
        var lockImg = lockGo.AddComponent<Image>();
        lockImg.color = new Color(0f, 0f, 0f, 0.7f);
        // Stretch to fill the card
        var lockRt = lockGo.GetComponent<RectTransform>();
        lockRt.anchorMin = Vector2.zero;
        lockRt.anchorMax = Vector2.one;
        lockRt.offsetMin = lockRt.offsetMax = Vector2.zero;
        MakeText("LockLabel", lockGo.transform, "TAKEN", 24);
        lockGo.SetActive(false);

        // Add ClassCardUI and wire it
        var cardScript = card.AddComponent<ClassCardUI>();
        var so = new SerializedObject(cardScript);
        so.FindProperty("classNameText").objectReferenceValue   = nameGo.GetComponent<TextMeshProUGUI>();
        so.FindProperty("descriptionText").objectReferenceValue = descGo.GetComponent<TextMeshProUGUI>();
        so.FindProperty("selectButton").objectReferenceValue    = btn;
        so.FindProperty("lockedOverlay").objectReferenceValue   = lockGo;
        so.ApplyModifiedProperties();

        return cardScript;
    }

    // ── UI helpers ────────────────────────────────────────────────────────────

    static Transform FindInChildren(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var found = FindInChildren(child, name);
            if (found != null) return found;
        }
        return null;
    }

    static GameObject MakePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.08f, 0.08f, 0.12f, 0.8f);
        return go;
    }

    static GameObject MakeText(string name, Transform parent, string text, int fontSize,
        Vector2 anchorMin = default, Vector2 anchorMax = default)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        if (anchorMin != default || anchorMax != default)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
        else
        {
            rt.sizeDelta = new Vector2(300, 40);
        }
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        return go;
    }
}
#endif
