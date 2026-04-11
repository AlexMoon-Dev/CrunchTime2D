using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixSceneDefaults
{
    public static void Execute()
    {
        int fixed_count = 0;

        // 1. Player2 must start INACTIVE so AreAllPlayersDead() works correctly in 1P mode.
        //    GameSetupManager.Apply() re-activates it when the user picks 2-player.
        var player2 = GameObject.Find("Player2");
        if (player2 == null)
        {
            // Try finding inactive objects
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allObjects)
            {
                if (go.name == "Player2" && go.scene.IsValid())
                {
                    player2 = go;
                    break;
                }
            }
        }
        if (player2 != null && player2.activeSelf)
        {
            player2.SetActive(false);
            EditorUtility.SetDirty(player2);
            Debug.Log("[FixSceneDefaults] Player2 set to INACTIVE.");
            fixed_count++;
        }
        else if (player2 != null)
        {
            Debug.Log("[FixSceneDefaults] Player2 already inactive — OK.");
        }
        else
        {
            Debug.LogWarning("[FixSceneDefaults] Player2 not found.");
        }

        // 2. GameOverPanel must start ACTIVE so GameOverUI.Awake() can subscribe to events.
        //    GameOverUI.Start() will call panel.SetActive(false) to hide it immediately.
        //    The static event subscription survives the deactivation.
        var allObjects2 = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject gameOverPanel = null;
        foreach (var go in allObjects2)
        {
            if (go.name == "GameOverPanel" && go.scene.IsValid())
            {
                gameOverPanel = go;
                break;
            }
        }
        if (gameOverPanel != null && !gameOverPanel.activeSelf)
        {
            gameOverPanel.SetActive(true);
            EditorUtility.SetDirty(gameOverPanel);
            Debug.Log("[FixSceneDefaults] GameOverPanel set to ACTIVE (Awake subscription will work).");
            fixed_count++;
        }
        else if (gameOverPanel != null)
        {
            Debug.Log("[FixSceneDefaults] GameOverPanel already active — OK.");
        }
        else
        {
            Debug.LogWarning("[FixSceneDefaults] GameOverPanel not found.");
        }

        if (fixed_count > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log($"[FixSceneDefaults] Done — {fixed_count} fix(es) applied. Scene saved.");
        }
        else
        {
            Debug.Log("[FixSceneDefaults] Nothing to fix.");
        }
    }
}
