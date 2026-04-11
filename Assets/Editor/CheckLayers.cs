using UnityEngine;
using UnityEditor;

public class CheckLayers
{
    public static void Execute()
    {
        int groundLayer   = LayerMask.NameToLayer("Ground");
        int platformLayer = LayerMask.NameToLayer("Platform");
        int playerLayer   = LayerMask.NameToLayer("Player");
        int enemyLayer    = LayerMask.NameToLayer("Enemy");

        Debug.Log($"[CheckLayers] Ground={groundLayer}, Platform={platformLayer}, Player={playerLayer}, Enemy={enemyLayer}");

        // Also log what mask value we'd need for Ground+Platform
        int mask = (1 << groundLayer) | (1 << platformLayer);
        Debug.Log($"[CheckLayers] groundLayers mask value = {mask}");
    }
}
