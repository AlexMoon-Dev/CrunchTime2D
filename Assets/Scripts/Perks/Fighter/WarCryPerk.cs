using System.Collections;
using UnityEngine;
[CreateAssetMenu(menuName = "CrunchTime/Perks/Fighter/WarCry", fileName = "Perk_WarCry")]
public class WarCryPerk : PerkSO
{
    public float damageBuff   = 0.25f;
    public float buffDuration = 5f;
    public float cooldown     = 20f;

    public override void Equip(PlayerLeveling owner)
    {
        owner.StartCoroutine(WarCryRoutine(GetStats(owner)));
    }

    private IEnumerator WarCryRoutine(PlayerStats stats)
    {
        while (true)
        {
            yield return new WaitForSeconds(cooldown);
            stats.MultiplyAttackDamage(1f + damageBuff);
            yield return new WaitForSeconds(buffDuration);
            stats.MultiplyAttackDamage(1f / (1f + damageBuff));
        }
    }
}
