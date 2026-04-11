using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// One-shot editor script: creates the PlayerCountPanel UI hierarchy in the scene,
/// deactivates LevelUpPanel, and wires all component references.
/// Run via the MCP execute_script tool — delete this file afterwards.
/// </summary>
public class SetupPlayerCountUI
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("[Setup] UICanvas not found!"); return; }

        // ── 1. Deactivate LevelUpPanel ────────────────────────────────────────
        var levelUpPanelGO = canvas.transform.Find("LevelUpPanel")?.gameObject;
        if (levelUpPanelGO != null)
            levelUpPanelGO.SetActive(false);

        var levelUpUI = levelUpPanelGO != null
            ? levelUpPanelGO.GetComponent<LevelUpUIController>() : null;

        // ── 2. Create root panel (full-screen dark overlay) ───────────────────
        var panelRoot = CreatePanel("PlayerCountPanel", canvas.transform,
            new Color(0.06f, 0.06f, 0.14f, 0.97f));
        StretchFull(panelRoot.GetComponent<RectTransform>());

        // ── 3. CountPanel — Step 1: 1 Player / 2 Players ─────────────────────
        var countPanel = CreatePanel("CountPanel", panelRoot.transform, Color.clear);
        Center(countPanel.GetComponent<RectTransform>(), new Vector2(520, 320));

        var countTitle = CreateText("Title", countPanel.transform,
            "HOW MANY PLAYERS?", 38);
        Place(countTitle, new Vector2(0, 110), new Vector2(500, 60));

        var onePBtn = CreateButton("OnePlayerBtn", countPanel.transform, "1 PLAYER",
            new Color(0.20f, 0.45f, 0.80f));
        Place(onePBtn, new Vector2(-135, 0), new Vector2(220, 75));

        var twoPBtn = CreateButton("TwoPlayersBtn", countPanel.transform, "2 PLAYERS",
            new Color(0.20f, 0.45f, 0.80f));
        Place(twoPBtn, new Vector2(135, 0), new Vector2(220, 75));

        var countHint = CreateText("Hint", countPanel.transform,
            "2 Players: P1 = Keyboard+Mouse   |   P2 = Controller", 18);
        Place(countHint, new Vector2(0, -95), new Vector2(500, 34));
        countHint.GetComponent<TextMeshProUGUI>().color = new Color(0.7f, 0.7f, 0.7f);

        // ── 4. SchemePanel — Step 2: Keyboard / Controller (1P only) ─────────
        var schemePanel = CreatePanel("SchemePanel", panelRoot.transform, Color.clear);
        Center(schemePanel.GetComponent<RectTransform>(), new Vector2(520, 380));
        schemePanel.SetActive(false);

        var schemeTitle = CreateText("Title", schemePanel.transform,
            "CHOOSE YOUR CONTROL SCHEME", 32);
        Place(schemeTitle, new Vector2(0, 140), new Vector2(500, 55));

        var kbBtn = CreateButton("KeyboardBtn", schemePanel.transform,
            "Keyboard + Mouse", new Color(0.18f, 0.50f, 0.25f));
        Place(kbBtn, new Vector2(0, 50), new Vector2(310, 75));

        var ctrlBtn = CreateButton("ControllerBtn", schemePanel.transform,
            "Controller", new Color(0.18f, 0.50f, 0.25f));
        Place(ctrlBtn, new Vector2(0, -45), new Vector2(310, 75));

        // ── 5. WaitPanel — shown while polling for a gamepad ──────────────────
        var waitPanel = new GameObject("WaitPanel");
        waitPanel.transform.SetParent(schemePanel.transform, false);
        var waitRT = waitPanel.AddComponent<RectTransform>();
        waitRT.anchorMin = Vector2.zero; waitRT.anchorMax = Vector2.one;
        waitRT.offsetMin = Vector2.zero; waitRT.offsetMax = Vector2.zero;
        waitPanel.SetActive(false);

        var waitTextGO = CreateText("WaitText", waitPanel.transform,
            "Waiting for controller...\nPlease connect a gamepad.", 24);
        Place(waitTextGO, new Vector2(0, -150), new Vector2(480, 72));
        waitTextGO.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.85f, 0.3f);

        // ── 6. Wire PlayerCountSelectionUI component ──────────────────────────
        var ui = panelRoot.AddComponent<PlayerCountSelectionUI>();
        ui.panel            = panelRoot;
        ui.countPanel       = countPanel;
        ui.schemePanel      = schemePanel;
        ui.waitPanel        = waitPanel;
        ui.waitText         = waitTextGO.GetComponent<TextMeshProUGUI>();
        ui.onePlayerButton  = onePBtn.GetComponent<Button>();
        ui.twoPlayersButton = twoPBtn.GetComponent<Button>();
        ui.keyboardButton   = kbBtn.GetComponent<Button>();
        ui.controllerButton = ctrlBtn.GetComponent<Button>();
        ui.levelUpUI        = levelUpUI;

        // ── 7. Add GameSetupManager to the GameManager object ─────────────────
        var gmGO = GameObject.Find("GameManager");
        if (gmGO != null && gmGO.GetComponent<GameSetupManager>() == null)
        {
            var gsm = gmGO.AddComponent<GameSetupManager>();
            var p2GO = GameObject.Find("Player2");
            if (p2GO != null) gsm.player2 = p2GO;
            else Debug.LogWarning("[Setup] Player2 GameObject not found — assign it manually in GameSetupManager.");
        }

        // ── 8. Save scene ─────────────────────────────────────────────────────
        EditorUtility.SetDirty(canvas);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log("[Setup] PlayerCountPanel created and wired. LevelUpPanel deactivated.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static GameObject CreatePanel(string name, Transform parent, Color bgColor)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var img   = go.AddComponent<Image>();
        img.color = bgColor;
        return go;
    }

    static GameObject CreateText(string name, Transform parent, string content, int size)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = content;
        tmp.fontSize  = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        return go;
    }

    static GameObject CreateButton(string name, Transform parent, string label, Color bg)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        var img  = go.AddComponent<Image>();
        img.color = bg;
        go.AddComponent<Button>();

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var labelRT     = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;
        var tmp  = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        return go;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static void Center(RectTransform rt, Vector2 size)
    {
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = size;
        rt.anchoredPosition = Vector2.zero;
    }

    static void Place(GameObject go, Vector2 pos, Vector2 size)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
    }
}
