# CrunchTime2D — Session README
**Engine:** Unity 6 (URP, 2D)  
**Repository:** [AlexMoon-Dev/CrunchTime2D](https://github.com/AlexMoon-Dev/CrunchTime2D)

---

## Session — 2026-04-15 | Menu System & Volume Settings

**Goal:** Build the start menu scene, in-game pause menu, and a 3-channel volume settings panel. Fix outstanding collider sizing on all enemy prefabs. Harden the editor tooling so individual steps can be run without clobbering existing work.

---

### New Scenes

| Scene | Path | Purpose |
|-------|------|---------|
| `MainMenu` | `Assets/Scenes/MainMenu.unity` | Start screen — full-screen background (`ART/1.jpg`), three invisible hit-area buttons over the baked-in button art (START / OPTIONS / EXIT), settings panel overlay |
| `GameScene` | `Assets/Scenes/GameScene.unity` | Main game (renamed from `SampleScene`). Now also contains `PauseMenuHandler` + `PauseMenuCanvas` |

Build Settings order: **MainMenu = 0 · GameScene = 1**

---

### New Scripts — `Assets/Scripts/UI/`

#### `VolumeSettings.cs`
- Singleton (`DontDestroyOnLoad`). Stores **General / SFX / Music** volumes as floats (0–1) in `PlayerPrefs` so they survive sessions.
- Exposes `SetMaster(float)`, `SetSFX(float)`, `SetMusic(float)`.
- **AudioMixer hookup is a 2-line uncomment** — add a public `AudioMixer` field, expose parameters `MasterVolume` / `SFXVolume` / `MusicVolume` (dB), uncomment the `SetFloat` call in `Save()`.

#### `VolumeSliderController.cs`
- `[RequireComponent(typeof(Slider))]`. Set `VolumeType` enum (`Master / SFX / Music`) in the Inspector.
- On first activation (`Start`): reads the saved value from `VolumeSettings.Instance` with `SetValueWithoutNotify` (no spurious save on open), then subscribes `onValueChanged`.

#### `SettingsPanelController.cs`
- Sits on the settings panel root. Provides `Open()` / `Close()` as persistent-listener-friendly methods so buttons can target the panel directly without needing a reference up to the parent controller.

#### `MainMenuController.cs`
- **START** → `SceneManager.LoadScene("GameScene")`
- **OPTIONS** → `settingsPanel.Open()`
- **EXIT** → `Application.Quit()`

#### `PauseMenuHandler.cs`
- Lives on an empty root `GameObject` in `GameScene`.
- **Esc key** (New Input System — `Keyboard.current.escapeKey.wasPressedThisFrame`) toggles pause, with two priority layers:
  1. If paused **and** settings panel is open → close settings only.
  2. Otherwise → toggle pause (allowed in any `GameState` except `GameOver`).
- Pause calls `GameManager.Instance.SetState(GameState.Paused)` — `GameManager` already handles `Time.timeScale`.
- Resume calls `GameManager.Instance.SetState(GameState.Wave)`.
- **BACK** → resume. **SETTINGS** → `settingsPanel.Open()`. **EXIT** → `CombatEventSystem.ClearAll()` + load `MainMenu`.

---

### New Editor Script — `Assets/Editor/MenuSetup.cs`

Programmatically builds both menu scenes. Available under **CrunchTime > Menus**:

| Menu item | What it does |
|-----------|-------------|
| **Setup All Menus** | Runs all three steps in sequence |
| **Setup Main Menu Scene** | (Re)creates `MainMenu.unity` from scratch — camera, EventSystem, Canvas, background `RawImage`, 3 transparent hit-area buttons, settings panel |
| **Inject Pause Menu into GameScene** | Opens `GameScene.unity` additively, adds `PauseMenuHandler` GO + `PauseMenuCanvas` (initially inactive, sort order 100) with background, 3 hit-area buttons, and settings panel. Skips if already present |
| **Configure Build Settings** | Sets `MainMenu(0)` / `GameScene(1)` |

**Hit-area button approach:** The background JPG is the complete visual — buttons are invisible `Image` components (`color.a = 0`, `raycastTarget = true`) anchored to match where the button art lives in the image. Adjust anchor handles in the Scene view if zones feel off.

**Settings panel layout:** Dark semi-transparent panel centred at 25–75% of screen width/height. Three rows (GENERAL · SFX · MUSIC), each with a label and a `Slider` + `VolumeSliderController`. CLOSE button at the bottom calls `SettingsPanelController.Close()`.

---

### `Assets/Editor/AnimationSetup.cs` — Collider Size Fix

All five `WirePrefab` calls now pass sprite-proportional `BoxCollider2D` sizes so a re-run of **Setup All Animations** produces correct hitboxes at scale (1, 1, 1):

| Enemy | Collider size |
|-------|--------------|
| Alien / Runner | 0.70 × 0.85 |
| Drone / Shooter | 0.45 × 0.55 |
| Hydra / Boss | 0.85 × 1.60 |
| Mage / Invoker | 0.55 × 1.15 |
| Mech / Brute | 0.75 × 1.15 |

---

### Scene Rename — `SampleScene` → `GameScene`

All string references updated:

| File | Change |
|------|--------|
| `Scripts/UI/MainMenuController.cs` | `LoadScene("SampleScene")` → `"GameScene"` |
| `Editor/MenuSetup.cs` | `GAME_SCENE` const path + all comments |
| `Scripts/UI/PauseMenuHandler.cs` | Doc comment |
| `ProjectSettings/EditorBuildSettings.asset` | Regenerated automatically by **Configure Build Settings** |

---

## Session — 2026-04-14 | Animation System

**Goal:** Replace all placeholder circles/squares with real pixel-art sprites and wire up the full animation state machine for every entity.

---

### New File: `Assets/Editor/AnimationSetup.cs`

One-shot editor script. Run via **CrunchTime → Setup All Animations** in the Unity menu bar.

**What it does:**

1. **Fills the 7 empty `Player_*.anim` clips** (they existed but had no keyframes) with male character sprites from `Assets/ART/Edits/male_char/`.
2. **Creates `Assets/Animations/Female/`** — seven `Female_*.anim` clips plus `PlayerAnimator_Female.overrideController` (an `AnimatorOverrideController` that shares the base `PlayerAnimator` state machine logic but substitutes female sprites for every clip).
3. **Creates `Assets/Animations/Enemies/{Alien|Drone|Hydra|Mage|Mech}/`** — animation clips and a full `AnimatorController` for each enemy type.
4. **Patches each enemy prefab** (`Runner`, `Shooter`, `Brute`, `Invoker`, `Boss`) via `PrefabUtility.EditPrefabContentsScope`: adds an `Animator` component if missing, assigns the correct controller, switches `SpriteRenderer.drawMode` from `Sliced` to `Simple`, and resets the tint color to white.

---

### Sprite-to-Animation Frame Mapping

Sprites live in `Assets/ART/Edits/<entity>/sprite_01.png … sprite_NN.png`.  
Frame ranges were determined by reviewing the original sheet PNG for each entity.

| Entity | Idle | Move | Special | Hurt | Die |
|--------|------|------|---------|------|-----|
| `enemy_alien` (Runner) | 12–15 | Run 1–11 | — | — | 14–15 |
| `enemy_drone` (Shooter) | 1–4 | — | Shoot 5–7 | 8–10 | 14–16 |
| `enemy_hydra` (Boss) | 1–5 | Walk 6–7 | Slam 8–9 | — | 8–9 |
| `enemy_mage` (Invoker) | 1–3 | — | Summon 4–7, 10–12 | 8–9 | 13–14 |
| `enemy_mech` (Brute) | 1–2 | Walk 3,4,8,9 | Attack 5–7 | 10–12 | 13–14 |
| `male_char` (P1) | `poses/standing_idle` | `run/sprite_01–08` | `poses/throwing_side` | `poses/waving_front` | `poses/back` |
| `female_char` (P2) | `poses/sprite_01` | `run/sprite_01–06` | `poses/sprite_05` | `poses/sprite_07` | `poses/sprite_08` |

---

### Animator State Machines

**Player (shared `PlayerAnimator.controller`)**  
States: `Idle` → `Run` → `Jump` → `Fall` → `Attack` → `Hurt` → `Die`  
Parameters: `Speed` (Float), `VerticalSpeed` (Float), `IsGrounded` (Bool), `AttackTrigger`, `HurtTrigger`, `DieTrigger` (Triggers)  
P1 uses `PlayerAnimator.controller` directly. P2 uses `PlayerAnimator_Female.overrideController`.

**Alien (Runner)**  
States: `Idle` ↔ `Run` (Speed), `Die` (AnyState trigger)  
Parameters: `Speed`, `HurtTrigger`, `DieTrigger`

**Drone (Shooter)**  
States: `Idle`, `Shoot`, `Hurt`, `Die` (all AnyState triggers; Shoot/Hurt exit back to Idle)  
Parameters: `ShootTrigger`, `HurtTrigger`, `DieTrigger`

**Hydra (Boss)**  
States: `Idle` ↔ `Walk` (Speed), `Slam` (AnyState trigger, exits to Walk), `Die`  
Parameters: `Speed`, `SlamTrigger`, `HurtTrigger`, `DieTrigger`

**Mage (Invoker)**  
States: `Idle`, `Summon`, `Hurt`, `Die` (AnyState triggers; Summon/Hurt exit to Idle)  
Parameters: `Speed`, `SummonTrigger`, `HurtTrigger`, `DieTrigger`

**Mech (Brute)**  
States: `Idle` ↔ `Walk` (Speed), `Attack`, `Hurt` (AnyState triggers; Attack exits to Walk, Hurt to Idle), `Die`  
Parameters: `Speed`, `AttackTrigger`, `HurtTrigger`, `DieTrigger`

---

### Modified Scripts

#### `Assets/Scripts/Enemies/EnemyBase.cs`
- Added `protected SpriteRenderer _sr` and `protected Animator _animator`, initialized in `Awake`.
- Added `public float deathAnimDuration = 0.6f` — delay between death trigger and `Destroy`.
- Added protected helpers: `AnimFloat(string, float)`, `AnimTrigger(string)`, `FaceTarget()`, `FaceVelocity(float)`.
- `TakeDamage`: fires `HurtTrigger` on non-lethal hits.
- `Die`: refactored into `AwardXP()` (virtual, overridable) + `DieTrigger` + `DestroyAfterDelay` coroutine. Stops velocity before destroy.
- **Breaking change:** `BossEnemy.Die()` override replaced with `BossEnemy.AwardXP()` override (same split XP logic, cleaner separation).

#### `Assets/Scripts/Enemies/RunnerEnemy.cs`
- `Behave`: calls `FaceVelocity(_rb.linearVelocity.x)` and `AnimFloat("Speed", ...)`.

#### `Assets/Scripts/Enemies/ShooterEnemy.cs`
- `Behave`: calls `FaceTarget()`.
- `FireAt`: calls `AnimTrigger("ShootTrigger")` before instantiating the projectile.

#### `Assets/Scripts/Enemies/BruteEnemy.cs`
- `Behave`: calls `FaceTarget()`, `AnimFloat("Speed", ...)`, and `AnimTrigger("AttackTrigger")` on attack.

#### `Assets/Scripts/Enemies/InvokerEnemy.cs`
- `Behave`: calls `FaceTarget()` and `AnimFloat("Speed", ...)`.
- `SummonRandom`: calls `AnimTrigger("SummonTrigger")` before instantiating the summoned enemy.

#### `Assets/Scripts/Enemies/BossEnemy.cs`
- `GroundSlam` coroutine: calls `AnimTrigger("SlamTrigger")` at start of the slam sequence.
- Replaced `Die()` override with `AwardXP()` override (death sequencing now handled by `EnemyBase.Die()`).

#### `Assets/Scripts/Player/PlayerController.cs`
- Added `private SpriteRenderer _sr`, initialized in `Awake`.
- `UpdateFacing`: replaced `transform.localScale = new Vector3(FacingDir, 1, 1)` with `_sr.flipX = FacingDir < 0`. Fixes child-object scale distortion that would occur with the old approach once real sprites replace the placeholder.

#### `Assets/Scripts/Player/PlayerSetup.cs`
- Added `public RuntimeAnimatorController maleController` and `femaleController` Inspector fields.
- `Awake`: applies the correct controller to the `Animator` component based on `playerIndex` (0 = male, 1 = female).
- Removed placeholder `sr.color = playerColor` tint — coloring is no longer needed once the real sprites are active.

---

### Post-Setup Inspector Steps (one-time, after running the menu item)

1. On **Player1** (`playerIndex = 0`) `PlayerSetup` component:
   - `Male Controller` → `Assets/Animations/Player/PlayerAnimator.controller`
   - `Female Controller` → `Assets/Animations/Female/PlayerAnimator_Female.overrideController`
2. On **Player2** (`playerIndex = 1`) `PlayerSetup` component — same fields, same assets.
3. Verify each enemy prefab has an `Animator` component with its controller assigned (should be automatic from the setup script).
4. Optionally disable `PlayerVisualFeedback` on player GameObjects once the `Hurt` animation is confirmed working — the component was a placeholder (red flash / green outline) and is no longer needed.

---

## Session — 2026-04-10 | Core Systems

**Session goal:** Build a barebones 2-player local co-op roguelike platformer loop from scratch.

---

## What was built this session

### Scripts (`Assets/Scripts/`)

| Folder | File | Purpose |
|---|---|---|
| `Combat/` | `DamageContext.cs` | Mutable damage payload passed through the event pipeline. Perks modify it here instead of touching attack logic directly. |
| `Combat/` | `CombatEventSystem.cs` | Static event bus. Perks subscribe here for runtime hooks (`OnBeforePlayerDamagesEnemy`, `OnAfterPlayerDamagesEnemy`, `OnPlayerKilledEnemy`, `OnPlayerDash`, `OnPlayerHit`). Call `ClearAll()` on scene reload. |
| `Combat/` | `Projectile.cs` | Player projectile. Reads `HasPerk("Ricochet")` and `HasPerk("Rapid Fire")` from `PlayerLeveling` to modify bounce/size behavior. |
| `Core/` | `GameManager.cs` | Singleton. Owns `GameState` enum (`ClassSelection`, `Wave`, `LevelUp`, `Paused`, `GameOver`). Sets `Time.timeScale` accordingly — only `Wave` runs at scale 1. |
| `Core/` | `WaveManager.cs` | Singleton. 60s waves, increasing difficulty (`1 + wave * 0.15`). Unlocks enemy types by wave number. Boss every 5 waves. Exposes `CurrentDifficulty` for use by `InvokerEnemy`. |
| `Core/` | `LevelUpManager.cs` | Subscribes to `PlayerLeveling.OnLevelUp`. When fired, pauses game, builds weighted perk offerings, waits for both players to confirm, then resumes. Every 10 levels offers class perks instead. |
| `Player/` | `PlayerStats.cs` | All runtime stats. Stat-change helpers (`AddArmor`, `MultiplyAttackDamage`, etc.) so perks don't set values directly. Fires `OnHealthChanged` and `OnDeath`. |
| `Player/` | `PlayerController.cs` | Movement, variable-height jump, coyote time (0.1s), double-jump support (set `maxJumps = 2`), dash with I-frames, drop-through via layer swap. Uses `PlayerInput` SendMessages. |
| `Player/` | `PlayerCombat.cs` | Class-switched attack logic (Tank/Fighter/Ranger). Routes damage through `CombatEventSystem`. Exposes `perkBulwark` and `perkArsenal` flags set by class perks. |
| `Player/` | `PlayerLeveling.cs` | XP tracking. Threshold = `100 * level^1.2`. Fires `OnLevelUp`. Tracks collected perks by name (for `HasPerk()` checks) and by reference (for history panel). |
| `Player/` | `PlayerRespawnHandler.cs` | On death: starts timer (`5s + deathCount * 3s`), disables player, checks if all players dead → Game Over. Respawns at 50% HP at `respawnPoint`. |
| `Player/` | `PlayerSetup.cs` | Glue component. Sets `playerIndex`, forces correct control scheme on `PlayerInput`. Applies the correct `RuntimeAnimatorController` (`maleController` / `femaleController`) at `Awake` based on `playerIndex`. |
| `Classes/` | `ClassDefinitionSO.cs` | ScriptableObject. Holds base stat deltas applied on class selection, class name/description, and list of class perks (offered every 10 levels). |
| `Classes/` | `ClassManager.cs` | Singleton. Enforces class-lock via static events `OnClassConfirmed` and `OnClassLocked`. Ranger is the only single-pick class. On both confirmed: applies stats, calls `GameManager.StartWave()`. |
| `Perks/` | `PerkSO.cs` | Base ScriptableObject. `Apply(PlayerStats)` for one-time stat changes. `Equip(PlayerLeveling)` for subscribing to `CombatEventSystem` events. `Unequip()` for cleanup. |
| `Perks/` | `PerkDatabase.cs` | Holds all `PerkSO` assets. `GetWeightedSelection(classType, owned, count)` returns a weighted random pool excluding already-owned perks. |
| `Enemies/` | `EnemyBase.cs` | Base enemy. Awards XP on death via overridable `AwardXP()`. `ApplyDifficultyMultiplier()` scales stats. `ApplyStun()` coroutine. Damages player on `OnCollisionStay2D`. Drives `Animator` + `SpriteRenderer` facing via `FaceTarget()` / `FaceVelocity()`. |
| `Enemies/` | `RunnerEnemy.cs` | Charges at nearest player. Drives `Speed` animator param and sprite facing. |
| `Enemies/` | `ShooterEnemy.cs` | Maintains distance, fires `EnemyProjectile`. Fires `ShootTrigger` on the animator each shot. Faces target. |
| `Enemies/` | `BruteEnemy.cs` | Slow, high HP/damage, heavy knockback. Drives `Speed` + `AttackTrigger` animator params. Faces target. |
| `Enemies/` | `InvokerEnemy.cs` | Flees from players. Summons a random enemy every 8s. Fires `SummonTrigger` on summon. Drives `Speed` + facing. |
| `Enemies/` | `BossEnemy.cs` | 5x Brute HP. Ground slam every 8s fires `SlamTrigger` then spawns `Shockwave`. `AwardXP()` override splits bonus XP. |
| `Enemies/` | `Shockwave.cs` | Travels horizontally on a Rigidbody2D, damages players on trigger. |
| `Enemies/` | `EnemyProjectile.cs` | Simple trigger projectile that deals damage to `PlayerStats` on contact. |
| `Arena/` | `ArenaBuilder.cs` | Builds the entire arena at runtime using colored 1-pixel sprites + `BoxCollider2D`. Ground floor + 3 one-way platforms. Sets camera orthographic size. Places spawn points on `WaveManager`. |
| `Arena/` | `OneWayPlatform.cs` | Companion to `PlatformEffector2D`. Drop-through works by `PlayerController` swapping player to the `DropThrough` layer for 0.3s. **Requires Physics 2D layer matrix: `DropThrough ↔ Platform` = unchecked.** |
| `UI/` | `HUDController.cs` | Owns both `PlayerHUDElement` refs and wave texts. Subscribes to `WaveManager` and `PlayerRespawnHandler` events. |
| `UI/` | `PlayerHUDElement.cs` | Per-player HUD section. Subscribes to `PlayerStats.OnHealthChanged` and `PlayerLeveling.OnLevelUp`. Drives HP/XP sliders and level text. |
| `UI/` | `LevelUpUIController.cs` | Full-screen overlay. P1 cards left, P2 right. Delegates confirmation to `LevelUpManager`. |
| `UI/` | `ClassSelectionUI.cs` | Shown at game start (inside `LevelUpPanel`). Wires to `ClassManager` static events for real-time lock feedback. |
| `UI/` | `ClassCardUI.cs` | Single class selection card with locked overlay. |
| `UI/` | `PerkCardUI.cs` | Single perk card with name, description, SELECT button, locked overlay. |
| `UI/` | `PerkHistoryPanel.cs` | Toggle with **Tab**. Reads `PlayerLeveling.CollectedPerks` and builds entries. |
| `UI/` | `GameOverUI.cs` | Listens for `GameState.GameOver`, shows wave number, wires Restart button to `GameManager.RestartGame()`. |

---

### Data Assets (`Assets/Data/`)

- `PerkDatabase.asset` — populated with all 36 perk instances
- `Classes/Tank.asset`, `Fighter.asset`, `Ranger.asset` — with stat deltas and class perk references
- `Perks/Neutral/` — 6 perks (Vitality, Plating, Brute Force, Swiftness, Edge, Quickstep)
- `Perks/Tank/` — 8 perks + 2 class perks (Bulwark, Fortress)
- `Perks/Fighter/` — 8 perks + 2 class perks (Momentum, Adaptable)
- `Perks/Ranger/` — 8 perks + 2 class perks (Predator, Arsenal)

### Prefabs (`Assets/Prefabs/`)

- `Enemies/` — Runner, Shooter, Brute, Invoker, Boss. Each has `SpriteRenderer` + `BoxCollider2D` + `Rigidbody2D`. After running **CrunchTime → Setup All Animations**, each also gets an `Animator` with its entity-specific controller assigned.
- `Projectiles/` — PlayerProjectile, EnemyProjectile, Shockwave
- `UI/PerkCard.prefab` — reusable perk card used by `LevelUpUIController`

### Input (`Assets/GameInputActions.inputactions`)

Custom input asset replacing the Unity default. Two control schemes:
- `KeyboardMouse` — WASD, Space, LShift, LMB, RMB, Mouse position
- `Gamepad` — Left stick, South, West, RT, LT, Right stick

---

## Scene Hierarchy (SampleScene)

```
Main Camera
Global Light 2D
GameManager           ← GameManager, WaveManager, ClassManager, LevelUpManager
ArenaBuilder          ← builds geometry at runtime
Player1               ← blue, playerIndex=0, KB+Mouse
  GroundCheck
  ProjectileSpawn
Player2               ← orange-red, playerIndex=1, Gamepad
  GroundCheck
  ProjectileSpawn
RespawnPoint          ← both players respawn here (center, y=-2)
UICanvas
  HUD
    P1_HUD            ← PlayerHUDElement, HP/XP sliders, LevelText, RespawnPanel
    P2_HUD            ← same
    WavePanel         ← WaveNumber, WaveTimer texts
  LevelUpPanel        ← active at game start (hosts class selection)
    ClassSelectionUI  ← ClassSelectionUI script, 3 cards per player
    P1_Cards / P2_Cards
    P1_Status / P2_Status
  GameOverPanel       ← inactive, activated by GameOverUI on state change
  PerkHistoryPanel    ← inactive, toggled by Tab key (PerkHistoryPanel script on UICanvas)
```

---

## Known Issues / Left To Fix Next Session

### Why the game doesn't respond to clicks right now
The most likely cause is a **missing EventSystem** in the scene. Unity UI buttons require an `EventSystem` GameObject to process mouse/controller input. It should be a child of the UICanvas or a sibling. To fix:
- In the Hierarchy, right-click → **UI → Event System**
- Or: select the UICanvas, then in the menu **GameObject → UI → Event System**

Other possible causes:
1. **PlayerInput actions not saved** — The `set_property` MCP call for the `.inputactions` asset failed during setup. Verify `Player1` and `Player2` have `GameInputActions.inputactions` assigned in their `PlayerInput` component.
2. **Canvas not set to Screen Space Overlay** — check the `UICanvas` Canvas component's Render Mode.
3. **`Time.timeScale = 0`** — `GameState.ClassSelection` pauses time. This is intentional and does not affect UI EventSystem, but double-check that `GameManager.SetState` is being called correctly on `Start()`.

### TODOs carried over / animation follow-ups
- Run **CrunchTime → Setup All Animations** once on project open to generate all animation assets
- Drag `PlayerAnimator.controller` → `PlayerSetup.maleController` and `PlayerAnimator_Female.overrideController` → `PlayerSetup.femaleController` on both player GameObjects
- `PlayerVisualFeedback` (red hit flash, green attack outline) can be disabled once `Hurt` / `Attack` animations are confirmed working
- Death animations play for `deathAnimDuration` (0.6s) before destroy — tune per enemy in the Inspector
- The Alien and Hydra have no dedicated hurt frames; `HurtTrigger` is still raised in code but has no Hurt state in their controllers — add one if needed

### Other TODOs for next session
- Add `PerkHistoryEntry` prefab (currently the script exists but there's no prefab assigned to `PerkHistoryPanel.entryPrefab`)
- Wire `ClassSelectionUI` into the `LevelUpUIController.classSelectionUI` field
- Assign `PlayerHUDElement.classIcon` and `PlayerHUDElement.className` display after class is picked (hook into `ClassManager.OnClassConfirmed`)
- Add a `PauseMenuController` that handles Escape mid-wave (currently Escape is bound as a `Pause` action but no pause menu UI exists)
- Add `ArmorBar` (secondary slider on top of HP bar) to `PlayerHUDElement` — the field exists, no visual is wired
- MultiShot perk counter not yet implemented in `PlayerCombat` (the perk SO exists and registers by name, but `PlayerCombat` doesn't check for it yet)
- Physics 2D layer collision matrix must have `DropThrough ↔ Platform` unchecked for drop-through to work
- All `// TODO: assign art` and `// TODO: replace with real VFX` comments mark swap-out points for future art pass

---

## Architecture Notes

### Damage pipeline
```
PlayerCombat.DoXAttack()
  → creates DamageContext(baseDamage, type, source)
  → CombatEventSystem.RaiseBeforePlayerDamage()   ← perks modify ctx here
  → ctx.Resolve()                                  ← finalDamage = base * multiplier
  → EnemyBase.TakeDamage(ctx)
  → CombatEventSystem.RaiseAfterPlayerDamage()     ← perks react to confirmed hit
  → if killed: CombatEventSystem.RaisePlayerKilledEnemy()
```

### Perk hook pattern
Perks never modify attack logic directly. They subscribe lambdas to `CombatEventSystem` events in their `Equip()` method. Because these are anonymous lambdas, `CombatEventSystem.ClearAll()` is called on game restart to prevent stale subscriptions from leaking across scenes.

### Drop-through platforms
`PlayerController.OnDropThrough` sets a flag. When the flag is active and Jump is pressed, a coroutine swaps the player to the `DropThrough` layer for 0.3s. `PlatformEffector2D` ignores that layer (configured in Physics 2D matrix). After 0.3s the player reverts to the `Player` layer and collides normally again.
