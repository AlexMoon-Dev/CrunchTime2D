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
    private ClassType[]          _selectedClasses = { ClassType.None, ClassType.None };
    private ClassDefinitionSO[]  _selectedDefs    = { null, null };
    private bool[]               _confirmed        = { false, false };

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
    public bool TryConfirmClass(int playerIndex, ClassDefinitionSO def)
    {
        // Ranger can only be chosen by one player
        if (def.classType == ClassType.Ranger && IsClassLocked(ClassType.Ranger))
        {
            Debug.LogWarning("[ClassManager] Only one Ranger allowed!");
            return false;
        }

        _selectedClasses[playerIndex] = def.classType;
        _selectedDefs[playerIndex]    = def;
        _confirmed[playerIndex]       = true;

        OnClassConfirmed?.Invoke(playerIndex, def.classType);
        OnClassLocked?.Invoke(def.classType);

        CheckBothConfirmed();
        return true;
    }

    private void CheckBothConfirmed()
    {
        bool singlePlayer = GameSetupManager.Instance != null
            && GameSetupManager.Instance.PlayerCount == 1;

        bool p2Done = singlePlayer || _confirmed[1];
        if (!_confirmed[0] || !p2Done) return;

        // Apply stats directly from the stored SO — no Resources.Load needed
        var players = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            _selectedDefs[p.playerIndex]?.ApplyBaseStats(p);
        }
        GameManager.Instance?.StartWave();
    }

    public ClassType GetClass(int playerIndex) => _selectedClasses[playerIndex];
}
