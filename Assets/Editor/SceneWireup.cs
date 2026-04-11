using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// One-shot editor script that wires all scene references, creates placeholder
/// enemy/projectile prefabs, and builds the UI hierarchy.
/// Run via MCP execute_script, then delete this file.
/// </summary>
public static class SceneWireup
{
    public static void Execute()
    {
        EnsureLayers();
        WirePlayers();
        CreatePlaceholderPrefabs();
        CreateUI();
        WireManagers();
        CreatePerkDatabase();
        CreateClassDefinitions();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[SceneWireup] Done — all references wired.");
    }

    // ── Layers ────────────────────────────────────────────────────────────────

    static void EnsureLayers()
    {
        // Add layers: Player, Enemy, Ground, Platform, DropThrough
        // Using SerializedObject to edit TagManager
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tagManager.FindProperty("layers");

        string[] needed = { "Player", "Enemy", "Ground", "Platform", "DropThrough" };
        foreach (var name in needed)
        {
            bool exists = false;
            for (int i = 8; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == name) { exists = true; break; }
            }
            if (!exists)
            {
                for (int i = 8; i < layers.arraySize; i++)
                {
                    var el = layers.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(el.stringValue)) { el.stringValue = name; break; }
                }
            }
        }
        tagManager.ApplyModifiedProperties();

        // Load input actions asset and assign to both PlayerInput components
        var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
            "Assets/GameInputActions.inputactions");

        if (inputAsset != null)
        {
            AssignInputActions("Player1", inputAsset, "KeyboardMouse");
            AssignInputActions("Player2", inputAsset, "Gamepad");
        }
        else
        {
            Debug.LogWarning("[SceneWireup] GameInputActions.inputactions not found.");
        }
    }

    static void AssignInputActions(string playerName, InputActionAsset asset, string scheme)
    {
        var go = GameObject.Find(playerName);
        if (go == null) return;
        var pi = go.GetComponent<PlayerInput>();
        if (pi == null) return;

        var so = new SerializedObject(pi);
        so.FindProperty("m_Actions").objectReferenceValue = asset;
        so.FindProperty("m_DefaultControlScheme").stringValue = scheme;
        so.FindProperty("m_DefaultActionMap").stringValue = "Player";
        so.FindProperty("m_NotificationBehavior").intValue = 2; // SendMessages
        so.ApplyModifiedProperties();
    }

    // ── Players ───────────────────────────────────────────────────────────────

    static void WirePlayers()
    {
        WirePlayer("Player1", 0);
        WirePlayer("Player2", 1);

        var respawnPoint = GameObject.Find("RespawnPoint");

        // Set Player layer
        int playerLayer = LayerMask.NameToLayer("Player");

        foreach (var name in new[] { "Player1", "Player2" })
        {
            var go = GameObject.Find(name);
            if (go == null) continue;
            if (playerLayer >= 0) go.layer = playerLayer;

            // BoxCollider2D size
            var col = go.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(0.9f, 1.8f);

            // Rigidbody interpolation
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null) rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // SpriteRenderer — use a 1x1 white pixel as placeholder
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = CreateWhiteSprite();
                sr.drawMode = SpriteDrawMode.Sliced;
                var so = new SerializedObject(sr);
                so.FindProperty("m_Size").vector2Value = new Vector2(0.9f, 1.8f);
                so.ApplyModifiedProperties();
            }

            // Respawn point
            var rh = go.GetComponent<PlayerRespawnHandler>();
            if (rh != null && respawnPoint != null)
                rh.respawnPoint = respawnPoint.transform;
        }
    }

    static void WirePlayer(string name, int index)
    {
        var go = GameObject.Find(name);
        if (go == null) return;

        var groundCheck = go.transform.Find("GroundCheck");
        var projSpawn   = go.transform.Find("ProjectileSpawn");

        // PlayerController
        var ctrl = go.GetComponent<PlayerController>();
        if (ctrl != null && groundCheck != null)
        {
            var so = new SerializedObject(ctrl);
            so.FindProperty("groundCheck").objectReferenceValue = groundCheck;
            so.FindProperty("groundLayers").intValue = LayerMask.GetMask("Ground", "Platform");
            so.ApplyModifiedProperties();
        }

        // PlayerCombat
        var combat = go.GetComponent<PlayerCombat>();
        if (combat != null)
        {
            var so = new SerializedObject(combat);
            so.FindProperty("enemyLayers").intValue = LayerMask.GetMask("Enemy");
            if (projSpawn != null)
                so.FindProperty("projectileSpawn").objectReferenceValue = projSpawn;
            so.ApplyModifiedProperties();
        }

        // PlayerStats
        var stats = go.GetComponent<PlayerStats>();
        if (stats != null) stats.playerIndex = index;
    }

    // ── Placeholder Prefabs ───────────────────────────────────────────────────

    static void CreatePlaceholderPrefabs()
    {
        Directory.CreateDirectory("Assets/Prefabs/Enemies");
        Directory.CreateDirectory("Assets/Prefabs/Projectiles");

        CreateEnemyPrefab("Runner",   new Color(1f,  0.3f, 0.3f), new Vector2(0.8f, 0.8f), typeof(RunnerEnemy));
        CreateEnemyPrefab("Shooter",  new Color(1f,  0.7f, 0.1f), new Vector2(0.8f, 0.9f), typeof(ShooterEnemy));
        CreateEnemyPrefab("Brute",    new Color(0.6f,0.1f, 0.1f), new Vector2(1.2f, 1.4f), typeof(BruteEnemy));
        CreateEnemyPrefab("Invoker",  new Color(0.7f,0.3f, 1.0f), new Vector2(0.8f, 1.1f), typeof(InvokerEnemy));
        CreateEnemyPrefab("Boss",     new Color(0.3f,0.0f, 0.0f), new Vector2(2.0f, 2.2f), typeof(BossEnemy));

        CreateProjectilePrefab("PlayerProjectile", new Color(1f, 1f, 0f), typeof(Projectile));
        CreateProjectilePrefab("EnemyProjectile",  new Color(1f, 0.3f, 0f), typeof(EnemyProjectile));
        CreateProjectilePrefab("Shockwave",        new Color(1f, 0.6f, 0.1f), typeof(Shockwave));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Wire prefabs into WaveManager and ShooterEnemy/InvokerEnemy/BossEnemy
        WireWaveManagerPrefabs();
    }

    static void CreateEnemyPrefab(string enemyName, Color color, Vector2 size, System.Type enemyScript)
    {
        string path = $"Assets/Prefabs/Enemies/{enemyName}.prefab";
        if (File.Exists(path)) return;

        var go = new GameObject(enemyName);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite  = CreateWhiteSprite();
        sr.color   = color;
        sr.drawMode = SpriteDrawMode.Sliced;
        var srSo = new SerializedObject(sr);
        srSo.FindProperty("m_Size").vector2Value = size;
        srSo.ApplyModifiedProperties();

        var col  = go.AddComponent<BoxCollider2D>();
        col.size = size;

        var rb  = go.AddComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        go.AddComponent(enemyScript);

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0) go.layer = enemyLayer;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        // Wire shooter's projectile prefab after creation
        if (enemyName == "Shooter")
        {
            var shooterComp = prefab.GetComponent<ShooterEnemy>();
            if (shooterComp != null)
            {
                var so = new SerializedObject(shooterComp);
                var ep = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectiles/EnemyProjectile.prefab");
                so.FindProperty("projectilePrefab").objectReferenceValue = ep;
                so.ApplyModifiedProperties();
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }
    }

    static void CreateProjectilePrefab(string projName, Color color, System.Type script)
    {
        string path = $"Assets/Prefabs/Projectiles/{projName}.prefab";
        if (File.Exists(path)) return;

        var go = new GameObject(projName);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSprite();
        sr.color  = color;
        sr.drawMode = SpriteDrawMode.Sliced;
        var srSo = new SerializedObject(sr);
        srSo.FindProperty("m_Size").vector2Value = new Vector2(0.3f, 0.15f);
        srSo.ApplyModifiedProperties();

        var col = go.AddComponent<BoxCollider2D>();
        col.size     = new Vector2(0.3f, 0.15f);
        col.isTrigger = true;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        go.AddComponent(script);

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    static void WireWaveManagerPrefabs()
    {
        var wm = Object.FindFirstObjectByType<WaveManager>();
        if (wm == null) return;

        var so = new SerializedObject(wm);
        so.FindProperty("runnerPrefab").objectReferenceValue  = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Runner.prefab");
        so.FindProperty("shooterPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Shooter.prefab");
        so.FindProperty("brutePrefab").objectReferenceValue   = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Brute.prefab");
        so.FindProperty("invokerPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Invoker.prefab");
        so.FindProperty("bossPrefab").objectReferenceValue    = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Boss.prefab");
        so.ApplyModifiedProperties();

        // Wire invoker's summon prefabs
        var invokerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Invoker.prefab");
        if (invokerPrefab != null)
        {
            var inv = invokerPrefab.GetComponent<InvokerEnemy>();
            if (inv != null)
            {
                var iso = new SerializedObject(inv);
                iso.FindProperty("runnerPrefab").objectReferenceValue  = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Runner.prefab");
                iso.FindProperty("shooterPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Shooter.prefab");
                iso.FindProperty("brutePrefab").objectReferenceValue   = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Brute.prefab");
                iso.ApplyModifiedProperties();
                PrefabUtility.SavePrefabAsset(invokerPrefab);
            }
        }

        // Wire shockwave prefab on boss
        var bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Boss.prefab");
        if (bossPrefab != null)
        {
            var boss = bossPrefab.GetComponent<BossEnemy>();
            if (boss != null)
            {
                var bso = new SerializedObject(boss);
                bso.FindProperty("shockwavePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectiles/Shockwave.prefab");
                bso.ApplyModifiedProperties();
                PrefabUtility.SavePrefabAsset(bossPrefab);
            }
        }

        // Wire player projectile into PlayerCombat
        var playerProjPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectiles/PlayerProjectile.prefab");
        foreach (var name in new[] { "Player1", "Player2" })
        {
            var go = GameObject.Find(name);
            if (go == null) continue;
            var combat = go.GetComponent<PlayerCombat>();
            if (combat == null) continue;
            var cso = new SerializedObject(combat);
            cso.FindProperty("projectilePrefab").objectReferenceValue = playerProjPrefab;
            cso.ApplyModifiedProperties();
        }
    }

    // ── UI ────────────────────────────────────────────────────────────────────

    static void CreateUI()
    {
        Directory.CreateDirectory("Assets/Prefabs/UI");

        // Main Canvas
        var canvasGo = new GameObject("UICanvas");
        var canvas   = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // HUD root
        var hud = MakeUIEmpty("HUD", canvasGo.transform);

        // P1 HUD — left half
        var p1Hud = MakeUIEmpty("P1_HUD", hud.transform);
        SetAnchors(p1Hud, new Vector2(0, 0), new Vector2(0.5f, 0.15f), Vector2.zero);

        // P2 HUD — right half
        var p2Hud = MakeUIEmpty("P2_HUD", hud.transform);
        SetAnchors(p2Hud, new Vector2(0.5f, 0), new Vector2(1f, 0.15f), Vector2.zero);

        // HP bars
        MakeSlider("HP_Bar", p1Hud.transform, new Color(0.2f, 0.8f, 0.2f));
        MakeSlider("HP_Bar", p2Hud.transform, new Color(0.2f, 0.8f, 0.2f));
        MakeSlider("XP_Bar", p1Hud.transform, new Color(0.2f, 0.4f, 1.0f));
        MakeSlider("XP_Bar", p2Hud.transform, new Color(0.2f, 0.4f, 1.0f));

        MakeText("LevelText", p1Hud.transform, "Lv.1", 24);
        MakeText("LevelText", p2Hud.transform, "Lv.1", 24);

        // Respawn panels
        var rp1 = MakeUIEmpty("RespawnPanel", p1Hud.transform);
        rp1.SetActive(false);
        MakeText("RespawnTimer", rp1.transform, "5s", 36);

        var rp2 = MakeUIEmpty("RespawnPanel", p2Hud.transform);
        rp2.SetActive(false);
        MakeText("RespawnTimer", rp2.transform, "5s", 36);

        // Wave info — top center
        var wavePanel = MakeUIEmpty("WavePanel", hud.transform);
        SetAnchors(wavePanel, new Vector2(0.35f, 0.92f), new Vector2(0.65f, 1f), Vector2.zero);
        MakeText("WaveNumber", wavePanel.transform, "Wave 1", 32);
        MakeText("WaveTimer",  wavePanel.transform, "60s",    24);

        // Level-up overlay
        var luPanel = MakeUIEmpty("LevelUpPanel", canvasGo.transform);
        SetAnchors(luPanel, Vector2.zero, Vector2.one, Vector2.zero);
        var bg = luPanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        luPanel.SetActive(false);

        var luScript = luPanel.AddComponent<LevelUpUIController>();

        // P1 card container — left half
        var p1Cards = MakeUIEmpty("P1_Cards", luPanel.transform);
        SetAnchors(p1Cards, new Vector2(0.02f, 0.1f), new Vector2(0.48f, 0.9f), Vector2.zero);
        var p1LayoutGroup = p1Cards.AddComponent<HorizontalLayoutGroup>();
        p1LayoutGroup.spacing = 10f;
        p1LayoutGroup.childForceExpandWidth = true;
        p1LayoutGroup.childForceExpandHeight = true;

        MakeText("P1_Status", luPanel.transform, "Choose a perk", 28);

        // P2 card container — right half
        var p2Cards = MakeUIEmpty("P2_Cards", luPanel.transform);
        SetAnchors(p2Cards, new Vector2(0.52f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero);
        var p2LayoutGroup = p2Cards.AddComponent<HorizontalLayoutGroup>();
        p2LayoutGroup.spacing = 10f;
        p2LayoutGroup.childForceExpandWidth = true;
        p2LayoutGroup.childForceExpandHeight = true;

        MakeText("P2_Status", luPanel.transform, "Choose a perk", 28);

        // Wire LevelUpUIController
        var luSo = new SerializedObject(luScript);
        luSo.FindProperty("panel").objectReferenceValue         = luPanel;
        luSo.FindProperty("p1CardContainer").objectReferenceValue = p1Cards.transform;
        luSo.FindProperty("p2CardContainer").objectReferenceValue = p2Cards.transform;
        luSo.FindProperty("p1StatusText").objectReferenceValue    = p1Cards.transform.parent.Find("P1_Status")?.GetComponent<TextMeshProUGUI>();
        luSo.FindProperty("p2StatusText").objectReferenceValue    = p2Cards.transform.parent.Find("P2_Status")?.GetComponent<TextMeshProUGUI>();
        luSo.ApplyModifiedProperties();

        // Perk card prefab — a simple button card
        var perkCardPrefab = CreatePerkCardPrefab();
        var perkCardSo = new SerializedObject(luScript);
        perkCardSo.FindProperty("perkCardPrefab").objectReferenceValue = perkCardPrefab;
        perkCardSo.ApplyModifiedProperties();

        // Class Selection overlay (child of LevelUpPanel)
        var csGo = MakeUIEmpty("ClassSelectionUI", luPanel.transform);
        SetAnchors(csGo, Vector2.zero, Vector2.one, Vector2.zero);
        csGo.AddComponent<ClassSelectionUI>();
        csGo.SetActive(false);

        // Game Over panel
        var goPanel = MakeUIEmpty("GameOverPanel", canvasGo.transform);
        SetAnchors(goPanel, Vector2.zero, Vector2.one, Vector2.zero);
        var goBg = goPanel.AddComponent<Image>();
        goBg.color = new Color(0, 0, 0, 0.9f);
        goPanel.SetActive(false);

        var goUI    = goPanel.AddComponent<GameOverUI>();
        var waveText = MakeText("WaveReached", goPanel.transform, "Survived to Wave 0", 48);
        var restartBtn = MakeButton("RestartButton", goPanel.transform, "RESTART");

        var goSo = new SerializedObject(goUI);
        goSo.FindProperty("panel").objectReferenceValue          = goPanel;
        goSo.FindProperty("waveReachedText").objectReferenceValue = waveText.GetComponent<TextMeshProUGUI>();
        goSo.FindProperty("restartButton").objectReferenceValue   = restartBtn.GetComponent<Button>();
        goSo.ApplyModifiedProperties();

        // Perk History overlay
        var histPanel = MakeUIEmpty("PerkHistoryPanel", canvasGo.transform);
        SetAnchors(histPanel, Vector2.zero, Vector2.one, Vector2.zero);
        var histBg = histPanel.AddComponent<Image>();
        histBg.color = new Color(0, 0, 0, 0.85f);
        histPanel.SetActive(false);

        var histScript = canvasGo.AddComponent<PerkHistoryPanel>();
        var histSo = new SerializedObject(histScript);
        histSo.FindProperty("overlay").objectReferenceValue    = histPanel;
        histSo.FindProperty("p1PerkList").objectReferenceValue = MakeUIEmpty("P1_PerkList", histPanel.transform).transform;
        histSo.FindProperty("p2PerkList").objectReferenceValue = MakeUIEmpty("P2_PerkList", histPanel.transform).transform;
        histSo.ApplyModifiedProperties();

        // HUDController
        var hudScript = hud.AddComponent<HUDController>();
        // (Player HUD elements wired separately as they need the PlayerHUDElement components)

        // Add GameOverUI component to canvas
        canvasGo.AddComponent<GameOverUI>();

        AssetDatabase.SaveAssets();
    }

    static PerkCardUI CreatePerkCardPrefab()
    {
        string path = "Assets/Prefabs/UI/PerkCard.prefab";
        if (File.Exists(path))
            return AssetDatabase.LoadAssetAtPath<GameObject>(path)?.GetComponent<PerkCardUI>();

        var go = new GameObject("PerkCard");
        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 8f;
        layout.childForceExpandWidth  = true;
        layout.childForceExpandHeight = false;

        MakeText("PerkName",        go.transform, "Perk Name",   22);
        MakeText("PerkDescription", go.transform, "Description", 16);

        var btnGo = MakeButton("SelectButton", go.transform, "SELECT");
        var card  = go.AddComponent<PerkCardUI>();

        var so = new SerializedObject(card);
        so.FindProperty("perkName").objectReferenceValue        = go.transform.Find("PerkName")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("perkDescription").objectReferenceValue = go.transform.Find("PerkDescription")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("selectButton").objectReferenceValue    = btnGo.GetComponent<Button>();
        so.ApplyModifiedProperties();

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab.GetComponent<PerkCardUI>();
    }

    // ── Managers wiring ───────────────────────────────────────────────────────

    static void WireManagers()
    {
        // GameManager players array
        var gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            var so = new SerializedObject(gm);
            var arr = so.FindProperty("players");
            arr.arraySize = 2;
            arr.GetArrayElementAtIndex(0).objectReferenceValue = GameObject.Find("Player1")?.GetComponent<PlayerStats>();
            arr.GetArrayElementAtIndex(1).objectReferenceValue = GameObject.Find("Player2")?.GetComponent<PlayerStats>();
            so.ApplyModifiedProperties();
        }

        // LevelUpManager
        var lum = Object.FindFirstObjectByType<LevelUpManager>();
        if (lum != null)
        {
            var so = new SerializedObject(lum);
            var luUI = Object.FindFirstObjectByType<LevelUpUIController>();
            so.FindProperty("levelUpUI").objectReferenceValue = luUI;

            // Wire perkDatabase if it exists
            var db = AssetDatabase.LoadAssetAtPath<PerkDatabase>("Assets/Data/PerkDatabase.asset");
            if (db != null)
                so.FindProperty("perkDatabase").objectReferenceValue = db;
            so.ApplyModifiedProperties();
        }
    }

    // ── ScriptableObjects ─────────────────────────────────────────────────────

    static void CreatePerkDatabase()
    {
        Directory.CreateDirectory("Assets/Data/Perks");
        string dbPath = "Assets/Data/PerkDatabase.asset";
        if (!File.Exists(dbPath))
        {
            var db = ScriptableObject.CreateInstance<PerkDatabase>();
            AssetDatabase.CreateAsset(db, dbPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[SceneWireup] PerkDatabase created. Drag all PerkSO assets into it, or call db.LoadFromResources() at runtime.");
        }
    }

    static void CreateClassDefinitions()
    {
        Directory.CreateDirectory("Assets/Data/Classes");

        CreateClass("Tank",    ClassType.Tank,
            "Tough frontliner. High armor and HP, slow melee.",
            bonusHealth: 30, bonusArmor: 10, bonusDmg: 2, bonusSpeed: -0.5f);

        CreateClass("Fighter", ClassType.Fighter,
            "Balanced brawler. Fast combos and high mobility.",
            bonusHealth: 10, bonusArmor: 2, bonusDmg: 5, bonusSpeed: 0.5f, bonusAtk: 0.2f);

        CreateClass("Ranger",  ClassType.Ranger,
            "Ranged attacker. Projects force from a distance.",
            bonusHealth: 0, bonusArmor: 0, bonusDmg: 8, bonusSpeed: 0.3f, bonusCrit: 0.05f);

        AssetDatabase.SaveAssets();
    }

    static void CreateClass(string name, ClassType type, string desc,
        float bonusHealth = 0, float bonusArmor = 0, float bonusDmg = 0,
        float bonusSpeed = 0, float bonusAtk = 0, float bonusCrit = 0)
    {
        string path = $"Assets/Data/Classes/{name}.asset";
        if (File.Exists(path)) return;

        var def = ScriptableObject.CreateInstance<ClassDefinitionSO>();
        def.classType           = type;
        def.className           = name;
        def.description         = desc;
        def.bonusMaxHealth      = bonusHealth;
        def.bonusArmor          = bonusArmor;
        def.bonusAttackDamage   = bonusDmg;
        def.bonusMoveSpeed      = bonusSpeed;
        def.bonusAttackSpeed    = bonusAtk;
        def.bonusCritChance     = bonusCrit;
        AssetDatabase.CreateAsset(def, path);
    }

    // ── UI helpers ────────────────────────────────────────────────────────────

    static GameObject MakeUIEmpty(string name, Transform parent)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static void SetAnchors(GameObject go, Vector2 min, Vector2 max, Vector2 pivot)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.pivot     = pivot == Vector2.zero ? new Vector2(0.5f, 0.5f) : pivot;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static GameObject MakeSlider(string name, Transform parent, Color fillColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 24);

        var slider = go.AddComponent<Slider>();
        slider.value = 1f;
        slider.minValue = 0f;
        slider.maxValue = 1f;

        // Background
        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(go.transform, false);
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f);
        var bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        slider.targetGraphic = bgImg;

        // Fill area
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var faRt = fillArea.AddComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero; faRt.anchorMax = Vector2.one;
        faRt.offsetMin = faRt.offsetMax = Vector2.zero;

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = fillColor;
        var fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;

        slider.fillRect = fillRt;
        return go;
    }

    static GameObject MakeText(string name, Transform parent, string text, int fontSize)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 50);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        return go;
    }

    static GameObject MakeButton(string name, Transform parent, string label)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 50);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.6f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        MakeText("Label", go.transform, label, 20);
        return go;
    }

    static Sprite CreateWhiteSprite()
    {
        var tex = new Texture2D(4, 4);
        var pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f,
            0, SpriteMeshType.FullRect, Vector4.one);
    }
}
#endif
