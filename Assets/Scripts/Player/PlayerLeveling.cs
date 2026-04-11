using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks XP and level for one player. Fires events consumed by LevelUpManager.
/// </summary>
public class PlayerLeveling : MonoBehaviour
{
    [Header("XP Curve")]
    public float xpExponent = 1.2f;     // xpThreshold = 100 * level^exponent
    public float xpBase     = 100f;

    [Header("Runtime")]
    [SerializeField] private int   _level   = 1;
    [SerializeField] private float _xp      = 0f;
    [SerializeField] private float _xpToNext;

    public int   Level   => _level;
    public float XP      => _xp;
    public float XPToNext => _xpToNext;

    public event Action<PlayerLeveling> OnLevelUp;

    // Collected perk names (used by PlayerCombat.HasPerk)
    private readonly List<string> _perkNames = new List<string>();
    // Full perk SO references (for PerkHistoryPanel display)
    private readonly List<PerkSO>  _perks     = new List<PerkSO>();

    private void Awake()
    {
        _xpToNext = CalcThreshold(_level);
    }

    public void AddXP(float amount)
    {
        _xp += amount;
        while (_xp >= _xpToNext)
        {
            _xp      -= _xpToNext;
            _level++;
            _xpToNext = CalcThreshold(_level);
            OnLevelUp?.Invoke(this);
        }
    }

    public void GrantPerk(PerkSO perk)
    {
        perk.Apply(GetComponent<PlayerStats>());
        perk.Equip(this);
        _perks.Add(perk);
        _perkNames.Add(perk.perkName);
    }

    public bool HasPerk(string name) => _perkNames.Contains(name);

    public IReadOnlyList<PerkSO> CollectedPerks => _perks;

    private float CalcThreshold(int level) => xpBase * Mathf.Pow(level, xpExponent);
}
