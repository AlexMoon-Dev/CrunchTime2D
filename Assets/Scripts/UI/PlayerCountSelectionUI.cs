using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// First screen shown on game start.
///
/// Step 1 — How many players?  (1P / 2P buttons)
/// Step 2 — (1P only) Which control scheme?  (Keyboard / Controller)
///           If Controller is chosen and no gamepad is connected, waits for one.
///
/// 2P always defaults: P1 = Keyboard+Mouse, P2 = Gamepad.
/// </summary>
public class PlayerCountSelectionUI : MonoBehaviour
{
    [Header("Root panel — this whole screen")]
    public GameObject panel;

    [Header("Step 1 — Player count")]
    public GameObject countPanel;
    public Button     onePlayerButton;
    public Button     twoPlayersButton;

    [Header("Step 2 — Control scheme (1P only)")]
    public GameObject schemePanel;
    public Button     keyboardButton;
    public Button     controllerButton;

    [Header("Waiting for controller sub-panel")]
    public GameObject      waitPanel;
    public TextMeshProUGUI waitText;

    [Header("Next screen to show after selection")]
    public LevelUpUIController levelUpUI;

    private bool _waitingForController;

    private void Start()
    {
        // Show only Step 1 initially
        panel.SetActive(true);
        countPanel.SetActive(true);
        schemePanel.SetActive(false);
        waitPanel?.SetActive(false);

        onePlayerButton.onClick.AddListener(OnOnePlayer);
        twoPlayersButton.onClick.AddListener(OnTwoPlayers);
        keyboardButton.onClick.AddListener(OnKeyboard);
        controllerButton.onClick.AddListener(OnController);

        // Enable the UI action map — PlayerInput only enables "Player" map by default,
        // which leaves the UI map disabled and blocks gamepad navigation of buttons.
        var pi = FindFirstObjectByType<PlayerInput>();
        pi?.actions.FindActionMap("UI")?.Enable();

        // Give gamepad/keyboard navigation a starting point
        EventSystem.current?.SetSelectedGameObject(onePlayerButton.gameObject);
    }

    private void OnDestroy()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    // ── Step 1 ────────────────────────────────────────────────────────────────

    private void OnOnePlayer()
    {
        countPanel.SetActive(false);
        schemePanel.SetActive(true);
        EventSystem.current?.SetSelectedGameObject(keyboardButton.gameObject);
    }

    private void OnTwoPlayers()
    {
        // 2P: P1 keyboard, P2 controller — no further choice
        Proceed(2, "KeyboardMouse");
    }

    // ── Step 2 ────────────────────────────────────────────────────────────────

    private void OnKeyboard()
    {
        Proceed(1, "KeyboardMouse");
    }

    private void OnController()
    {
        if (Gamepad.all.Count > 0)
        {
            Proceed(1, "Gamepad");
            return;
        }

        // No controller connected — wait for one
        _waitingForController = true;
        keyboardButton.interactable  = false;
        controllerButton.interactable = false;
        waitPanel?.SetActive(true);
        waitText?.SetText("Waiting for controller...\nPlease connect a gamepad.");
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (!_waitingForController) return;
        if (device is Gamepad && change == InputDeviceChange.Added)
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
            _waitingForController = false;
            Proceed(1, "Gamepad");
        }
    }

    // ── Common ────────────────────────────────────────────────────────────────

    private void Proceed(int playerCount, string p1Scheme)
    {
        panel.SetActive(false);
        GameSetupManager.Instance?.Apply(playerCount, p1Scheme);
        GameManager.Instance?.SetState(GameState.ClassSelection);
        levelUpUI?.ShowClassSelection();
    }
}
