# CrunchTime2D — Session README
**Date:** 2026-04-10  
**Engine:** Unity 6 (URP, 2D)  
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
| `Player/` | `PlayerSetup.cs` | Glue component. Sets `playerIndex`, tints sprite, forces correct control scheme on `PlayerInput` at `Start()`. |
| `Classes/` | `ClassDefinitionSO.cs` | ScriptableObject. Holds base stat deltas applied on class selection, class name/description, and list of class perks (offered every 10 levels). |
| `Classes/` | `ClassManager.cs` | Singleton. Enforces class-lock via static events `OnClassConfirmed` and `OnClassLocked`. Ranger is the only single-pick class. On both confirmed: applies stats, calls `GameManager.StartWave()`. |
| `Perks/` | `PerkSO.cs` | Base ScriptableObject. `Apply(PlayerStats)` for one-time stat changes. `Equip(PlayerLeveling)` for subscribing to `CombatEventSystem` events. `Unequip()` for cleanup. |
| `Perks/` | `PerkDatabase.cs` | Holds all `PerkSO` assets. `GetWeightedSelection(classType, owned, count)` returns a weighted random pool excluding already-owned perks. |
| `Enemies/` | `EnemyBase.cs` | Base enemy. Awards XP on death. `ApplyDifficultyMultiplier()` scales stats. `ApplyStun()` coroutine. Damages player on `OnCollisionStay2D`. |
| `Enemies/` | `RunnerEnemy.cs` | Charges at nearest player. Fast, low HP. |
| `Enemies/` | `ShooterEnemy.cs` | Maintains distance, fires `EnemyProjectile` at a configurable fire rate. |
| `Enemies/` | `BruteEnemy.cs` | Slow, high HP/damage, heavy knockback. |
| `Enemies/` | `InvokerEnemy.cs` | Flees from players. Summons a random enemy every 8s. Uses `WaveManager.CurrentDifficulty` on spawned enemies. |
| `Enemies/` | `BossEnemy.cs` | 5x Brute HP. Ground slam every 8s that spawns horizontal `Shockwave` in both directions. Splits bonus XP on death. |
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

- `Enemies/` — Runner, Shooter, Brute, Invoker, Boss (all colored placeholder sprites)
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
