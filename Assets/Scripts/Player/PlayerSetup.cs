using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Glues all player components together at startup.
/// LevelUpManager subscribes to OnLevelUp via FindObjectsByType in its own OnEnable,
/// so no manual wiring is needed here.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerLeveling))]
[RequireComponent(typeof(PlayerRespawnHandler))]
public class PlayerSetup : MonoBehaviour
{
    [Header("Identity")]
    public int playerIndex = 0;   // 0 = P1 (KB+Mouse), 1 = P2 (Gamepad)

    [Header("Visual — placeholder colored sprite")]
    public Color playerColor = Color.blue;

    private void Awake()
    {
        GetComponent<PlayerStats>().playerIndex = playerIndex;

        // Color-code players for placeholder art
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = playerColor;
    }

    /// <summary>
    /// Called by GameSetupManager after the player count / control scheme screen.
    /// </summary>
    public void ApplyControlScheme(string scheme)
    {
        var pi = GetComponent<PlayerInput>();
        if (scheme == "KeyboardMouse" && Keyboard.current != null)
            pi.SwitchCurrentControlScheme("KeyboardMouse", Keyboard.current, Mouse.current);
        else if (scheme == "Gamepad" && Gamepad.all.Count > 0)
            pi.SwitchCurrentControlScheme("Gamepad", Gamepad.all[0]);
    }
}
