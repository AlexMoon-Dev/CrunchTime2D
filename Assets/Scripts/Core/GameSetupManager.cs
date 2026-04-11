using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Stores the player count and control-scheme choices made on the setup screen.
/// Applies them to the player GameObjects once confirmed.
/// Attach to the GameManager object.
/// </summary>
public class GameSetupManager : MonoBehaviour
{
    public static GameSetupManager Instance { get; private set; }

    [Header("Player 2 root GameObject — assign in Inspector")]
    public GameObject player2;

    public int    PlayerCount      { get; private set; } = 2;
    public string P1ControlScheme  { get; private set; } = "KeyboardMouse";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Called by PlayerCountSelectionUI once the player has made their choices.
    /// </summary>
    public void Apply(int playerCount, string p1Scheme)
    {
        PlayerCount     = playerCount;
        P1ControlScheme = p1Scheme;

        // Configure P1
        SetControlScheme(0, p1Scheme);

        if (playerCount == 2)
        {
            player2?.SetActive(true);
            SetControlScheme(1, "Gamepad");
        }
        else
        {
            // Disable P2 entirely — all FindObjectsByType calls will ignore it
            player2?.SetActive(false);
        }
    }

    private void SetControlScheme(int playerIndex, string scheme)
    {
        foreach (var setup in FindObjectsByType<PlayerSetup>(FindObjectsSortMode.None))
        {
            if (setup.playerIndex == playerIndex)
            {
                setup.ApplyControlScheme(scheme);
                return;
            }
        }
    }
}
