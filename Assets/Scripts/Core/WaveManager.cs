using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns enemies in waves. Each wave: 60 s timer, increasing difficulty and variety.
/// Waves 5/10/15... spawn a Boss.
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Config")]
    public float waveDuration   = 60f;
    public float difficultyBase = 0.15f;  // per-wave increment

    [Header("Spawn Points — assign in inspector")]
    public Transform[] spawnPoints;

    [Header("Enemy Prefabs — assign in inspector")]
    public GameObject runnerPrefab;
    public GameObject shooterPrefab;
    public GameObject brutePrefab;
    public GameObject invokerPrefab;
    public GameObject bossPrefab;

    [Header("Spawn Rate (enemies/sec) base")]
    public float baseSpawnRate = 0.25f;

    [Header("Grace period before first spawn each wave (seconds)")]
    public float spawnGracePeriod = 4f;

    // ── Runtime ───────────────────────────────────────────────────────────────
    public int   CurrentWave       { get; private set; } = 0;
    public float WaveTimeRemaining { get; private set; }
    public float CurrentDifficulty { get; private set; } = 1f;

    public static event System.Action<int> OnWaveStarted;
    public static event System.Action<int> OnWaveEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.Wave && CurrentWave == 0)
            StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        while (true)
        {
            // Wait if paused for level-up
            yield return new WaitUntil(() =>
                GameManager.Instance.CurrentState == GameState.Wave);

            CurrentWave++;
            CurrentDifficulty = 1f + CurrentWave * difficultyBase;
            WaveTimeRemaining = waveDuration;

            OnWaveStarted?.Invoke(CurrentWave);

            // Boss wave?
            if (CurrentWave % 5 == 0)
                SpawnBoss();

            // Start spawning enemies for this wave
            var spawnRoutine = StartCoroutine(SpawnLoop());

            // Count down
            while (WaveTimeRemaining > 0f)
            {
                yield return null;
                if (GameManager.Instance.CurrentState == GameState.Wave)
                    WaveTimeRemaining -= Time.deltaTime;
            }

            StopCoroutine(spawnRoutine);
            OnWaveEnded?.Invoke(CurrentWave);

            // Small break between waves
            yield return new WaitForSeconds(2f);
        }
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(spawnGracePeriod);
        while (true)
        {
            float interval = 1f / (baseSpawnRate * (1f + CurrentWave * 0.1f));
            yield return new WaitForSeconds(interval);

            if (GameManager.Instance.CurrentState != GameState.Wave) continue;

            SpawnEnemy(ChooseEnemyType());
        }
    }

    private GameObject ChooseEnemyType()
    {
        // Unlock variety as waves progress
        var options = new List<(GameObject prefab, int weight)>
        {
            (runnerPrefab, 5)
        };
        if (CurrentWave >= 2) options.Add((shooterPrefab, 3));
        if (CurrentWave >= 3) options.Add((brutePrefab, 2));
        if (CurrentWave >= 5) options.Add((invokerPrefab, 1));

        int total = 0;
        foreach (var o in options) total += o.weight;
        int roll = Random.Range(0, total);
        int cum  = 0;
        foreach (var o in options)
        {
            cum += o.weight;
            if (roll < cum) return o.prefab;
        }
        return runnerPrefab;
    }

    private void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null || spawnPoints.Length == 0) return;
        var sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        var go = Instantiate(prefab, sp.position, Quaternion.identity);
        go.GetComponent<EnemyBase>()?.ApplyDifficultyMultiplier(CurrentDifficulty);
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null || spawnPoints.Length == 0) return;
        var sp = spawnPoints[spawnPoints.Length / 2]; // center spawn
        Instantiate(bossPrefab, sp.position, Quaternion.identity);
        Debug.Log($"[WaveManager] BOSS spawned on wave {CurrentWave}");
    }
}
