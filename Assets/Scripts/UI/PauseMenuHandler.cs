using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Lives in GameScene as an empty GameObject.
/// Esc toggles pause via GameManager. If the settings panel is open,
/// the first Esc closes it; the second Esc unpauses.
/// </summary>
public class PauseMenuHandler : MonoBehaviour
{
    [SerializeField] GameObject              pauseCanvas;
    [SerializeField] SettingsPanelController settingsPanel;

    bool _isPaused;

    void Update()
    {
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        // Layer 1: close settings panel if it's open
        if (_isPaused && settingsPanel.gameObject.activeSelf)
        {
            settingsPanel.Close();
            return;
        }

        // Layer 2: toggle pause — allowed in any state except GameOver
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.CurrentState != GameState.GameOver)
                Toggle();
        }
        else
        {
            Toggle(); // fallback when running scene directly in editor
        }
    }

    void Toggle() => SetPaused(!_isPaused);

    void SetPaused(bool paused)
    {
        _isPaused = paused;
        pauseCanvas.SetActive(paused);

        if (!paused)
            settingsPanel.Close();

        if (GameManager.Instance != null)
            GameManager.Instance.SetState(paused ? GameState.Paused : GameState.Wave);
        else
            Time.timeScale = paused ? 0f : 1f;
    }

    // ── Button callbacks ──────────────────────────────────────────────────────

    public void OnClickBack()     => SetPaused(false);

    public void OnClickSettings() => settingsPanel.Open();

    public void OnClickExit()
    {
        Time.timeScale = 1f;
        CombatEventSystem.ClearAll();
        SceneManager.LoadScene("MainMenu");
    }
}
