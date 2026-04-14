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
    public int playerIndex = 0;   // 0 = P1 Male (KB+Mouse), 1 = P2 Female (Gamepad)

    [Header("Animator Controllers")]
    public RuntimeAnimatorController maleController;    // PlayerAnimator.controller
    public RuntimeAnimatorController femaleController;  // PlayerAnimator_Female.overrideController

    private void Awake()
    {
        GetComponent<PlayerStats>().playerIndex = playerIndex;

        // Apply character-specific animator
        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            var ctrl = playerIndex == 0 ? maleController : femaleController;
            if (ctrl != null) anim.runtimeAnimatorController = ctrl;
        }
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
