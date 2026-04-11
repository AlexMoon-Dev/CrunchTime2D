using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Listens for OnLevelUp from all players. When triggered, pauses the game
/// and coordinates the UI so both players must confirm before resuming.
/// </summary>
public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance { get; private set; }

    [Header("References")]
    public LevelUpUIController levelUpUI;
    public PerkDatabase        perkDatabase;

    // Pending state — cleared after both players confirm
    private readonly bool[]              _confirmed  = { false, false };
    private readonly List<PerkSO>[]      _offerings  = { new List<PerkSO>(), new List<PerkSO>() };
    private readonly bool[]              _levelUpPending = { false, false };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        // Subscribe to every player's level-up event
        foreach (var lv in FindObjectsByType<PlayerLeveling>(FindObjectsSortMode.None))
            lv.OnLevelUp += HandleLevelUp;
    }

    private void OnDisable()
    {
        foreach (var lv in FindObjectsByType<PlayerLeveling>(FindObjectsSortMode.None))
            lv.OnLevelUp -= HandleLevelUp;
    }

    private void HandleLevelUp(PlayerLeveling leveling)
    {
        int idx = leveling.GetComponent<PlayerStats>().playerIndex;
        _levelUpPending[idx] = true;

        // Build perk offerings for this player
        bool isClassPerkLevel = (leveling.Level % 10 == 0);
        var classType         = leveling.GetComponent<PlayerStats>().EquippedClass;
        var owned             = new List<string>();
        foreach (var p in leveling.CollectedPerks) owned.Add(p.perkName);

        _offerings[idx].Clear();
        if (isClassPerkLevel)
        {
            // Offer class perks from ClassDefinitionSO
            var classDef = Resources.Load<ClassDefinitionSO>($"Classes/{classType}");
            if (classDef != null)
                _offerings[idx].AddRange(classDef.classPerks);
            if (_offerings[idx].Count == 0)  // fallback to normal perks
                _offerings[idx].AddRange(perkDatabase.GetWeightedSelection(classType, owned, 3));
        }
        else
        {
            _offerings[idx].AddRange(perkDatabase.GetWeightedSelection(classType, owned, 3));
        }

        // If both players are pending, show UI
        if (_levelUpPending[0] && _levelUpPending[1])
            TriggerUI();
        else
            TriggerUI(); // Single-player case or staggered — show for the one pending
    }

    private void TriggerUI()
    {
        GameManager.Instance?.SetState(GameState.LevelUp);
        // Only reset confirmation for players who have a new pending level-up.
        // A player who already confirmed this pause keeps their confirmed state.
        for (int i = 0; i < 2; i++)
            if (_levelUpPending[i]) _confirmed[i] = false;
        levelUpUI?.Show(_offerings[0], _offerings[1]);
    }

    /// <summary>Called by UI when a player confirms their perk pick.</summary>
    public void ConfirmPerk(int playerIndex, PerkSO chosen)
    {
        var leveling = GetPlayerLeveling(playerIndex);
        leveling?.GrantPerk(chosen);

        _confirmed[playerIndex]      = true;
        _levelUpPending[playerIndex] = false;

        levelUpUI?.SetWaiting(playerIndex);

        if (BothConfirmed())
            ResumeGame();
    }

    private bool BothConfirmed()
    {
        // In single-player (only P1), P2 is vacuously confirmed
        bool p2Active = false;
        foreach (var lv in FindObjectsByType<PlayerLeveling>(FindObjectsSortMode.None))
            if (lv.GetComponent<PlayerStats>().playerIndex == 1) { p2Active = true; break; }

        return _confirmed[0] && (!p2Active || _confirmed[1]);
    }

    private void ResumeGame()
    {
        levelUpUI?.Hide();
        GameManager.Instance?.SetState(GameState.Wave);
    }

    private PlayerLeveling GetPlayerLeveling(int idx)
    {
        foreach (var lv in FindObjectsByType<PlayerLeveling>(FindObjectsSortMode.None))
            if (lv.GetComponent<PlayerStats>().playerIndex == idx) return lv;
        return null;
    }
}
