# CrunchTime2D — Controls & How To Play

---

## Controls

### Player 1 — Keyboard + Mouse

| Action | Input | Notes |
|---|---|---|
| Move | `A` / `D` | Horizontal only. Hold to run at `moveSpeed` (default 6). |
| Jump | `Space` | Variable height — release early for a shorter jump. Coyote time: 0.1s after walking off a ledge. |
| Drop Through Platform | `S` (while on a one-way platform) | Temporarily swaps player to a non-colliding layer for 0.3s. |
| Dash | `Left Shift` | Dashes in your current move direction (or forward if standing still). Brief I-frames active during dash. Cooldown recharges on landing. |
| Basic Attack | `Left Mouse Button` | Short cooldown, class-dependent (see Classes below). |
| Heavy Attack | `Right Mouse Button` | Longer cooldown. **Ranger only:** hold to charge, release to fire — damage scales with charge time. |
| Aim (Ranger) | `Mouse Position` | Converted from screen space to world direction automatically. |
| Confirm (UI) | `Enter` | Confirm class/perk selection. |
| Navigate (UI) | `Arrow Keys` | Move between options. |
| Pause / Perk History | `Escape` / `Tab` | `Escape` pauses. `Tab` opens the collected perks overlay. |

---

### Player 2 — Gamepad

| Action | Input | Notes |
|---|---|---|
| Move | `Left Stick` | Horizontal axis drives movement. |
| Jump | `Button South` (A / Cross) | Same variable-height jump as P1. |
| Drop Through Platform | `D-Pad Down` | Same 0.3s layer-swap mechanic. |
| Dash | `Button West` (X / Square) | Same dash logic as P1. |
| Basic Attack | `Right Trigger` | |
| Heavy Attack | `Left Trigger` | **Ranger only:** hold to charge, release to fire. |
| Aim (Ranger) | `Right Stick` | Deadzone: must exceed 0.1 magnitude to register. |
| Confirm (UI) | `Button South` (A / Cross) | |
| Navigate (UI) | `Left Stick` | |
| Pause | `Start` | |

---

## How To Play

### 1. Class Selection (before Wave 1)

When you launch the game the class selection screen appears. Time is paused.

- **P1 (left side)** uses `Arrow Keys` + `Enter` or clicks with the mouse.
- **P2 (right side)** uses the gamepad `Left Stick` + `Button South`.

Each player picks one of three classes:

| Class | Playstyle | Basic Attack | Heavy Attack |
|---|---|---|---|
| **Tank** | High HP and armor, slow. Built to absorb damage for the team. | Wide arc melee sweep hitting all enemies around you. | Ground slam — AoE in front. With the **Bulwark** class perk: becomes a shield bash that scales damage with your armor value. |
| **Fighter** | Balanced brawler. Fast combo attacks and mobile. | 2-hit combo (3-hit with **Flurry** perk). Last hit of the combo deals bonus damage. | Forward lunge — moves you toward enemies and deals high damage to everything in the lunge path. |
| **Ranger** | Ranged. Fires projectiles in your aim direction. | Single projectile toward aim direction. | Charged shot — hold `RMB` / `Left Trigger` to charge (up to 2s), release to fire. Damage and projectile size scale with charge time. With **Arsenal** class perk: fires 3 projectiles in a spread instead. |

**Class lock rule:** The first player to confirm a class locks it. The other player's matching card grays out immediately. **Only one player can be Ranger.**

Once both players confirm, Wave 1 starts.

---

### 2. Surviving Waves

- Waves last **60 seconds** each. A timer counts down at the top of the screen.
- Enemies spawn continuously. Spawn rate and enemy strength increase each wave.
- Kill enemies to earn **XP**. XP is awarded individually to each living player.
- New enemy types unlock as waves progress:
  - Wave 1+: Runners (fast, low HP, charge at you)
  - Wave 2+: Shooters (keep distance, fire slow projectiles)
  - Wave 3+: Brutes (slow, high HP, heavy knockback on hit)
  - Wave 5+: Invokers (summon other enemies every 8s — kill these first)
- **Every 5 waves** a Boss spawns at the start of the wave. The Boss is a scaled-up Brute that also fires horizontal shockwaves across the platform it's standing on every 8 seconds. Defeating it grants bonus XP to both players.

---

### 3. Leveling Up

When a player earns enough XP to level up (threshold = `100 × level^1.2`), the game pauses and the perk selection screen appears.

- P1's three perk options appear on the **left half** of the screen.
- P2's three perk options appear on the **right half**.
- Each player picks one. Perks are weighted by class — a Tank will see more defensive options, a Ranger will see more projectile options.
- Once a player picks, their side shows **"Waiting..."** until the other player also picks.
- Both confirmed → perks are applied instantly → game resumes.

**Every 10 levels** the three cards are replaced with your class's two **Class Perks** instead (major mechanic modifiers — see the perk descriptions in-game).

Players level independently. If P1 levels up while P2 hasn't yet, the game still pauses for P1's pick. P2 will get their own pause when they hit their own threshold.

---

### 4. Platforms

The arena has four surfaces:

| Platform | Position | Type |
|---|---|---|
| Ground floor | Bottom, full width | Solid — cannot drop through |
| Left platform | Mid-height, left side | One-way — drop through with `S` / `D-Pad Down` |
| Right platform | Mid-height, right side | One-way — drop through |
| Top platform | Center, highest point | One-way — drop through |

To drop through a one-way platform: stand on it and press `S` (KB) or `D-Pad Down` (gamepad). You do **not** need to press Jump — just the down input is enough.

---

### 5. Death and Respawn

When a player's HP reaches 0:

- They are removed from the arena (ghost/gray visual on HUD).
- A respawn timer starts: **5 seconds + (3 seconds × number of times you've died)**.
- The HUD shows the countdown on the dead player's side.
- When the timer expires, the player respawns at the center of the arena at **50% HP**.

**Game Over condition:** If the second player dies while the first player is still on their respawn timer (i.e., both players are simultaneously dead or waiting), the game ends immediately.

---

### 6. Perk History (Pause Overlay)

At any time during a wave press `Tab` (keyboard) to open a full-screen overlay showing every perk each player has collected, organized by player side. Press `Tab` again to close it.

---

## Quick Reference Card

```
KB+Mouse                        Gamepad
────────────────────────────    ─────────────────────────────
A / D          Move             Left Stick      Move
Space          Jump             Button South    Jump
S              Drop Through     D-Pad Down      Drop Through
Left Shift     Dash             Button West     Dash
LMB            Basic Attack     Right Trigger   Basic Attack
RMB (hold)     Heavy Attack     Left Trigger    Heavy Attack
Mouse          Aim (Ranger)     Right Stick     Aim (Ranger)
Enter          UI Confirm       Button South    UI Confirm
Arrow Keys     UI Navigate      Left Stick      UI Navigate
Escape         Pause            Start           Pause
Tab            Perk History     —               —
```
