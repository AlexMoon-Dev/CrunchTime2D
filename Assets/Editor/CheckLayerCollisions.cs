using UnityEngine;
using UnityEditor;

public class CheckLayerCollisions
{
    public static void Execute()
    {
        int player   = LayerMask.NameToLayer("Player");
        int ground   = LayerMask.NameToLayer("Ground");
        int platform = LayerMask.NameToLayer("Platform");
        int enemy    = LayerMask.NameToLayer("Enemy");

        Debug.Log($"[LayerCheck] Player({player}) vs Ground({ground}): collides={!Physics2D.GetIgnoreLayerCollision(player, ground)}");
        Debug.Log($"[LayerCheck] Player({player}) vs Platform({platform}): collides={!Physics2D.GetIgnoreLayerCollision(player, platform)}");
        Debug.Log($"[LayerCheck] Player({player}) vs Enemy({enemy}): collides={!Physics2D.GetIgnoreLayerCollision(player, enemy)}");
        Debug.Log($"[LayerCheck] Enemy({enemy}) vs Ground({ground}): collides={!Physics2D.GetIgnoreLayerCollision(enemy, ground)}");
    }
}
