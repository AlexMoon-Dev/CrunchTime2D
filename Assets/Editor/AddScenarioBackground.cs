#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Adds Scenario.jpeg as a world-space background sprite to GameScene.
/// Run via  CrunchTime ▶ Add Background to GameScene
/// </summary>
public static class AddScenarioBackground
{
    const string ScenePath  = "Assets/Scenes/GameScene.unity";
    const string SpritePath = "Assets/ART/Scenario.jpeg";
    const string GoName     = "ScenarioBackground";

    [MenuItem("CrunchTime/Add Background to GameScene")]
    public static void AddBackground()
    {
        // Load the Scenario sprite
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite == null)
        {
            Debug.LogError($"[CrunchTime] Sprite not found at {SpritePath}");
            return;
        }

        // Open GameScene (saves current scene first with a dialog if dirty)
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // Remove stale background if it already exists
        var existing = GameObject.Find(GoName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
            Debug.Log("[CrunchTime] Replaced existing ScenarioBackground.");
        }

        // Create the background GameObject
        var go = new GameObject(GoName);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = -100;   // render behind all other sprites

        // Scale to "cover" the camera view (no letterboxing)
        // Camera: orthographic size = 5  →  view height = 10 units
        // Sprite: 1024×765 px at 100 PPU  →  10.24 × 7.65 world units
        // Target: 16:9 aspect  →  17.78 × 10 units
        // Cover scale = max(17.78/10.24, 10/7.65) = max(1.736, 1.307) = 1.736
        var cam = FindMainCamera();
        float scale;
        if (cam != null)
        {
            float camH   = cam.orthographicSize * 2f;
            float camW   = camH * cam.aspect;
            float sprW   = sprite.bounds.size.x;
            float sprH   = sprite.bounds.size.y;
            scale = Mathf.Max(camW / sprW, camH / sprH);
        }
        else
        {
            scale = 1.736f;   // fallback for 16:9 + ortho size 5
        }

        go.transform.localScale = new Vector3(scale, scale, 1f);
        go.transform.position   = new Vector3(0f, 0f, 10f);   // z=10 keeps it behind world geometry

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log($"[CrunchTime] ScenarioBackground added to GameScene (scale {scale:F3}).");
    }

    static Camera FindMainCamera()
    {
        var cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (var c in cameras)
            if (c.CompareTag("MainCamera")) return c;
        return cameras.Length > 0 ? cameras[0] : null;
    }
}
#endif
