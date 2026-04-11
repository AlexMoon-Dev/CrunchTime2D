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

    private void Start()
    {
        // Force the correct control scheme on startup
        var pi = GetComponent<PlayerInput>();
        if (playerIndex == 0)
            pi.SwitchCurrentControlScheme("KeyboardMouse",
                Keyboard.current, Mouse.current);
        else if (Gamepad.current != null)
            pi.SwitchCurrentControlScheme("Gamepad", Gamepad.current);
    }
}
