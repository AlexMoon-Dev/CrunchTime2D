using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Tank/GuardiansOath", fileName = "Perk_GuardiansOath")]
public class GuardiansOathPerk : PerkSO
{
    public float triggerThreshold = 0.25f;  // 25% HP
    public float damageReduction  = 0.30f;
    public float auraRadius       = 5f;

    public override void Equip(PlayerLeveling owner)
    {
        var myStats = GetStats(owner);
        // Reduce damage taken by ALL players within radius when any ally is below threshold
        CombatEventSystem.OnPlayerHit += (ps, ctx) =>
        {
            // Check if any player near this tank is below threshold
            var allStats = Object.FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);
            bool allyLow = false;
            foreach (var s in allStats)
            {
                if (s == myStats) continue;
                if (s.CurrentHealth / s.maxHealth < triggerThreshold &&
                    Vector2.Distance(s.transform.position, myStats.transform.position) <= auraRadius)
                {
                    allyLow = true;
                    break;
                }
            }
            if (allyLow && ps == myStats)
                ctx.damageMultiplier *= (1f - damageReduction);
        };
    }
}
