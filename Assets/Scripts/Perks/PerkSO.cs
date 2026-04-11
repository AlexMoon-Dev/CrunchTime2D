using UnityEngine;

/// <summary>
/// Base ScriptableObject for all perks.
/// Subclass this to add perk-specific logic.
///
/// Apply()  — one-time stat modification on perk grant.
/// Equip()  — subscribe to CombatEventSystem events for runtime hooks.
/// Unequip()— unsubscribe (called on scene reload / game restart).
/// </summary>
[CreateAssetMenu(menuName = "CrunchTime/Perk (base)", fileName = "NewPerk")]
public class PerkSO : ScriptableObject
{
    [Header("Identity")]
    public string perkName;
    [TextArea] public string description;
    public Sprite icon;    // TODO: assign art

    [Header("Class Weights (0 = never offered to this class, 10 = very common)")]
    [Range(0,10)] public int tankWeight    = 5;
    [Range(0,10)] public int fighterWeight = 5;
    [Range(0,10)] public int rangerWeight  = 5;

    public bool isClassPerk = false;   // Class Perks offered every 10 levels

    // ── Overridable hooks ─────────────────────────────────────────────────────

    /// <summary>Called once when the perk is granted. Apply flat stat deltas here.</summary>
    public virtual void Apply(PlayerStats stats) { }

    /// <summary>
    /// Called after Apply. Subscribe to CombatEventSystem here for runtime effects.
    /// The PlayerLeveling reference lets you grab any sibling component.
    /// </summary>
    public virtual void Equip(PlayerLeveling owner) { }

    /// <summary>Unsubscribe from events. Called by PerkDatabase on scene reset.</summary>
    public virtual void Unequip(PlayerLeveling owner) { }

    // ── Helpers ───────────────────────────────────────────────────────────────

    protected PlayerCombat GetCombat(PlayerLeveling owner)
        => owner.GetComponent<PlayerCombat>();

    protected PlayerStats GetStats(PlayerLeveling owner)
        => owner.GetComponent<PlayerStats>();

    protected PlayerController GetCtrl(PlayerLeveling owner)
        => owner.GetComponent<PlayerController>();
}
