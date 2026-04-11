using UnityEngine;
using TMPro;

/// <summary>
/// Pause overlay showing all collected perks for each player (Hades-style).
/// Toggle with Escape / Start.
/// </summary>
public class PerkHistoryPanel : MonoBehaviour
{
    [Header("Panels")]
    public GameObject overlay;
    public Transform  p1PerkList;
    public Transform  p2PerkList;

    [Header("Prefab")]
    public PerkHistoryEntry entryPrefab;

    private bool _visible = false;

    private void Update()
    {
        // Tab key also toggles (keyboard shortcut)
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame)
            Toggle();
    }

    public void Toggle()
    {
        _visible = !_visible;
        overlay?.SetActive(_visible);
        if (_visible) Refresh();
    }

    private void Refresh()
    {
        PopulateList(p1PerkList, 0);
        PopulateList(p2PerkList, 1);
    }

    private void PopulateList(Transform container, int playerIndex)
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);

        PlayerLeveling leveling = null;
        foreach (var lv in FindObjectsByType<PlayerLeveling>(FindObjectsSortMode.None))
        {
            if (lv.GetComponent<PlayerStats>().playerIndex == playerIndex)
            { leveling = lv; break; }
        }

        if (leveling == null) return;
        foreach (var perk in leveling.CollectedPerks)
        {
            var entry = Instantiate(entryPrefab, container);
            entry.Setup(perk);
        }
    }
}
