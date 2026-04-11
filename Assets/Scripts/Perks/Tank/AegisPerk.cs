using System.Collections;
using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Tank/Aegis", fileName = "Perk_Aegis")]
public class AegisPerk : PerkSO
{
    public float shieldPercent  = 0.20f;  // 20% of max armor as temp HP
    public float rechargeTime   = 10f;

    public override void Equip(PlayerLeveling owner)
    {
        var stats = GetStats(owner);
        owner.StartCoroutine(ShieldRoutine(stats));
    }

    private IEnumerator ShieldRoutine(PlayerStats stats)
    {
        while (true)
        {
            yield return new WaitForSeconds(rechargeTime);
            float shield = stats.armor * shieldPercent;
            stats.Heal(shield);
        }
    }
}
