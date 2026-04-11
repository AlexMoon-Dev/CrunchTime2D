using System.Collections;
using UnityEngine;

/// <summary>
/// Stays at the back. Every 8 seconds summons a random enemy type.
/// Punishes players who ignore it.
/// </summary>
public class InvokerEnemy : EnemyBase
{
    [Header("Invoker")]
    public float summonInterval = 8f;
    public GameObject runnerPrefab;   // TODO: assign
    public GameObject shooterPrefab;  // TODO: assign
    public GameObject brutePrefab;    // TODO: assign

    public override void Awake()
    {
        base.Awake();
        maxHealth      = 60f;
        damage         = 0f;
        moveSpeed      = 1.5f;
        xpValue        = 40f;
        _currentHealth = maxHealth;
    }

    public override void Start()
    {
        base.Start();
        StartCoroutine(SummonRoutine());
    }

    protected override void Behave()
    {
        if (_target == null) return;

        // Flee from players — stay at back of arena
        float dist = Vector2.Distance(transform.position, _target.position);
        if (dist < 5f)
        {
            Vector2 away = ((Vector2)transform.position - (Vector2)_target.position).normalized;
            _rb.linearVelocity = new Vector2(away.x * moveSpeed, _rb.linearVelocity.y);
        }
        else
        {
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        }
    }

    private IEnumerator SummonRoutine()
    {
        while (!IsDead)
        {
            yield return new WaitForSeconds(summonInterval);
            SummonRandom();
        }
    }

    private void SummonRandom()
    {
        var prefabs = new[] { runnerPrefab, shooterPrefab, brutePrefab };
        // Filter nulls
        var valid = System.Array.FindAll(prefabs, p => p != null);
        if (valid.Length == 0) return;

        var chosen = valid[Random.Range(0, valid.Length)];
        Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0.5f, 0f);
        var go = Instantiate(chosen, transform.position + offset, Quaternion.identity);

        // Apply current wave difficulty
        if (WaveManager.Instance != null)
            go.GetComponent<EnemyBase>()?.ApplyDifficultyMultiplier(WaveManager.Instance.CurrentDifficulty);
    }
}
