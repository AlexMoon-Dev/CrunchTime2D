#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Adds PlayerHUDElement components to P1_HUD and P2_HUD, wires all
/// sub-references, then wires HUDController to both of them.
/// Run once via MCP, then delete.
/// </summary>
public static class WireHUD
{
    public static void Execute()
    {
        var hud = FindInactive("HUD");
        if (hud == null) { Debug.LogError("[WireHUD] HUD GameObject not found."); return; }

        var p1Hud = FindInactive("P1_HUD");
        var p2Hud = FindInactive("P2_HUD");
        if (p1Hud == null || p2Hud == null) { Debug.LogError("[WireHUD] P1_HUD or P2_HUD not found."); return; }

        var p1Element = WirePlayerHUD(p1Hud);
        var p2Element = WirePlayerHUD(p2Hud);

        // HUDController
        var hudCtrl = hud.GetComponent<HUDController>();
        if (hudCtrl == null) hudCtrl = hud.AddComponent<HUDController>();

        var wavePanel  = FindInactive("WavePanel");
        var waveNumTxt = wavePanel != null ? wavePanel.transform.Find("WaveNumber")?.GetComponent<TextMeshProUGUI>() : null;
        var waveTimTxt = wavePanel != null ? wavePanel.transform.Find("WaveTimer")?.GetComponent<TextMeshProUGUI>()  : null;

        var so = new SerializedObject(hudCtrl);
        so.FindProperty("player1HUD").objectReferenceValue   = p1Element;
        so.FindProperty("player2HUD").objectReferenceValue   = p2Element;
        so.FindProperty("waveNumberText").objectReferenceValue = waveNumTxt;
        so.FindProperty("waveTimerText").objectReferenceValue  = waveTimTxt;
        so.ApplyModifiedProperties();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[WireHUD] Done — HUD fully wired.");
    }

    static PlayerHUDElement WirePlayerHUD(GameObject hudGo)
    {
        var el = hudGo.GetComponent<PlayerHUDElement>();
        if (el == null) el = hudGo.AddComponent<PlayerHUDElement>();

        var hpBar    = hudGo.transform.Find("HP_Bar")?.GetComponent<Slider>();
        var xpBar    = hudGo.transform.Find("XP_Bar")?.GetComponent<Slider>();
        var levelTxt = hudGo.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
        var respPanel= hudGo.transform.Find("RespawnPanel")?.gameObject;
        var respTxt  = respPanel != null
            ? respPanel.transform.Find("RespawnTimer")?.GetComponent<TextMeshProUGUI>()
            : null;

        var so = new SerializedObject(el);
        so.FindProperty("hpBar").objectReferenceValue           = hpBar;
        so.FindProperty("xpBar").objectReferenceValue           = xpBar;
        so.FindProperty("levelText").objectReferenceValue       = levelTxt;
        so.FindProperty("respawnPanel").objectReferenceValue    = respPanel;
        so.FindProperty("respawnTimerText").objectReferenceValue = respTxt;
        so.ApplyModifiedProperties();

        return el;
    }

    // Searches all root objects and their children, including inactive
    static GameObject FindInactive(string name)
    {
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var t = FindInChildren(root.transform, name);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    static Transform FindInChildren(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            var found = FindInChildren(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
#endif
