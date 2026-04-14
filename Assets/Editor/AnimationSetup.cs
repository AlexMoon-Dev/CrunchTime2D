#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// One-shot setup: creates all animation clips and animator controllers for every
/// entity in the game, fills in the empty Player clips, and wires Animator
/// components onto the enemy prefabs.
///
/// Run via  CrunchTime ▶ Setup All Animations
/// </summary>
public static class AnimationSetup
{
    [MenuItem("CrunchTime/Setup All Animations")]
    public static void SetupAllAnimations()
    {
        EnsureFolder("Assets/Animations/Female");
        EnsureFolder("Assets/Animations/Enemies");

        SetupMalePlayer();
        SetupFemalePlayer();
        SetupAlien();
        SetupDrone();
        SetupHydra();
        SetupMage();
        SetupMech();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CrunchTime] All animations set up successfully.");
    }

    // ── Generic helpers ───────────────────────────────────────────────────────

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        EnsureFolder(path[..slash]);
        AssetDatabase.CreateFolder(path[..slash], path[(slash + 1)..]);
    }

    static Sprite GetSprite(string path) =>
        AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();

    static Sprite[] GetNumbered(string folder, params int[] indices) =>
        indices
            .Select(i => GetSprite($"{folder}/sprite_{i:D2}.png"))
            .Where(s => s != null)
            .ToArray();

    static AnimationClip MakeClip(string name, Sprite[] sprites, float fps = 12f, bool loop = true)
    {
        var clip = new AnimationClip { name = name, frameRate = fps };
        var cfg  = AnimationUtility.GetAnimationClipSettings(clip);
        cfg.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, cfg);

        if (sprites != null && sprites.Length > 0)
        {
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keys    = sprites
                .Select((s, i) => new ObjectReferenceKeyframe { time = i / fps, value = s })
                .ToArray();
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        }
        return clip;
    }

    /// <summary>Updates an existing clip (preserving GUID) or creates a new asset.</summary>
    static AnimationClip SaveClip(AnimationClip clip, string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(clip, existing);
            EditorUtility.SetDirty(existing);
            return existing;
        }
        AssetDatabase.CreateAsset(clip, path);
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
    }

    // ── State machine helpers ─────────────────────────────────────────────────

    static AnimatorState AddState(AnimatorStateMachine sm, string name, AnimationClip clip)
    {
        var s = sm.AddState(name);
        s.motion = clip;
        return s;
    }

    static void AddSpeedTransitions(AnimatorState idle, AnimatorState move, float threshold = 0.1f)
    {
        var toMove = idle.AddTransition(move);
        toMove.AddCondition(AnimatorConditionMode.Greater, threshold, "Speed");
        toMove.duration = 0f; toMove.hasExitTime = false;

        var toIdle = move.AddTransition(idle);
        toIdle.AddCondition(AnimatorConditionMode.Less, threshold, "Speed");
        toIdle.duration = 0f; toIdle.hasExitTime = false;
    }

    static AnimatorStateTransition AnyTo(AnimatorStateMachine sm, AnimatorState dst, string trigger)
    {
        var t = sm.AddAnyStateTransition(dst);
        t.AddCondition(AnimatorConditionMode.If, 0, trigger);
        t.duration = 0f; t.hasExitTime = false; t.canTransitionToSelf = false;
        return t;
    }

    static void ExitTo(AnimatorState src, AnimatorState dst, float exitTime = 0.9f)
    {
        var t = src.AddTransition(dst);
        t.hasExitTime = true; t.exitTime = exitTime; t.duration = 0f;
    }

    // ── Player Male ───────────────────────────────────────────────────────────

    static void SetupMalePlayer()
    {
        const string P = "Assets/ART/Edits/male_char/poses";
        const string R = "Assets/ART/Edits/male_char/run";
        const string D = "Assets/Animations/Player";

        SaveClip(MakeClip("Player_Idle",
            new[] { GetSprite($"{P}/standing_idle.png") }, 8f),
            $"{D}/Player_Idle.anim");

        SaveClip(MakeClip("Player_Run",
            GetNumbered(R, 1,2,3,4,5,6,7,8), 12f),
            $"{D}/Player_Run.anim");

        SaveClip(MakeClip("Player_Jump",
            new[] { GetSprite($"{P}/right_profile.png") }, 12f, false),
            $"{D}/Player_Jump.anim");

        SaveClip(MakeClip("Player_Fall",
            new[] { GetSprite($"{P}/sneaking_side.png") }, 12f, false),
            $"{D}/Player_Fall.anim");

        SaveClip(MakeClip("Player_Attack",
            new[] { GetSprite($"{P}/throwing_side.png") }, 12f, false),
            $"{D}/Player_Attack.anim");

        SaveClip(MakeClip("Player_Hurt",
            new[] { GetSprite($"{P}/waving_front.png") }, 12f, false),
            $"{D}/Player_Hurt.anim");

        SaveClip(MakeClip("Player_Die",
            new[] { GetSprite($"{P}/back.png") }, 12f, false),
            $"{D}/Player_Die.anim");

        Debug.Log("[CrunchTime] Male player clips updated.");
    }

    // ── Player Female ─────────────────────────────────────────────────────────

    static void SetupFemalePlayer()
    {
        const string P = "Assets/ART/Edits/female_char/poses";
        const string R = "Assets/ART/Edits/female_char/run";
        const string D = "Assets/Animations/Female";

        // Female animation clips
        var idle   = SaveClip(MakeClip("Female_Idle",   new[] { GetSprite($"{P}/sprite_01.png") }, 8f),         $"{D}/Female_Idle.anim");
        var run    = SaveClip(MakeClip("Female_Run",    GetNumbered(R, 1,2,3,4,5,6), 12f),                     $"{D}/Female_Run.anim");
        var jump   = SaveClip(MakeClip("Female_Jump",   new[] { GetSprite($"{P}/sprite_03.png") }, 12f, false), $"{D}/Female_Jump.anim");
        var fall   = SaveClip(MakeClip("Female_Fall",   new[] { GetSprite($"{P}/sprite_06.png") }, 12f, false), $"{D}/Female_Fall.anim");
        var attack = SaveClip(MakeClip("Female_Attack", new[] { GetSprite($"{P}/sprite_05.png") }, 12f, false), $"{D}/Female_Attack.anim");
        var hurt   = SaveClip(MakeClip("Female_Hurt",   new[] { GetSprite($"{P}/sprite_07.png") }, 12f, false), $"{D}/Female_Hurt.anim");
        var die    = SaveClip(MakeClip("Female_Die",    new[] { GetSprite($"{P}/sprite_08.png") }, 12f, false), $"{D}/Female_Die.anim");

        // AnimatorOverrideController that swaps male clips → female clips
        var baseCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(
            "Assets/Animations/Player/PlayerAnimator.controller");
        if (baseCtrl == null)
        {
            Debug.LogWarning("[CrunchTime] PlayerAnimator.controller not found — skipping female override.");
            return;
        }

        var overridePath = $"{D}/PlayerAnimator_Female.overrideController";
        var overrideCtrl = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(overridePath);
        if (overrideCtrl == null)
        {
            overrideCtrl = new AnimatorOverrideController(baseCtrl);
            AssetDatabase.CreateAsset(overrideCtrl, overridePath);
            overrideCtrl = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(overridePath);
        }
        else
        {
            overrideCtrl.runtimeAnimatorController = baseCtrl;
        }

        // Build override map: male base clip → female clip
        var maleClips = new Dictionary<string, AnimationClip>
        {
            { "Player_Idle",   idle   },
            { "Player_Run",    run    },
            { "Player_Jump",   jump   },
            { "Player_Fall",   fall   },
            { "Player_Attack", attack },
            { "Player_Hurt",   hurt   },
            { "Player_Die",    die    },
        };

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideCtrl.GetOverrides(overrides);
        for (int i = 0; i < overrides.Count; i++)
        {
            var orig = overrides[i].Key;
            if (orig != null && maleClips.TryGetValue(orig.name, out var replacement))
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(orig, replacement);
        }
        overrideCtrl.ApplyOverrides(overrides);
        EditorUtility.SetDirty(overrideCtrl);

        Debug.Log("[CrunchTime] Female player clips + override controller ready.");
    }

    // ── Enemy Alien (Runner) ──────────────────────────────────────────────────

    static void SetupAlien()
    {
        EnsureFolder("Assets/Animations/Enemies/Alien");
        const string A = "Assets/ART/Edits/enemy_alien";
        const string D = "Assets/Animations/Enemies/Alien";

        var idleClip = SaveClip(MakeClip("Alien_Idle", GetNumbered(A, 12,13,14,15), 8f),              $"{D}/Alien_Idle.anim");
        var runClip  = SaveClip(MakeClip("Alien_Run",  GetNumbered(A, 1,2,3,4,5,6,7,8,9,10,11), 12f), $"{D}/Alien_Run.anim");
        var dieClip  = SaveClip(MakeClip("Alien_Die",  GetNumbered(A, 14,15), 10f, false),             $"{D}/Alien_Die.anim");

        var ctrl = BuildController($"{D}/AlienAnimator.controller",
            new[] { "Speed" }, null,
            new[] { "HurtTrigger", "DieTrigger" });

        var sm   = ctrl.layers[0].stateMachine;
        var idle = AddState(sm, "Idle", idleClip);
        var run  = AddState(sm, "Run",  runClip);
        var die  = AddState(sm, "Die",  dieClip);
        sm.defaultState = idle;

        AddSpeedTransitions(idle, run);
        AnyTo(sm, die, "DieTrigger");

        WirePrefab("Assets/Prefabs/Enemies/Runner.prefab", ctrl);
        Debug.Log("[CrunchTime] Alien set up.");
    }

    // ── Enemy Drone (Shooter) ─────────────────────────────────────────────────

    static void SetupDrone()
    {
        EnsureFolder("Assets/Animations/Enemies/Drone");
        const string A = "Assets/ART/Edits/enemy_drone";
        const string D = "Assets/Animations/Enemies/Drone";

        var idleClip  = SaveClip(MakeClip("Drone_Idle",  GetNumbered(A, 1,2,3,4),    8f),         $"{D}/Drone_Idle.anim");
        var shootClip = SaveClip(MakeClip("Drone_Shoot", GetNumbered(A, 5,6,7),       12f, false), $"{D}/Drone_Shoot.anim");
        var hurtClip  = SaveClip(MakeClip("Drone_Hurt",  GetNumbered(A, 8,9,10),      12f, false), $"{D}/Drone_Hurt.anim");
        var dieClip   = SaveClip(MakeClip("Drone_Die",   GetNumbered(A, 14,15,16),    10f, false), $"{D}/Drone_Die.anim");

        var ctrl = BuildController($"{D}/DroneAnimator.controller",
            null, new[] { "ShootTrigger" },
            new[] { "HurtTrigger", "DieTrigger" });

        var sm    = ctrl.layers[0].stateMachine;
        var idle  = AddState(sm, "Idle",  idleClip);
        var shoot = AddState(sm, "Shoot", shootClip);
        var hurt  = AddState(sm, "Hurt",  hurtClip);
        var die   = AddState(sm, "Die",   dieClip);
        sm.defaultState = idle;

        AnyTo(sm, shoot, "ShootTrigger");
        AnyTo(sm, hurt,  "HurtTrigger");
        AnyTo(sm, die,   "DieTrigger");
        ExitTo(shoot, idle);
        ExitTo(hurt,  idle);

        WirePrefab("Assets/Prefabs/Enemies/Shooter.prefab", ctrl);
        Debug.Log("[CrunchTime] Drone set up.");
    }

    // ── Enemy Hydra (Boss) ────────────────────────────────────────────────────

    static void SetupHydra()
    {
        EnsureFolder("Assets/Animations/Enemies/Hydra");
        const string A = "Assets/ART/Edits/enemy_hydra";
        const string D = "Assets/Animations/Enemies/Hydra";

        var idleClip = SaveClip(MakeClip("Hydra_Idle", GetNumbered(A, 1,2,3,4,5), 8f),         $"{D}/Hydra_Idle.anim");
        var walkClip = SaveClip(MakeClip("Hydra_Walk", GetNumbered(A, 6,7),        12f),         $"{D}/Hydra_Walk.anim");
        var slamClip = SaveClip(MakeClip("Hydra_Slam", GetNumbered(A, 8,9),        10f, false), $"{D}/Hydra_Slam.anim");
        var dieClip  = SaveClip(MakeClip("Hydra_Die",  GetNumbered(A, 8,9),        8f,  false), $"{D}/Hydra_Die.anim");

        var ctrl = BuildController($"{D}/HydraAnimator.controller",
            new[] { "Speed" }, new[] { "SlamTrigger" },
            new[] { "HurtTrigger", "DieTrigger" });

        var sm   = ctrl.layers[0].stateMachine;
        var idle = AddState(sm, "Idle", idleClip);
        var walk = AddState(sm, "Walk", walkClip);
        var slam = AddState(sm, "Slam", slamClip);
        var die  = AddState(sm, "Die",  dieClip);
        sm.defaultState = idle;

        AddSpeedTransitions(idle, walk);
        AnyTo(sm, slam, "SlamTrigger");
        AnyTo(sm, die,  "DieTrigger");
        ExitTo(slam, walk);

        WirePrefab("Assets/Prefabs/Enemies/Boss.prefab", ctrl);
        Debug.Log("[CrunchTime] Hydra set up.");
    }

    // ── Enemy Mage (Invoker) ──────────────────────────────────────────────────

    static void SetupMage()
    {
        EnsureFolder("Assets/Animations/Enemies/Mage");
        const string A = "Assets/ART/Edits/enemy_mage";
        const string D = "Assets/Animations/Enemies/Mage";

        var idleClip   = SaveClip(MakeClip("Mage_Idle",   GetNumbered(A, 1,2,3),           8f),         $"{D}/Mage_Idle.anim");
        var summonClip = SaveClip(MakeClip("Mage_Summon", GetNumbered(A, 4,5,6,7,10,11,12), 10f, false), $"{D}/Mage_Summon.anim");
        var hurtClip   = SaveClip(MakeClip("Mage_Hurt",   GetNumbered(A, 8,9),             12f, false), $"{D}/Mage_Hurt.anim");
        var dieClip    = SaveClip(MakeClip("Mage_Die",    GetNumbered(A, 13,14),            10f, false), $"{D}/Mage_Die.anim");

        var ctrl = BuildController($"{D}/MageAnimator.controller",
            new[] { "Speed" }, new[] { "SummonTrigger" },
            new[] { "HurtTrigger", "DieTrigger" });

        var sm     = ctrl.layers[0].stateMachine;
        var idle   = AddState(sm, "Idle",   idleClip);
        var summon = AddState(sm, "Summon", summonClip);
        var hurt   = AddState(sm, "Hurt",   hurtClip);
        var die    = AddState(sm, "Die",    dieClip);
        sm.defaultState = idle;

        AnyTo(sm, summon, "SummonTrigger");
        AnyTo(sm, hurt,   "HurtTrigger");
        AnyTo(sm, die,    "DieTrigger");
        ExitTo(summon, idle);
        ExitTo(hurt,   idle);

        WirePrefab("Assets/Prefabs/Enemies/Invoker.prefab", ctrl);
        Debug.Log("[CrunchTime] Mage set up.");
    }

    // ── Enemy Mech (Brute) ────────────────────────────────────────────────────

    static void SetupMech()
    {
        EnsureFolder("Assets/Animations/Enemies/Mech");
        const string A = "Assets/ART/Edits/enemy_mech";
        const string D = "Assets/Animations/Enemies/Mech";

        var idleClip   = SaveClip(MakeClip("Mech_Idle",   GetNumbered(A, 1,2),       8f),         $"{D}/Mech_Idle.anim");
        var walkClip   = SaveClip(MakeClip("Mech_Walk",   GetNumbered(A, 3,4,8,9),   10f),         $"{D}/Mech_Walk.anim");
        var attackClip = SaveClip(MakeClip("Mech_Attack", GetNumbered(A, 5,6,7),     10f, false), $"{D}/Mech_Attack.anim");
        var hurtClip   = SaveClip(MakeClip("Mech_Hurt",   GetNumbered(A, 10,11,12),  12f, false), $"{D}/Mech_Hurt.anim");
        var dieClip    = SaveClip(MakeClip("Mech_Die",    GetNumbered(A, 13,14),     10f, false), $"{D}/Mech_Die.anim");

        var ctrl = BuildController($"{D}/MechAnimator.controller",
            new[] { "Speed" }, new[] { "AttackTrigger" },
            new[] { "HurtTrigger", "DieTrigger" });

        var sm     = ctrl.layers[0].stateMachine;
        var idle   = AddState(sm, "Idle",   idleClip);
        var walk   = AddState(sm, "Walk",   walkClip);
        var attack = AddState(sm, "Attack", attackClip);
        var hurt   = AddState(sm, "Hurt",   hurtClip);
        var die    = AddState(sm, "Die",    dieClip);
        sm.defaultState = idle;

        AddSpeedTransitions(idle, walk);
        AnyTo(sm, attack, "AttackTrigger");
        AnyTo(sm, hurt,   "HurtTrigger");
        AnyTo(sm, die,    "DieTrigger");
        ExitTo(attack, walk);
        ExitTo(hurt,   idle);

        WirePrefab("Assets/Prefabs/Enemies/Brute.prefab", ctrl);
        Debug.Log("[CrunchTime] Mech set up.");
    }

    // ── Controller factory ────────────────────────────────────────────────────

    /// <summary>
    /// Creates a blank AnimatorController at <paramref name="path"/> (deletes existing).
    /// Adds float params for <paramref name="floatParams"/>,
    /// extra triggers for <paramref name="extraTriggers"/>,
    /// and always adds the triggers in <paramref name="baseTriggers"/>.
    /// Does NOT add states — caller does that.
    /// </summary>
    static AnimatorController BuildController(string path,
        string[] floatParams, string[] extraTriggers, string[] baseTriggers)
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
            AssetDatabase.DeleteAsset(path);

        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        foreach (var f in floatParams   ?? System.Array.Empty<string>())
            ctrl.AddParameter(f, AnimatorControllerParameterType.Float);
        foreach (var t in extraTriggers ?? System.Array.Empty<string>())
            ctrl.AddParameter(t, AnimatorControllerParameterType.Trigger);
        foreach (var t in baseTriggers  ?? System.Array.Empty<string>())
            ctrl.AddParameter(t, AnimatorControllerParameterType.Trigger);
        return ctrl;
    }

    // ── Prefab wiring ─────────────────────────────────────────────────────────

    /// <summary>
    /// Opens the enemy prefab, adds an Animator if missing, assigns the controller,
    /// switches SpriteRenderer to Simple draw mode, and resets the tint to white.
    /// </summary>
    static void WirePrefab(string prefabPath, RuntimeAnimatorController ctrl)
    {
        using var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath);
        var root = scope.prefabContentsRoot;

        // Animator
        var anim = root.GetComponent<Animator>();
        if (anim == null) anim = root.AddComponent<Animator>();
        anim.runtimeAnimatorController = ctrl;

        // SpriteRenderer — switch from Sliced placeholder to Simple pixel art
        var sr = root.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.drawMode = SpriteDrawMode.Simple;
            sr.color    = Color.white;
        }
    }
}
#endif
