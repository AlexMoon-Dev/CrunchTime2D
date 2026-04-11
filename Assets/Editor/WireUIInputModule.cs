using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class WireUIInputModule
{
    public static void Execute()
    {
        var eventSystemGO = GameObject.Find("EventSystem");
        if (eventSystemGO == null) { Debug.LogError("[WireUIInputModule] EventSystem not found!"); return; }

        var uiModule = eventSystemGO.GetComponent<InputSystemUIInputModule>();
        if (uiModule == null) { Debug.LogError("[WireUIInputModule] InputSystemUIInputModule not found!"); return; }

        var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/GameInputActions.inputactions");
        if (asset == null) { Debug.LogError("[WireUIInputModule] GameInputActions.inputactions not found!"); return; }

        var uiMap = asset.FindActionMap("UI");
        if (uiMap == null) { Debug.LogError("[WireUIInputModule] UI action map not found!"); return; }

        uiModule.actionsAsset = asset;
        uiModule.move   = InputActionReference.Create(uiMap.FindAction("Navigate"));
        uiModule.submit = InputActionReference.Create(uiMap.FindAction("Submit"));
        uiModule.cancel = InputActionReference.Create(uiMap.FindAction("Cancel"));

        // Wire mouse point and click from the Player action map's Aim/BasicAttack bindings
        // so the module keeps mouse-click support after being explicitly configured
        var playerMap = asset.FindActionMap("Player");
        if (playerMap != null)
        {
            uiModule.point     = InputActionReference.Create(playerMap.FindAction("Aim"));
            uiModule.leftClick = InputActionReference.Create(playerMap.FindAction("BasicAttack"));
        }

        EditorUtility.SetDirty(eventSystemGO);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log("[WireUIInputModule] InputSystemUIInputModule wired to GameInputActions UI map.");
    }
}
