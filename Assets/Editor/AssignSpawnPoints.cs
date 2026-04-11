using UnityEngine;
using UnityEditor;

public class AssignSpawnPoints
{
    public static void Execute()
    {
        var gameManagerGO = GameObject.Find("GameManager");
        if (gameManagerGO == null) { Debug.LogError("GameManager not found"); return; }

        var waveManager = gameManagerGO.GetComponent<WaveManager>();
        if (waveManager == null) { Debug.LogError("WaveManager not found on GameManager"); return; }

        var names = new[] { "SpawnPoint_Left", "SpawnPoint_Right", "SpawnPoint_TopLeft", "SpawnPoint_TopRight" };
        var points = new Transform[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            var go = GameObject.Find(names[i]);
            if (go == null) { Debug.LogError($"Spawn point '{names[i]}' not found"); return; }
            points[i] = go.transform;
        }

        waveManager.spawnPoints = points;
        EditorUtility.SetDirty(gameManagerGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameManagerGO.scene);
        Debug.Log("[AssignSpawnPoints] Assigned 4 spawn points to WaveManager.");
    }
}
