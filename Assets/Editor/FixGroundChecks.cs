using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FixGroundChecks
{
    public static void Execute()
    {
        int fixed_count = 0;
        foreach (var name in new[] { "Player1", "Player2" })
        {
            var playerGO = GameObject.Find(name);
            if (playerGO == null) { Debug.LogWarning($"[FixGroundChecks] {name} not found."); continue; }

            var gc = playerGO.transform.Find("GroundCheck");
            if (gc == null) { Debug.LogWarning($"[FixGroundChecks] GroundCheck not found under {name}."); continue; }

            gc.localPosition = new Vector3(0f, -0.62f, 0f);
            EditorUtility.SetDirty(gc.gameObject);
            fixed_count++;
            Debug.Log($"[FixGroundChecks] {name}/GroundCheck moved to y=-0.62");
        }

        if (fixed_count > 0)
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
}
