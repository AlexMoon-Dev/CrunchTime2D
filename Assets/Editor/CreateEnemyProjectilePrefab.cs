using UnityEngine;
using UnityEditor;

public class CreateEnemyProjectilePrefab
{
    public static void Execute()
    {
        var go = new GameObject("EnemyProjectile");
        go.layer = LayerMask.NameToLayer("Default");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        sr.color  = new Color(1f, 0.3f, 0.1f); // red-orange

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.12f;

        go.AddComponent<EnemyProjectile>();

        string path   = "Assets/Prefabs/Projectiles/EnemyProjectile.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        if (prefab == null) { Debug.LogError("Failed to create EnemyProjectile prefab."); return; }

        // Assign to Shooter prefab
        string shooterPath = "Assets/Prefabs/Enemies/Shooter.prefab";
        var shooterPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>(shooterPath);
        if (shooterPrefab == null) { Debug.LogError("Shooter prefab not found."); return; }

        var shooter = shooterPrefab.GetComponent<ShooterEnemy>();
        if (shooter == null) { Debug.LogError("ShooterEnemy component not found."); return; }

        shooter.projectilePrefab = prefab;
        EditorUtility.SetDirty(shooterPrefab);
        AssetDatabase.SaveAssets();

        Debug.Log("[CreateEnemyProjectilePrefab] Done — EnemyProjectile prefab created and assigned to Shooter.");
    }
}
