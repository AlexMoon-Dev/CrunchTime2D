using System;
using UnityEngine;

/// <summary>
/// Enforces class-lock: the first player to pick a class locks it for the other.
/// Fires events so ClassSelectionUI can gray out cards in real time.
/// </summary>
public class ClassManager : MonoBehaviour
{
    public static ClassManager Instance { get; private set; }

    // Indexed by playerIndex (0 / 1)
    private ClassType[] _selectedClasses = { ClassType.None, ClassType.None };
    private bool[]      _confirmed        = { false, false };

    public static event Action<int, ClassType> OnClassConfirmed;  // (playerIndex, class)
    public static event Action<ClassType>      OnClassLocked;     // class is now unavailable

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool IsClassLocked(ClassType ct)
    {
        foreach (var sel in _selectedClasses)
            if (sel == ct && ct != ClassType.None) return true;
        return false;
    }

    /// <summary>Returns false if the class is already locked by the other player.</summary>
    public bool TryConfirmClass(int playerIndex, ClassType ct)
    {
        // Ranger can only be chosen by one player
        if (ct == ClassType.Ranger && IsClassLocked(ClassType.Ranger))
        {
            Debug.LogWarning("[ClassManager] Only one Ranger allowed!");
            return false;
        }

        _selectedClasses[playerIndex] = ct;
        _confirmed[playerIndex]       = true;

        OnClassConfirmed?.Invoke(playerIndex, ct);
        OnClassLocked?.Invoke(ct);


        CheckBothConfirmed();
        return true;
    }

    private void CheckBothConfirmed()
    {
        if (!_confirmed[0] || !_confirmed[1]) return;
        // Apply stats to each player and transition to wave
        var players = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            var cls = _selectedClasses[p.playerIndex];
            var def = Resources.Load<ClassDefinitionSO>($"Classes/{cls}");
            def?.ApplyBaseStats(p);
        }
        GameManager.Instance?.StartWave();
    }

    public ClassType GetClass(int playerIndex) => _selectedClasses[playerIndex];
}
