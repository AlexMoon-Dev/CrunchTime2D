using System.Collections;
using UnityEngine;

/// <summary>
/// Handles death visuals, respawn timer, and GAME OVER detection.
/// </summary>
public class PlayerRespawnHandler : MonoBehaviour
{
    [Header("Respawn Settings")]
    public float baseRespawnTime = 5f;
    public float deathPenalty    = 3f;   // extra seconds per death
    public Transform respawnPoint;       // set to center of arena

    private PlayerStats      _stats;
    private PlayerController _ctrl;
    private int   _deathCount   = 0;
    private bool  _waitingToRespawn = false;

    public bool IsWaiting => _waitingToRespawn;
    public int  DeathCount => _deathCount;
    public float RespawnTimeLeft { get; private set; }

    public static event System.Action<PlayerRespawnHandler> OnRespawnTimerStarted;
    public static event System.Action<PlayerRespawnHandler> OnPlayerRespawned;

    private void Awake()
    {
        _stats = GetComponent<PlayerStats>();
        _ctrl  = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        _stats.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        _stats.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (_waitingToRespawn) return;
        _deathCount++;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        _waitingToRespawn = true;

        // Check if all other players are also dead → GAME OVER
        // We delay 1 frame so other death handlers can run first
        yield return null;

        bool allDead = AreAllPlayersDead();
        if (allDead)
        {
            GameManager.Instance?.TriggerGameOver();
            yield break;
        }

        // Disable movement and combat
        SetPlayerActive(false);
        OnRespawnTimerStarted?.Invoke(this);

        float timer = baseRespawnTime + (_deathCount - 1) * deathPenalty;
        RespawnTimeLeft = timer;
        while (RespawnTimeLeft > 0f)
        {
            yield return null;
            RespawnTimeLeft -= Time.deltaTime;

            // While waiting, if the OTHER player dies: GAME OVER
            if (AreAllPlayersDead())
            {
                GameManager.Instance?.TriggerGameOver();
                yield break;
            }
        }

        Respawn();
    }

    private void Respawn()
    {
        _waitingToRespawn = false;
        if (respawnPoint != null)
            transform.position = respawnPoint.position;

        // Restore to 50% health — use ReviveWithHealth to bypass the IsDead guard
        var stats = GetComponent<PlayerStats>();
        stats.ReviveWithHealth(stats.maxHealth * 0.5f);

        SetPlayerActive(true);
        OnPlayerRespawned?.Invoke(this);
    }

    private void SetPlayerActive(bool active)
    {
        if (_ctrl != null) _ctrl.enabled = active;
        var combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.enabled = active;

        // Ghost visual: tint sprite grey when dead
        // TODO: swap for a proper ghost shader/animation
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = active ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }

    private static bool AreAllPlayersDead()
    {
        var allHandlers = FindObjectsByType<PlayerRespawnHandler>(FindObjectsSortMode.None);
        foreach (var h in allHandlers)
        {
            // A player who is alive (not waiting to respawn and not dead stats-wise)
            if (!h._waitingToRespawn && !h._stats.IsDead)
                return false;
        }
        return true;
    }
}
