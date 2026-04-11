using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { ClassSelection, Wave, LevelUp, Paused, GameOver }

/// <summary>
/// Singleton. Owns game-state transitions and is the single source of truth
/// for whether the game is running/paused/over.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.ClassSelection;

    public static event Action<GameState> OnGameStateChanged;

    [Header("References")]
    public PlayerStats[] players; // 0 = P1, 1 = P2

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        SetState(GameState.ClassSelection);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        // Only Wave runs at normal speed; ClassSelection, LevelUp, Paused, GameOver all freeze time
        Time.timeScale = (newState == GameState.Wave) ? 1f : 0f;
        OnGameStateChanged?.Invoke(newState);
    }

    public void StartWave()
    {
        SetState(GameState.Wave);
    }

    public void TriggerGameOver()
    {
        SetState(GameState.GameOver);
        CombatEventSystem.ClearAll();
        // TODO: show game-over screen / restart prompt
        Debug.Log("[GameManager] GAME OVER");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        CombatEventSystem.ClearAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
