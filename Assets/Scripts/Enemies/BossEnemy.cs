using System.Collections;
using UnityEngine;

/// <summary>
/// Scaled-up Brute. Ground slam special every 8 seconds that fires a horizontal shockwave.
/// Bonus XP split between all living players on kill.
/// </summary>
public class BossEnemy : BruteEnemy
{
    [Header("Boss")]
    public float slamCooldown    = 8f;
    public float slamDamage      = 25f;
    public float shockwaveSpeed  = 8f;
    public float shockwaveWidth  = 0.8f;
    public float bonusXP         = 200f;
    public GameObject shockwavePrefab;  // TODO: assign placeholder

    private float _slamTimer;

    public override void Awake()
    {
        base.Awake();
        // Boss is 5x normal Brute
        maxHealth      = 100f * 5f;
        damage         = 18f * 1.5f;
        moveSpeed      = 1.5f;
        xpValue        = 0f; // handled in Die() override
        armor          = 10f;
        _currentHealth = maxHealth;
        _slamTimer     = slamCooldown;   // wait a full cycle before first slam
    }

    protected override void Behave()
    {
        base.Behave();

        _slamTimer -= Time.deltaTime;
        if (_slamTimer <= 0f)
        {
            _slamTimer = slamCooldown;
            StartCoroutine(GroundSlam());
        }
    }

    private IEnumerator GroundSlam()
    {
        // Brief telegraph
        yield return new WaitForSeconds(0.4f);

        // Spawn or inline shockwave in both directions
        for (int dir = -1; dir <= 1; dir += 2)
            SpawnShockwave(dir);
    }

    private void SpawnShockwave(int direction)
    {
        if (shockwavePrefab != null)
        {
            var go = Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
            var sw = go.GetComponent<Shockwave>();
            if (sw != null) { sw.damage = slamDamage; sw.direction = direction; }
            return;
        }

        // Inline fallback: sweep a box across the ground
        StartCoroutine(InlineShockwave(direction));
    }

    private IEnumerator InlineShockwave(int direction)
    {
        LayerMask playerMask = LayerMask.GetMask("Player");
        Vector2 pos = transform.position;
        float traveled = 0f;
        float maxDist  = 20f;

        while (traveled < maxDist)
        {
            var hits = Physics2D.OverlapBoxAll(pos, new Vector2(shockwaveWidth, 1f), 0f, playerMask);
            foreach (var h in hits)
            {
                var ps = h.GetComponent<PlayerStats>();
                if (ps != null)
                {
                    var ctx = new DamageContext(slamDamage, DamageType.AoE, gameObject,
                        new Vector2(direction * 5f, 4f));
                    ps.TakeDamage(ctx);
                }
            }
            pos.x     += direction * shockwaveSpeed * 0.05f;
            traveled  += shockwaveSpeed * 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
    }

    protected override void Die()
    {
        _currentHealth = 0f;
        // Bonus XP to all living players
        var players = FindObjectsByType<PlayerLeveling>(FindObjectsSortMode.None);
        float split = bonusXP / Mathf.Max(1, players.Length);
        foreach (var p in players)
        {
            var stats = p.GetComponent<PlayerStats>();
            if (stats != null && !stats.IsDead) p.AddXP(xpValue + split);
        }
        // TODO: death VFX
        Destroy(gameObject);
    }
}
