#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// One-shot editor tool: CrunchTime ▶ Setup Menu Scenes
///
/// • Creates Assets/Scenes/MainMenu.unity with a full-screen background,
///   three transparent hit-area buttons (START / OPTIONS / EXIT), and a
///   settings panel (General / SFX / Music sliders).
///
/// • Injects a PauseMenuCanvas + PauseMenuHandler into GameScene
///   with three hit-area buttons (BACK / EXIT / SETTINGS) and the
///   same style settings panel.
///
/// • Adds both scenes to Build Settings (MainMenu = 0, GameScene = 1).
///
/// Button anchor positions are tuned to match Assets/ART/1.jpg and 2.jpg.
/// Tweak them in the Inspector if the clickable zones feel off after resizing.
/// </summary>
public static class MenuSetup
{
    const string MAIN_MENU_SCENE = "Assets/Scenes/MainMenu.unity";
    const string GAME_SCENE      = "Assets/Scenes/GameScene.unity";
    const string BG_START        = "Assets/ART/1.jpg";
    const string BG_PAUSE        = "Assets/ART/2.jpg";

    [MenuItem("CrunchTime/Setup Menu Scenes")]
    public static void SetupMenuScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        BuildMainMenuScene();
        InjectPauseMenuIntoGameScene();
        ConfigureBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CrunchTime] Menu scenes ready. Open MainMenu.unity to verify.");
    }

    // ── Main Menu scene ───────────────────────────────────────────────────────

    static void BuildMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SetActiveScene(scene);

        CreateMainCamera();
        CreateEventSystem();

        // VolumeSettings singleton — persists into GameScene via DontDestroyOnLoad
        new GameObject("VolumeSettings").AddComponent<VolumeSettings>();

        // Canvas
        var canvasGO = CreateCanvas(sortOrder: 0);

        // Full-screen background image (the art IS the button art — buttons are invisible overlays)
        CreateBackground(canvasGO.transform, BG_START);

        // Settings panel (hidden by default)
        var settingsPanel = BuildSettingsPanel(canvasGO.transform);
        settingsPanel.SetActive(false);
        var panelCtrl = settingsPanel.GetComponent<SettingsPanelController>();

        // Controller
        var ctrlGO = new GameObject("MainMenuController");
        ctrlGO.transform.SetParent(canvasGO.transform, false);
        var ctrl = ctrlGO.AddComponent<MainMenuController>();

        var ctrlSO = new SerializedObject(ctrl);
        ctrlSO.FindProperty("settingsPanel").objectReferenceValue = panelCtrl;
        ctrlSO.ApplyModifiedProperties();

        // Transparent hit-area buttons — anchors match button positions in 1.jpg
        // START  (left button)
        var startBtn = CreateHitArea(canvasGO.transform, "StartButton",
            new Vector2(0.07f, 0.08f), new Vector2(0.30f, 0.20f));
        UnityEventTools.AddPersistentListener(startBtn.onClick, ctrl.OnClickStart);

        // OPTIONS (centre button)
        var optionsBtn = CreateHitArea(canvasGO.transform, "OptionsButton",
            new Vector2(0.37f, 0.08f), new Vector2(0.63f, 0.20f));
        UnityEventTools.AddPersistentListener(optionsBtn.onClick, ctrl.OnClickOptions);

        // EXIT (right button)
        var exitBtn = CreateHitArea(canvasGO.transform, "ExitButton",
            new Vector2(0.70f, 0.08f), new Vector2(0.93f, 0.20f));
        UnityEventTools.AddPersistentListener(exitBtn.onClick, ctrl.OnClickExit);

        EditorSceneManager.SaveScene(scene, MAIN_MENU_SCENE);
        Debug.Log("[CrunchTime] MainMenu.unity saved.");
    }

    // ── Pause menu injection into GameScene ─────────────────────────────────

    static void InjectPauseMenuIntoGameScene()
    {
        var scene = EditorSceneManager.OpenScene(GAME_SCENE, OpenSceneMode.Additive);
        EditorSceneManager.SetActiveScene(scene);

        // Guard against re-running
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == "PauseMenuHandler")
            {
                Debug.Log("[CrunchTime] PauseMenuHandler already exists in GameScene — skipping injection.");
                EditorSceneManager.CloseScene(scene, false);
                return;
            }
        }

        // VolumeSettings in GameScene too — singleton handles duplicates if coming from MainMenu
        new GameObject("VolumeSettings").AddComponent<VolumeSettings>();

        // Pause canvas — starts inactive; PauseMenuHandler activates it on Esc
        var canvasGO = CreateCanvas(sortOrder: 100);
        canvasGO.name = "PauseMenuCanvas";
        canvasGO.SetActive(false);

        CreateBackground(canvasGO.transform, BG_PAUSE);

        var settingsPanel = BuildSettingsPanel(canvasGO.transform);
        settingsPanel.SetActive(false);
        var panelCtrl = settingsPanel.GetComponent<SettingsPanelController>();

        // PauseMenuHandler — empty root GameObject
        var handlerGO = new GameObject("PauseMenuHandler");
        var handler   = handlerGO.AddComponent<PauseMenuHandler>();

        var handlerSO = new SerializedObject(handler);
        handlerSO.FindProperty("pauseCanvas").objectReferenceValue    = canvasGO;
        handlerSO.FindProperty("settingsPanel").objectReferenceValue  = panelCtrl;
        handlerSO.ApplyModifiedProperties();

        // Transparent hit-area buttons — anchors match button positions in 2.jpg
        // BACK (left)
        var backBtn = CreateHitArea(canvasGO.transform, "BackButton",
            new Vector2(0.07f, 0.04f), new Vector2(0.30f, 0.17f));
        UnityEventTools.AddPersistentListener(backBtn.onClick, handler.OnClickBack);

        // EXIT (centre)
        var exitBtn = CreateHitArea(canvasGO.transform, "ExitButton",
            new Vector2(0.37f, 0.04f), new Vector2(0.63f, 0.17f));
        UnityEventTools.AddPersistentListener(exitBtn.onClick, handler.OnClickExit);

        // SETTINGS (right)
        var settingsBtn = CreateHitArea(canvasGO.transform, "SettingsButton",
            new Vector2(0.67f, 0.04f), new Vector2(0.93f, 0.17f));
        UnityEventTools.AddPersistentListener(settingsBtn.onClick, handler.OnClickSettings);

        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.CloseScene(scene, false);
        Debug.Log("[CrunchTime] Pause menu injected into GameScene.");
    }

    // ── Settings panel ────────────────────────────────────────────────────────

    static GameObject BuildSettingsPanel(Transform parent)
    {
        // Root — centred, covers middle half of the screen
        var panelGO = new GameObject("SettingsPanel", typeof(RectTransform));
        panelGO.transform.SetParent(parent, false);
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.25f, 0.25f);
        panelRT.anchorMax = new Vector2(0.75f, 0.80f);
        panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

        // Dark background
        var bgGO = CreateChildImage(panelGO.transform, "PanelBG");
        bgGO.GetComponent<Image>().color = new Color(0.04f, 0.04f, 0.10f, 0.93f);

        // Title
        var title = CreateLabel(panelGO.transform, "SETTINGS", 36);
        SetAnchors(title, 0.1f, 0.82f, 0.9f, 0.96f);

        // Volume rows (label left 28 %, slider right 67 %)
        BuildVolumeRow(panelGO.transform, "GENERAL", VolumeSliderController.VolumeType.Master, 0.60f, 0.74f);
        BuildVolumeRow(panelGO.transform, "SFX",     VolumeSliderController.VolumeType.SFX,    0.40f, 0.54f);
        BuildVolumeRow(panelGO.transform, "MUSIC",   VolumeSliderController.VolumeType.Music,  0.20f, 0.34f);

        // Close button
        var closeBtnGO = CreateStyledButton(panelGO.transform, "CloseButton", "CLOSE");
        SetAnchors(closeBtnGO, 0.35f, 0.04f, 0.65f, 0.14f);

        var panelCtrl = panelGO.AddComponent<SettingsPanelController>();
        UnityEventTools.AddPersistentListener(
            closeBtnGO.GetComponent<Button>().onClick, panelCtrl.Close);

        return panelGO;
    }

    static void BuildVolumeRow(Transform parent, string labelText,
        VolumeSliderController.VolumeType type, float yMin, float yMax)
    {
        var rowGO = new GameObject($"{labelText}_Row", typeof(RectTransform));
        rowGO.transform.SetParent(parent, false);
        SetAnchors(rowGO, 0.05f, yMin, 0.95f, yMax);

        // Label
        var label = CreateLabel(rowGO.transform, labelText, 20);
        SetAnchors(label.gameObject, 0f, 0f, 0.28f, 1f);
        label.alignment = TextAlignmentOptions.MidlineLeft;

        // Slider
        var slider = BuildSlider(rowGO.transform);
        SetAnchors(slider.gameObject, 0.31f, 0.1f, 1f, 0.9f);

        // VolumeSliderController — set enum via SerializedObject so it survives domain reload
        var volCtrl = slider.gameObject.AddComponent<VolumeSliderController>();
        var so      = new SerializedObject(volCtrl);
        so.FindProperty("volumeType").enumValueIndex = (int)type;
        so.ApplyModifiedProperties();
    }

    // ── Low-level UI builders ─────────────────────────────────────────────────

    static void CreateMainCamera()
    {
        var go  = new GameObject("Main Camera");
        go.tag  = "MainCamera";
        var cam = go.AddComponent<Camera>();
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic    = true;
    }

    static void CreateEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        go.AddComponent<InputSystemUIInputModule>();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
    }

    static GameObject CreateCanvas(int sortOrder)
    {
        var go     = new GameObject("Canvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    static void CreateBackground(Transform parent, string texPath)
    {
        var go  = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        go.transform.SetParent(parent, false);
        var rt  = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<RawImage>().texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        go.transform.SetAsFirstSibling();
    }

    /// <summary>Invisible Image that registers clicks — sits over a baked-in button in the background art.</summary>
    static Button CreateHitArea(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go  = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        SetAnchors(go, anchorMin.x, anchorMin.y, anchorMax.x, anchorMax.y);
        var img = go.GetComponent<Image>();
        img.color          = new Color(0f, 0f, 0f, 0f); // fully transparent
        img.raycastTarget  = true;                        // still catches clicks
        return go.GetComponent<Button>();
    }

    static Slider BuildSlider(Transform parent)
    {
        var root   = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        root.transform.SetParent(parent, false);

        // Track background
        var bg   = CreateChildImage(root.transform, "Background");
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0f, 0.25f);
        bgRT.anchorMax = new Vector2(1f, 0.75f);
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.22f, 1f);

        // Fill area
        var fillArea   = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(root.transform, false);
        var fillAreaRT = fillArea.GetComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRT.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRT.offsetMin = new Vector2(5f,   0f);
        fillAreaRT.offsetMax = new Vector2(-15f, 0f);

        var fill   = CreateChildImage(fillArea.transform, "Fill");
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin  = Vector2.zero;
        fillRT.anchorMax  = new Vector2(1f, 1f);
        fillRT.sizeDelta  = new Vector2(10f, 0f);
        fill.GetComponent<Image>().color = new Color(0.22f, 0.60f, 1f, 1f);

        // Handle area
        var handleArea   = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(root.transform, false);
        var handleAreaRT = handleArea.GetComponent<RectTransform>();
        handleAreaRT.anchorMin = Vector2.zero;
        handleAreaRT.anchorMax = Vector2.one;
        handleAreaRT.offsetMin = new Vector2(10f,  0f);
        handleAreaRT.offsetMax = new Vector2(-10f, 0f);

        var handle   = CreateChildImage(handleArea.transform, "Handle");
        var handleRT = handle.GetComponent<RectTransform>();
        handleRT.anchorMin = handleRT.anchorMax = new Vector2(0f, 0.5f);
        handleRT.sizeDelta = new Vector2(20f, 20f);
        handleRT.offsetMin = handleRT.offsetMax = Vector2.zero;
        handle.GetComponent<Image>().color = Color.white;

        // Wire Slider component
        var slider         = root.GetComponent<Slider>();
        slider.fillRect    = fillRT;
        slider.handleRect  = handleRT;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction   = Slider.Direction.LeftToRight;
        slider.minValue    = 0f;
        slider.maxValue    = 1f;
        slider.value       = 1f;

        return slider;
    }

    static GameObject CreateStyledButton(Transform parent, string name, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(0.10f, 0.28f, 0.55f, 0.92f);

        var textGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(go.transform, false);
        var rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;

        return go;
    }

    static TextMeshProUGUI CreateLabel(Transform parent, string text, int fontSize)
    {
        var go = new GameObject($"{text}_Label",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp       = go.GetComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        return tmp;
    }

    static GameObject CreateChildImage(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    /// <summary>Sets all four anchors and zeroes out offsets so the rect exactly fills the anchor area.</summary>
    static void SetAnchors(GameObject go, float xMin, float yMin, float xMax, float yMax)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void SetAnchors(TextMeshProUGUI tmp, float xMin, float yMin, float xMax, float yMax)
        => SetAnchors(tmp.gameObject, xMin, yMin, xMax, yMax);

    // ── Build Settings ────────────────────────────────────────────────────────

    static void ConfigureBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MAIN_MENU_SCENE, true),  // index 0
            new EditorBuildSettingsScene(GAME_SCENE,      true),  // index 1
        };
        Debug.Log("[CrunchTime] Build Settings: MainMenu(0), GameScene(1).");
    }
}
#endif
