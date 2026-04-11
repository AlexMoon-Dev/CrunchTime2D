using UnityEngine;
using UnityEditor;
using TMPro;

public class CreateDamageNumberPrefab
{
    public static void Execute()
    {
        // Build the GameObject
        var go = new GameObject("DamageNumber");

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text          = "0";
        tmp.fontSize      = 3.5f;
        tmp.color         = Color.white;
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.sortingOrder  = 10;

        go.AddComponent<DamageNumber>();

        // Save as prefab
        string path = "Assets/Prefabs/UI/DamageNumber.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        if (prefab != null)
            Debug.Log($"[CreateDamageNumberPrefab] Created prefab at {path}");
        else
            Debug.LogError("[CreateDamageNumberPrefab] Failed to create prefab.");
    }
}
