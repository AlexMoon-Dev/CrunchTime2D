using UnityEngine;
using TMPro;

/// <summary>
/// Manages the always-visible HUD: HP/XP bars per player, wave info, respawn timers.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Player HUD Elements")]
    public PlayerHUDElement player1HUD;
    public PlayerHUDElement player2HUD;

    [Header("Wave Info")]
    public TextMeshProUGUI waveNumberText;
    public TextMeshProUGUI waveTimerText;

    private void OnEnable()
    {
        WaveManager.OnWaveStarted        += OnWaveStarted;
        PlayerRespawnHandler.OnRespawnTimerStarted += OnRespawnStarted;
        PlayerRespawnHandler.OnPlayerRespawned     += OnPlayerRespawned;
    }

    private void OnDisable()
    {
        WaveManager.OnWaveStarted        -= OnWaveStarted;
        PlayerRespawnHandler.OnRespawnTimerStarted -= OnRespawnStarted;
        PlayerRespawnHandler.OnPlayerRespawned     -= OnPlayerRespawned;
    }

    private void Start()
    {
        // Bind player stats to HUD elements
        var allStats = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);
        foreach (var s in allStats)
        {
            var hud = s.playerIndex == 0 ? player1HUD : player2HUD;
            hud?.Bind(s, s.GetComponent<PlayerLeveling>());
        }
    }

    private void Update()
    {
        if (WaveManager.Instance == null) return;
        waveTimerText?.SetText($"{Mathf.CeilToInt(WaveManager.Instance.WaveTimeRemaining)}s");

        // Update respawn timers
        UpdateRespawnTimer(player1HUD, 0);
        UpdateRespawnTimer(player2HUD, 1);
    }

    private void OnWaveStarted(int wave)
    {
        waveNumberText?.SetText($"Wave {wave}");
    }

    private void OnRespawnStarted(PlayerRespawnHandler handler)
    {
        int idx = handler.GetComponent<PlayerStats>().playerIndex;
        var hud = idx == 0 ? player1HUD : player2HUD;
        hud?.ShowDead(true);
    }

    private void OnPlayerRespawned(PlayerRespawnHandler handler)
    {
        int idx = handler.GetComponent<PlayerStats>().playerIndex;
        var hud = idx == 0 ? player1HUD : player2HUD;
        hud?.ShowDead(false);
    }

    private void UpdateRespawnTimer(PlayerHUDElement hud, int playerIdx)
    {
        if (hud == null) return;
        var handlers = FindObjectsByType<PlayerRespawnHandler>(FindObjectsSortMode.None);
        foreach (var h in handlers)
        {
            if (h.GetComponent<PlayerStats>().playerIndex == playerIdx && h.IsWaiting)
            {
                hud.SetRespawnTimer(h.RespawnTimeLeft);
                return;
            }
        }
    }
}
