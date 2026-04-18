# Findings — Library Review from Building Sample Games

Original snapshot: 2026-04-18. Updated 2026-04-18 (a) — four non-deferred §6 items landed in commit `702dd54`; their findings below are annotated as **Fixed**. Updated 2026-04-18 (b) — BattleGrid expanded from a tactical-grid stub into a real game with chip selection, enemy AI patterns, and a full HUD. Updated 2026-04-18 (c) — **Shooter** sample added (twin-stick arena survival); first real load test for `ObjectPool`, `TimerManager`, and `Camera2D.ScreenToWorld`. Updated 2026-04-18 (d) — **Puzzle** sample added (match-3 cascade); first real consumer of `TileMap` + `TileLayer<T>`. Surfaced a SpriteFont charset footgun (§1.10). Updated 2026-04-18 (e) — **Roguelike** sample added (turn-based dungeon crawler with procedural generation); second `TileMap` consumer + hand-rolled turn scheduling. Updated 2026-04-18 (f) — **TowerDefense** sample added; second `ObjectPool` consumer and first mouse-driven grid-cell placement interaction.

## Context

The framework has been exercised by five sample games in deliberately different genres:

1. **`MonoGame.GameFramework.BattleGrid`** — real-time grid duel inspired by Mega Man Battle Network. 3×3 grid per side, WASD movement, space-bar buster, Tab-triggered chip selection from a pool of four (Cannon / Wide Shot / Sword / Recov), enemy AI alternating between movement and two attack patterns, HP bars + controls hint HUD.
2. **`MonoGame.GameFramework.Platformer`** — side-scrolling platformer with gravity, AABB collision, camera follow, a patrolling enemy, goal + win state, and a title screen.
3. **`MonoGame.GameFramework.Shooter`** — top-down twin-stick arena survival. Free WASD movement in a 2400×1600 arena, mouse-aim firing with a 0.15 s cooldown, pursuit-AI enemies spawning in pairs every 1.5 s, score + HP HUD, game-over + R-to-restart.
4. **`MonoGame.GameFramework.Puzzle`** — match-3 gem board. 7×7 grid of colored gems, click two adjacent cells to swap, matches of 3+ in a row or column clear with gravity-pack and refill cascades, score counter, R to reshuffle.
5. **`MonoGame.GameFramework.Roguelike`** — turn-based dungeon crawler. Procedurally-generated rooms-and-corridors maps (60×34 `TileLayer<TileKind>`), bump-to-attack combat on a grid, monsters take a turn after every player action, stairs descend to a freshly generated deeper level. No FOV or inventory in MVP.

All five are playable end-to-end. The library is exercised by 5 distinct consumers across 5 distinct genres; the unused-surface analysis below draws on that full set.

---

## Library surface coverage

**Validated by all five games:**
- `Input.KeyboardManager`
- `Rendering.Primitives` — universal pattern for colored rectangles
- `Rendering.SpriteSheet.Static` + `SpriteSheet.Tint` — title-screen buttons in all five
- `Lifecycle.GameState` + `Lifecycle.GameStateManager` (auto-lifecycle)
- `UI.UIManager` — hit-testing + `OnClick` + `HoveredElement` for title buttons

**Validated by four games:**
- `Input.MouseManager` — all except Roguelike (keyboard-only by design)

**Validated by three games (3 of 5):**
- `Rendering.DrawManager` — BattleGrid + Platformer use it for world draws; Puzzle, Shooter, Roguelike bypass and draw directly. Now a minority pattern — only 2 of 5 games. `DrawManager` is optional, not essential.
- `Rendering.TileMap` + `TileLayer<T>` — Puzzle (7×7 gems), Roguelike (60×34 dungeon), and conceptually fits BattleGrid's hand-rolled grids. Fully validated.

**Validated by two games:**
- `Rendering.Camera2D` — Platformer (side-scrolling follow) + Shooter (twin-stick follow + `ScreenToWorld` for mouse aim).

**Validated by one game:**
- Shooter: `Pooling.ObjectPool<T>` (both Projectile and Enemy), `Timing.TimerManager.Every`, `Camera2D.ScreenToWorld`
- Puzzle: `TileMap.WorldToCell` (mouse-pick → grid cell)
- Roguelike: `TileLayer.Fill`, `TileMap.GetCellRect` for rendering at scale, `TileLayer<TileKind>` with an enum cell type. Also hand-rolled a tiny `TurnScheduler` pattern inline (see §1.11).
- BattleGrid: `Events.EventManager` (string API), `Lifecycle.SceneManager`, `Persistence.SettingsManager`, `Text.TextManager` (tilde-console log), `Input.GamePadManager`
- Platformer: `Camera2D.FollowLerp` with snap-on-respawn

**Not used by any game (still speculative after 5):**
- `Audio.SoundManager` — not a single game has played a sound
- `Content.AssetCatalog` — no game has >1 content asset
- `Debugging.ILogger` / `ConsoleLogger` / `PerformanceMonitor`
- `Persistence.SaveSystem` / `SaveFile<T>`
- `Tween.Tween<T>` / `Easing`
- `Utilities.MathUtilities`
- `Events.EventManager` typed API (`Subscribe<T>`/`Publish<T>`)
- `Timing.Timer` (raw class)
- `Core.Entity` — only BattleGrid still subscribes; Platformer, Shooter, Puzzle, Roguelike all skip it.
- `SpriteSheet.Animated` — five games, zero consumers.

After five games the unused set is ~10 primitives, stable across the last two data points. That set is the basis for §8's delete/keep recommendations.

---

## §1 — Shared patterns across both games (library candidates)

These would be added to the core library, not a genre module, because both games (and likely any future game) hit them.

### 1.1 `GameStateManager` lifecycle hook calls are manual
Both games push/change states and then call `Entered()` (and sometimes `Leaving()`) explicitly. The library's `PushState`/`PopState`/`ChangeState` only manipulate the stack — they don't fire lifecycle methods. Every consumer reinvents the same two-line ceremony:

```csharp
_gameStateManager.PeekState().Leaving();
_gameStateManager.ChangeState(nextState);
_gameStateManager.PeekState().Entered();
```

**Recommendation**: `GameStateManager.PushState(state)` should call `state.Entered()`. `PopState()` should call `Leaving()` on the popped state and `Revealed()` on the new top. `ChangeState` should call both. Consumers keep the option to call them manually for exotic flows, but the default should Just Work.

> **✅ Fixed 2026-04-18 (commit `702dd54`)**. `Push/Pop/ChangeState` now auto-fire `Entered`/`Leaving`/`Obscuring`/`Revealed`. Initial push on an empty stack fires `Entered` only (no `Revealed` — nothing to reveal from). Demo's `BattleState` migrated its `Revealed` debug-state push into `Entered`. Platformer's Play-click callback collapsed from four lines to one. Covered by 5 new `GameStateManagerTests`.

### 1.2 Pixel texture for colored rectangles
Both games need a 1×1 white texture for drawing colored rectangles. Platformer uses it ubiquitously (player, platforms, enemies, goal, overlays); tactical could use it but relies on pre-made sprites. Every game creates it the same way:

```csharp
_pixel = new Texture2D(GraphicsDevice, 1, 1);
_pixel.SetData(new[] { Color.White });
```

**Recommendation**: `Rendering.Primitives` or similar — a lazy-initialized `PixelTexture` service keyed to the current `GraphicsDevice`, plus convenience `DrawRectangle`/`DrawLine` extension methods on `SpriteBatch`. Low effort, eliminates copy-paste.

> **✅ Fixed 2026-04-18 (commits `702dd54` + follow-up)**. `Rendering/Primitives.cs` provides static `Initialize(GraphicsDevice)` + `Pixel` + `DrawRectangle(spriteBatch, rect, color)`. Adopted in the follow-up commit by the platformer (`Game1`/`PlayState`/`TitleState`) and the demo's `ConsoleUI`. The demo adoption also cleaned up a dead `GraphicsDevice` parameter chain through `BattleState` → `DebugState` → `ConsoleUI`. No unit tests (GraphicsDevice isn't headless-friendly); smoke-tested via both sample games.

### 1.3 Screen-space vs world-space rendering
Platformer has two draw contexts per frame: world (camera matrix) and UI (identity). The library provides `Camera2D.GetViewMatrix` but no guidance or helpers for mixed-context rendering. The tactical demo has one context because it uses no camera. Any future game with a camera will hit this.

**Recommendation**: Either document the two-pass pattern explicitly in CLAUDE.md, or ship a `Renderer` wrapper that manages world-vs-UI Begin/End. Probably the former — a wrapper is premature until more games show the same friction.

### 1.4 Input remapping is absent and both games pretend it's fine
Both games hardcode WASD/Arrows/Space. Any serious consumer will want rebinding. This is already on the Tier 3 backlog.

### 1.6 `ObjectPool<T>` works, but the ceremony repeats per game (NEW 2026-04-18c)
Shooter exercises `ObjectPool<Projectile>` and `ObjectPool<Enemy>` under real load (dozens of rent/return per second during combat). The pool itself is fine. What *does* repeat is the surrounding pattern:

```csharp
// Rent → Launch/Spawn → add to live list
// every frame: Update + cull dead → Return + remove from live list
private readonly ObjectPool<T> _pool;
private readonly List<T> _live = new();

// Rent:
T x = _pool.Rent();
x.Launch(...);
_live.Add(x);

// Cull:
for (int i = _live.Count - 1; i >= 0; i--) {
  _live[i].Update(dt);
  if (!_live[i].Alive) {
    _pool.Return(_live[i]);
    _live.RemoveAt(i);
  }
}
```

That block appears twice in Shooter's `PlayState` already. Any game with more entity types would repeat it per-type.

**Recommendation**: a thin `PooledEntitySet<T>` wrapper that combines pool + live-list + per-frame Update-and-cull using an `Alive` convention (or `Func<T,bool> isDead`). ~30 lines. Would delete ~20 lines per consumer entity type. Low-risk library addition after one more game confirms the pattern.

### 1.7 `TimerManager.Every` fits spawn waves well (NEW 2026-04-18c)
Shooter's enemy spawn uses `_timers.Every(1.5f, SpawnWave)` — exactly the shape the API was designed for, and a genuine one-line win over a hand-rolled accumulator. Confirms that `TimerManager` as-is is valuable *when* the pattern is "fire callback every N seconds", even if BattleGrid's per-frame countdowns don't fit.

(BattleGrid's four hand-rolled float counters from §5 still argue for a separate simpler API — the two styles serve different needs. Recommend keeping `TimerManager.Every`/`After` and adding a `CountdownTimer` struct that wraps `float remaining; bool Tick(dt)` for the common case.)

### 1.11 Turn scheduling: the library primitive doesn't fit; the hand-rolled one is trivial (NEW 2026-04-18e)
Roguelike needed a turn scheduler. The obvious library candidate — `Timing.TimerManager` — doesn't fit at all: timers are wall-clock and fire callbacks; turn scheduling is "after the player acts once, let each monster act once". They have different shapes.

What Roguelike ended up writing is ~10 lines in `PlayState.MonstersTurn`: a plain `foreach monster in list { ... }` loop that runs exactly once after each successful player action. BattleGrid's enemy AI tick is structurally similar (an action alternator gated by a timer). The shared pattern is *"after event X, run a one-shot pass over a set"* — not worth a library primitive.

**Recommendation**: do **not** add a `TurnScheduler` to the library. The per-game loop is 10 lines and wildly different in detail between BattleGrid and Roguelike. Two data points that look superficially similar but don't actually share code — the opposite of a library candidate.

### 1.12 Procgen lives in the game, not the library (NEW 2026-04-18e)
Roguelike's `DungeonGenerator` is ~80 lines of rooms-and-corridors carving. It operates entirely on `TileLayer<TileKind>` via the existing `Fill`/indexer/`InBounds` API. Nothing about the generator would transfer to another genre — even a second roguelike would likely want different generation (BSP, cellular automata, hand-authored prefabs) depending on its feel. The `TileLayer<T>` API is the right place to stop; generators stay per-game.

### 1.13 Scrolling text log is a third hand-rolled HUD pattern (NEW 2026-04-18e)
BattleGrid, Shooter, and now Roguelike all hand-roll some variety of scrolling text panel: BattleGrid's console, Shooter's last-event line, Roguelike's combat log with fade. Three games, three implementations, all ≤30 lines. The library has `Text.TextManager.ScrollText` for something like this — none of the games used it (it's coupled to the handle-based text model, which no game other than BattleGrid uses). A simple `LogBox` widget that takes (rect, font, max lines, fade style) would collapse ~90 lines across 3 games. Not an urgent extraction.

### 1.9 `TileMap` + `TileLayer<T>` handle match-3 well; a few sharp edges (NEW 2026-04-18d)
Puzzle's `Board` is the first real `TileMap` consumer. Verdict: the core API holds up, but there are sharp edges.

**What worked:**
- `TileLayer<Gem>` with a color-enum cell type is exactly the shape puzzle wanted. No wrapper needed.
- `Map.GetCellRect(c, r)` for rendering and `Map.WorldToCell(mousePos)` for click-pick are ergonomic both directions. Zero friction for the 80% case.
- `TileLayer.InBounds(c, r)` reads naturally in guard clauses.
- `Map.Origin` cleanly offsets the whole board to arbitrary screen coordinates without forcing a wrapper class.

**Sharp edges:**
- `Gems[c, r]` via a cached `TileLayer<Gem>` reference is what you want; `Map.GetLayer<Gem>("gems")[c, r]` is verbose. The usage pattern (cache once in ctor) isn't obvious — document it.
- `WorldToCell` returns a `(int, int)` tuple and doesn't clamp. Negative values and out-of-bounds come back silently. Puzzle added the bounds check itself. A `TryWorldToCell(Vector2, out int col, out int row)` that returns `bool` would be safer.
- Swap-two-cells is one of the most common grid ops; Puzzle hand-rolled a tuple-deconstructed swap. `TileLayer<T>.Swap((c,r), (c,r))` is a 3-line helper.

**Recommendation**: add `TileLayer.Swap`, `TileMap.TryWorldToCell`, and a CLAUDE.md note about caching the `GetLayer<T>` reference. Very low-effort, all backward-compatible.

### 1.10 SpriteFont default charset is a footgun for user-facing strings (NEW 2026-04-18d)
Every sample's content pipeline declares the Arial spritefont with `CharacterRegion Start=U+0020 End=U+007E` (ASCII printable). Any drawn string containing a character outside that range — em-dash `—`, curly quotes, non-breaking space, accented letters, anything pasted from a designer — crashes `SpriteFont.MeasureString` with `ArgumentException: Text contains characters that cannot be resolved by this SpriteFont.`

Puzzle hit this when a non-matching swap showed "No match — reverted" (the em-dash silently made it into the source from editor autocorrection or paste). One-character ASCII fix, but the defensive surface is real: every developer writing flavor text is one copy-paste away from a runtime crash on a rarely-visited UI path.

**Two workable library responses:**
1. **Widen the default spritefont charset** to include U+0020..U+00FF (Latin-1 supplement) plus common typographic punctuation (en-dash, em-dash, curly quotes, ellipsis). Template the Content.mgcb + spritefont when a new game is scaffolded. Catches 95% of paste-from-anywhere failures at the font-build step, not at runtime.
2. **Add a safe-draw helper** in `Text.TextManager` / a `Text.SafeFont` wrapper: `Measure`/`Draw` filter unknown glyphs down to `?` or a DefaultCharacter instead of throwing. Quieter failure mode for the remaining 5%.

Both are worth doing; (1) has higher leverage. Worth flagging in CLAUDE.md regardless, as a "things that will bite you" note.

### 1.8 `Camera2D.ScreenToWorld` is ergonomic for mouse-aim (NEW 2026-04-18c)
Shooter's aim direction needed three lines end-to-end:
```csharp
Vector2 mouseWorld = _camera.ScreenToWorld(_mouse.GetMousePosition());
Vector2 dir = mouseWorld - _player.Position;
dir.Normalize();
```
Zero friction. The Camera2D / MouseManager split composes cleanly. No change recommended.

### 1.5 `GameState.IsActive` conflates "updating" and "visible" (NEW 2026-04-18b)
`GameStateManager.Update` and `GameStateManager.Draw` both skip states where `IsActive == false`. That collapses two concerns into one flag. The intended pattern — "when a state is obscured by another, stop updating it but keep drawing it behind the overlay" — isn't expressible with the current API.

BattleGrid hit this when adding the chip-selection overlay. Pushing a `ChipSelectState` onto `PlayState` would have stopped `PlayState` from drawing too, hiding the battle behind the overlay. Workaround: `PlayState.Obscuring` is now a no-op (doesn't set `IsActive = false`), and chip selection is handled as an internal `Mode` enum rather than a separate pushed state. Works, but it pushes state-machine logic into the consumer that the library's state stack was supposed to handle.

**Recommendation**: split `IsActive` into `IsUpdating` and `IsVisible` (or similar). `PushState` sets the old top's `IsUpdating = false` but leaves `IsVisible = true`. That lets overlay-style states work as intended. Medium change; update the library tests accordingly.

---

## §2 — Platformer-specific patterns (genre-module candidates)

Patterns the tactical demo doesn't touch and likely wouldn't want. Strong candidates for `MonoGame.GameFramework.Platformer` *when* a second platformer-style game comes along.

### 2.1 Physics loop shape
- Gravity applied per-frame to `Velocity.Y` with terminal-velocity clamp.
- Horizontal input → target velocity → accelerate toward it (ground accel ≠ air accel).
- Separate-axis AABB resolution: move X, resolve X collisions; then move Y, resolve Y.
- `IsGrounded` set by collision resolution; consumed by coyote + jump-accel gating.

All of this is ~50 lines concentrated in `Player.Update`. It would be identical in any sibling platformer.

### 2.2 Jump feel
- Coyote time window (150 ms).
- Jump buffer window (150 ms).
- Variable-height jump (release button → clamp upward velocity).
- Post-move jump re-check so buffered jump fires on the same frame as landing.

This is the single most-duplicated chunk of code between any two platformers. A `PlatformerJumpConfig { Velocity, CutVelocity, CoyoteTime, BufferTime }` + a `PlatformerController` helper that consumes `(inputX, jumpPressed, jumpHeld, IsGrounded)` and produces `Velocity` would eliminate most of `Player.cs`.

### 2.3 Camera follow with snap-on-respawn
`Camera2D.FollowLerp` handles the normal follow. On respawn, we need to `Position = Target` to avoid a long lerp across the whole level. Both platformers will want this.

### 2.4 Death plane
`if (player.Position.Y > deathPlaneY) Respawn();` — common to any game with pits. Not sure it rises to a library primitive; it's one line per game.

### 2.5 Patrolling enemy
The `Enemy` class bounces between two x-coordinates at constant speed. Appears in many 2D genres (platformer, sidescroller, shmup). Could be `AI.PatrolBehavior` at the library level rather than a platformer-specific module.

---

## §3 — BattleGrid-specific patterns (for a hypothetical `GridDuel` module)

BattleGrid is no longer turn-based — it's real-time with a chip-selection pause. Patterns that would recur in any grid-duel style game:

- Grid-to-pixel coordinate helpers (`Grid.PlayerCellTopLeft`, `Grid.EnemyCellCenter`, `Grid.RowCenterY`). Two-sided 3×N grids are common in this genre. Could become `Rendering.TwoSidedGrid` or similar.
- Action-tick AI: simple state machine ticking at a fixed interval, alternating categories of actions (move / attack, or similar). A couple of lines of code but repeated in every enemy in this genre.
- Chip / card / ability selection overlay: pauses gameplay, offers 2–4 random options from a pool, commits on pick, then cooldown. The "inline Mode enum instead of separate state" workaround from §1.5 is really this pattern.
- Row-based projectile collision: projectile + entity collision is really "do they share a row at a given X overlap?" — row is the dominant axis. A `Rendering.RowGrid` helper that precomputes row bounds would simplify collision checks in dense patterns.
- HP bar + cooldown-indicator HUD: BattleGrid's hand-rolled bars via `Primitives.DrawRectangle` would repeat in every game with 1–2 combatants. A small `HUD.HpBar(rect, current, max, fill)` helper would eat this.

Not extracting yet (only one consumer), but the density of these patterns is higher than the platformer's — a second grid-duel game would turn most of them into library code quickly.

---

## §4 — API friction

Things that work but awkwardly. Each is a candidate for a small library fix; grouped by cost/benefit.

### 4.1 `UIManager.AddUIElement` forces `DrawManager.AddSprite`
`UIManager` tracks elements for hit-testing (`GetElementAt`, `HoveredElement`, `OnClick`). It also registers each element with `DrawManager`. The registration is hard-coded and can't be opted out of.

**Why this hurts**: Platformer uses two draw contexts (camera + screen). UI belongs in screen space. `DrawManager.Draw` renders in whatever transform the active `SpriteBatch.Begin` provides — but `DrawManager` always draws everything registered, and always with `Color.White`. The platformer's `TitleState` therefore adds buttons to `UIManager` (for the interaction layer) but never calls `_drawManager.Draw`, and instead re-draws the buttons manually with color + hover state.

**Recommendation**: split into `UIManager` (interaction: hit-test, focus, click callbacks, hover — no coupling to drawing) and a separate optional `UIRenderer` that *can* render registered elements through DrawManager for consumers that want that. Keep the existing one-line convenience `AddUIElement(group, sprite)` as an extension method that does both.

> **✅ Fixed 2026-04-18 (commit `702dd54`)**. `UIManager` constructor no longer takes `DrawManager`; `AddUIElement`/`RemoveUIElement` no longer touch it. Demo's `ConsoleUI` and `PlayerHealthUI` now call `DrawManager.AddSprite`/`RemoveSprite` explicitly alongside the UI registration — two lines instead of one, but the coupling is gone. No separate `UIRenderer` was introduced; the decoupling alone was enough. Covered by 5 new `UIManagerTests` including one that constructs `UIManager` with a null `MouseManager` to prove no `DrawManager` is required.

### 4.2 `DrawManager` renders everything white
`DrawManager.Draw` hardcodes `Color.White` and doesn't pass through a tint. For non-white sprites this is fine (texture has the colors). For the pixel-texture pattern in §1.2, it prevents colored rectangles through `DrawManager`.

**Recommendation**: when the pixel-texture utility (§1.2) lands, `SpriteSheet` should grow a `Tint` property that `DrawManager.Draw` respects, defaulting to `Color.White`.

> **✅ Fixed 2026-04-18 (commit `702dd54`)**. `SpriteSheet.Tint` is a mutable `Color` property defaulting to `Color.White`; `DrawManager.Draw` uses `sprite.Tint` instead of hardcoded white. Consumers can now tint, flash, or fade sprites at runtime without replacing them. Covered by 2 new `SpriteSheetTests`.

### 4.3 `Entity` abstract class is unused in 2 of 3 sample games
Platformer and Shooter's entity types (Player/Enemy/Projectile/Platform/Goal) are plain classes, not `Entity` subclasses. BattleGrid is the only consumer, and even there the abstract methods mostly no-op.

The abstract class is not forcing any real shape. Two of three games actively don't use it.

Notably, Shooter's entities evolved a *different* shape from BattleGrid's: plain `Alive` flag + `Update(float dt, ...)` + `Draw(SpriteBatch)` with no `ContentManager` involvement at all. This is the shape that lines up with the `ObjectPool<T>` lifecycle — `Entity`'s `LoadContent`/`UnloadContent` are pool-hostile.

**Recommendation**: strongly consider deleting `Core.Entity`. If a base entity earns its keep in the future, the Shooter shape (`bool Alive`, `Update(dt)`, `Draw`) is a better starting point than the current ContentManager-centric shape.

### 4.4 `Text` rendering has two disconnected paths
Tactical uses `Text.TextManager` (handle-based, batched). Platformer uses `SpriteBatch.DrawString` directly with a cached `SpriteFont`. Both work; neither dominates. `TextManager` is the right choice for HUD-like text that persists across frames; direct `DrawString` is right for one-off overlays ("You Win"). The library doesn't document which to pick.

**Recommendation**: CLAUDE.md should have a short "text rendering" section with the two patterns and when each fits. No code change needed.

> **✅ Fixed 2026-04-18 (commit `702dd54`)**. CLAUDE.md now has a "Text rendering" subsection under Architecture spelling out when to pick `TextManager` vs direct `DrawString`.

---

## §5 — Unused library surface (over-build audit)

Some library code was written speculatively and hasn't earned its place yet. Not a call to delete anything — just to be honest about validation status.

| Primitive | Status | Likely trigger |
|---|---|---|
| `ObjectPool<T>` | **Validated in Shooter** | Both Projectile and Enemy run through pools with prewarm + onReturn. Handled load fine. Surrounding boilerplate is the new finding — see §1.6. |
| `Timer` / `TimerManager` | **Partially validated in Shooter** | `TimerManager.Every` fits enemy spawn waves perfectly (§1.7). Raw `Timer` class still unused. BattleGrid's four hand-rolled countdowns still argue for a simpler `CountdownTimer` shape. |
| `Tween<T>` / `Easing` | Unused after 4 games | BattleGrid's sword-flash alpha and Puzzle's gem-fall animation are the obvious customers; both skipped for MVP scope. If the first polish pass on either doesn't reach for Tween, delete it. |
| `SaveSystem` / `SaveFile` | Unused | First persistent progress |
| `TileMap` / `TileLayer` | **Validated in Puzzle + Roguelike** | Two consumers at very different scales (7×7 vs 60×34). API scaled cleanly. BattleGrid and Platformer still hand-roll their grids; migration is reasonable follow-up. |
| `AssetCatalog` | Unused | Any game with ≥ 10 content entries. Both samples are down to one asset each (the font). |
| `ILogger` / `ConsoleLogger` | Unused | First debugging session painful enough to add logging. `ConsoleUI` partially fills this role in BattleGrid but doesn't use the library's logger. |
| `MathUtilities` | Unused after 3 games | Shooter used `System.Random` + `Vector2.Normalize` + `Math.Clamp` directly — `MathUtilities.Angle`/`RandomFloat`/`RandomInt` never felt missing. Strong candidate for deletion if puzzle + roguelike also skip it. |
| `PerformanceMonitor` | Unused | First perf complaint |
| `EventManager.Subscribe<T>`/`Publish<T>` (typed) | Unused | Any game that grows past ~5 event types. BattleGrid's event set (PlayerMoved/PlayerHit/EnemyHit/FiredProjectile) stayed small enough that the string API is still fine. |

After five games, three primitives are validated (`ObjectPool`, `TimerManager.Every`, `TileMap`/`TileLayer`). Three more are partially validated (`DrawManager`, `Camera2D`). The rest — roughly 30% of the library — has never been used. That's the steady-state signal. §8 below turns this into concrete keep/delete recommendations.

---

## §6 — Recommended actions

Ordered by value-per-effort. None are urgent; pick when the trigger hits.

1. ✅ **Fix `GameStateManager` lifecycle auto-calls** (§1.1). **Done 2026-04-18.**
2. ✅ **Decouple `UIManager` from `DrawManager`** (§4.1). **Done 2026-04-18.**
3. ✅ **Ship the pixel-texture helper + `SpriteSheet.Tint`** (§1.2, §4.2). **Done 2026-04-18.** Adopted by both samples in the follow-up commit.
4. ⏳ **Start a `MonoGame.GameFramework.Platformer` genre module** once a *second* platformer is started (§2). **Still deferred.** After BattleGrid shipped, it's now clearer that platformer + grid-duel share *nothing* — extraction is still sample-of-one.
5. ⏳ **Delete or redesign `Entity` abstract class** (§4.3). **Still deferred.** BattleGrid uses `Entity` but gains nothing from it; verdict unchanged — wait for game #3.
6. ✅ **Document text rendering paths in CLAUDE.md** (§4.4). **Done 2026-04-18.**

### New from 2026-04-18b

7. ⏳ **Split `GameState.IsActive` into `IsUpdating` and `IsVisible`** (§1.5, new). BattleGrid's chip-selection overlay couldn't be implemented as a pushed state because `IsActive = false` would also stop the background battle from drawing. It lives as an inline `Mode` enum instead. A proper overlay API is likely the single most impactful library change if a third game has any pause/menu overlay. **Medium effort, high leverage.**
8. ⏳ **Simpler per-frame `Timer` shape** (§5). `TimerManager.After`/`Every` don't fit the pattern that most games actually hit: "decrement each frame and check if elapsed". Four hand-rolled float counters in BattleGrid confirm this. A `Timer.Tick(dt)` returning a bool would eliminate that duplication.
9. ⏳ **Migrate BattleGrid and Platformer to `TileMap`/`TileLayer`** (§5). Both games hand-roll what the library already provides; the adoption would validate the library primitives and remove local code. Pure win when next touching either game.

Deferred — wait for a trigger:
- Input rebinding layer (Tier 3 backlog; first user friction).
- Particle system (landing dust would trigger this).
- Dev console overlay (first painful debugging session).
- Pathfinding (smart enemy).

---

## §7 — What the two-game exercise actually proved

- **Camera2D was worth building**. Only game #2 used it, but it was critical there. The audit-phase dirty-flag caching even had a measurable validation target (the `GetViewMatrix` stability test in the test project).
- **Test project caught a real bug** (`Tween<T>` zero-duration → stuck at `From`). The 87 tests cost about an hour; the single bug they found would have taken longer to notice.
- **Genre modules were correctly deferred**. Building them before game #2 would have designed against imagined abstractions. The items in §2 are concrete because they came from real duplication.
- **Over-build is visible now**. §5 lists 10+ primitives not used by either game. Some are fine ("will land soon"), some are speculative ("built because it seemed generally useful"). Honest accounting prevents the library from growing into a dumping ground.
- **The library passed the "second consumer" test.** The platformer was built in ~9 small phases without blocking changes to the library (aside from discoveries logged here). That's the main signal that the core abstractions are OK.
- **The findings loop works.** Four of the six §6 items were fixed in a single bundled follow-up commit (`702dd54`) with test coverage for each. The two deferred items (genre module, `Entity` redesign) are explicitly sample-of-one extractions that only game #3 can honestly validate — deferring them was correct, not procrastination.

### Updated after BattleGrid's buildout (2026-04-18b)

- **Genre-module extraction still has no signal, and the reason is stronger now.** Platformer and BattleGrid don't just have different mechanics — they have *different shapes of problem*. Platformer's core is continuous physics + camera; BattleGrid's core is discrete grid movement + discrete attack scheduling. The two share nothing at the gameplay layer. The only overlap is plumbing that already lives in the library (states, input, rendering, font).
- **The library's state-stack API doesn't handle pause-overlays.** §1.5 is the single biggest new friction. Most games that grow beyond a single play screen will hit it.
- **`Rendering.Primitives` was the right addition.** Both samples now use it ubiquitously; neither had to re-invent a 1×1 pixel texture. Low-effort library win validated by real use.
- **Rectangle-based entity rendering revealed a hitbox-anchor class of bug.** When Phase 5 moved the projectile spawn to the character's vertical center but left the hitbox as a 38×22 sub-region anchored top-left, collisions silently stopped working. Lesson for any `Entity` redesign: if a `Bounds` concept lands on the base class, it should default to the full visual rect — sub-hitboxes are a per-game opt-in with real risk of this exact desync.
- **Hand-rolled patterns in BattleGrid point to small, targeted library additions** (§3). A `TwoSidedGrid`, a simpler `Timer.Tick`, a `HUD.HpBar` — each would pay for itself in a second game of the same genre. None big enough to justify speculative build.

### Updated after Shooter (2026-04-18c)

- **`ObjectPool` works at load, but the rent/update/cull loop is boilerplate.** §1.6's `PooledEntitySet<T>` proposal is the first real library win to come out of Shooter. One more game with pooled entities would justify building it.
- **`Camera2D` is now 2-game-validated**, across two very different camera patterns (side-scrolling follow vs. twin-stick follow with screen-to-world). Confirmed worth keeping.
- **Entity base class's lack of validation deepened.** Shooter is the second game to skip `Core.Entity`, with a materially different entity shape optimized for pooling (§4.3). A third skip would make deletion clearly the right call.
- **HP-bar HUD is a repeating pattern.** BattleGrid and Shooter both hand-roll the same 4-rectangle bar (background, fill, 4 border strips). Two datapoints is starting to look like a library helper.
- **Title/Play state split is a shared skeleton across all 3 games.** The copy-paste between title states is ~80% identical. A `TitleState` base class in the library with virtual `Draw`/button-config hook would eliminate it — but only if the 4th and 5th games also follow this pattern.

### Updated after Puzzle (2026-04-18d)

- **`Core.Entity` deletion is now the obvious call.** 3 of 4 games skip it. Puzzle didn't even need to skip it consciously — grid cells aren't entities. Holding the class gives zero value and costs one naming decision ("is this an Entity or a plain class?") for every new game.
- **`TileMap` / `TileLayer` is a validated primitive.** Puzzle's `Board` is 200 lines on top of it and reads cleanly. The three sharp edges (§1.9) are the natural backlog.
- **`TitleState` copy-paste is now 4 games deep.** Roughly 80 lines duplicated per game, zero variation beyond button labels and background color. Clear candidate for a library `TitleState<TPlay>` base class or a `MenuBuilder` helper. Will decide in §8 after Roguelike.
- **SpriteFont charset footgun (§1.10) is the first library-level safety issue.** Any game shipped to real users would hit it. Fixing this in the sample-game template is cheap; fixing it properly in the library needs a small decision (widen font, or add safe-draw). Priority bumped to "do this before the next game if possible".
- **Four games, zero uses of `SoundManager`, `AssetCatalog`, `SaveSystem`, typed events, `MathUtilities`, raw `Timer`, `PerformanceMonitor`, `SpriteSheet.Animated`.** That's roughly a third of the library that has yet to justify its existence. §8 should make delete-or-keep recommendations on each.
- **`DrawManager` is validated but not essential.** Puzzle and Shooter skipped it and drew entities directly. Either pattern works; the library doesn't force it. That's the right shape.

### Updated after TowerDefense (2026-04-18f)

- **`ObjectPool` pattern confirmed again.** Second pooled-entity game produces the same rent/update/cull boilerplate as Shooter. §1.6's `PooledEntitySet<T>` proposal now has two independent consumers justifying it. Upgrading to a Tier D extraction candidate at the next §8 revision.
- **Grid-cell click interaction is hand-rolled, NOT `UIManager.OnClick`.** TowerDefense ignored `UIManager` for placement entirely — it reads mouse position, computes `(col, row)` from origin + cell size, checks the path-cell set and tower dictionary, places. ~15 lines total. Making this a `UIManager` feature would require creating a `SpriteSheet` per grid cell (20×14 = 280 sprites) for hit-testing that doesn't need pre-existing visuals. The grid-click pattern is genuinely different from button-click.
  - **Finding**: `UIManager` is right to stay button-focused. A *separate* helper `Grid.TryMouseToCell(mousePos, origin, cellSize, cols, rows, out int col, out int row)` would eliminate TowerDefense's hand-rolled conversion. Puzzle had the same hand-rolled conversion. Two consumers. Flag for extraction.
- **Per-enemy HP bars validate the `UI.HpBar` helper candidate at scale.** TowerDefense renders up to ~15 enemy HP bars simultaneously, each with the same 4-rectangle pattern (bg + fill + no border at this size). Confirms §8 Tier D `HpBar` proposal would handle many-entities case without issue.
- **`TimerManager.Every` for wave spawns worked; but timer-cancel would be useful.** The wave spawn timer runs for the duration of the wave; there's no clean way to stop it after the wave's target count is reached (I just made `TrySpawnEnemy` check the counter and no-op). The existing `Timer.Cancel()` would work if the `TimerManager.Every` call returned the `Timer` — it does (`return timer`), I just didn't store it. Small finding: document that `Every`/`After` return the timer and that you can Cancel it.

### Updated after Roguelike (2026-04-18e) — and this is the fifth game

- **`TileMap` scales from 49 cells to 2,040 cells with zero friction.** Two consumers, two very different use-cases (match-3 gems vs. procgen dungeon), same API. Confirmed keeper.
- **`Core.Entity` deletion is now unambiguous.** 4 of 5 games skip it. Roguelike's `Actor` base class is a different shape entirely (grid `Col`/`Row` + HP + `Damage`), directly incompatible with `Core.Entity`'s `ContentManager`-centric lifecycle. No future game is likely to adopt `Core.Entity` as-is.
- **`TitleState` copy-paste is now 5 games deep.** Same ~80 lines per game, still zero variation beyond labels + background color. This is unambiguously a library extraction candidate; §8 will make the concrete proposal.
- **Turn scheduling doesn't want a library primitive.** §1.11 argues against adding a `TurnScheduler`: the two games with turn-ish logic (BattleGrid action ticks, Roguelike monster passes) don't share enough to justify it. Good example of a superficially-similar-but-actually-distinct pattern.
- **Procgen lives per-game (§1.12).** The `TileLayer<T>` API is the right stopping point; generators shouldn't live in the library.
- **Scrolling text logs are a third repeating pattern (§1.13).** BattleGrid, Shooter, Roguelike all hand-roll ~20-line variants. Medium-priority extraction candidate (`LogBox` widget).
- **Five games, ~30% of the library still unused.** Stable signal now — next game won't change the picture meaningfully. §8 makes delete/keep calls.

---

## §8 — Recommended next library work (prioritized, from 5-game data)

Five games across five genres (grid-duel, platformer, twin-stick shooter, match-3 puzzle, turn-based roguelike) are in hand. The signal is as strong as it's going to get without shipping a real project. This section turns every preceding observation into a concrete, actionable decision: **do this**, **delete this**, **hold for trigger**, or **do not build this**.

Tiers are ordered by priority. Each item names the section it came from for traceability.

### Tier A — Do now (safety + trivial wins)

These are small, backward-compatible, and have concrete evidence. Build them before the next game.

1. **Widen default spritefont charset** (§1.10). Update both the sample-game template and each existing game's `Content/fonts/Arial.spritefont` to cover U+0020..U+00FF plus common typographic punctuation (em-dash, en-dash, curly quotes, ellipsis). Zero API change; just a content-pipeline setting. Prevents the class of crash Puzzle hit at runtime. **~5 lines of XML per game.**
2. **`TileLayer<T>.Swap((c,r),(c,r))`** (§1.9). 3-line helper; Puzzle would have used it.
3. **`TileMap.TryWorldToCell(Vector2, out int col, out int row)`** returning `bool` (§1.9). Bounds-clamping variant. Puzzle added the check itself because the existing `WorldToCell` returns out-of-bounds silently.
4. **Document the `cache-GetLayer<T>` pattern in CLAUDE.md**. Existing `TileMap` consumers all cache the layer reference in their constructor; this is the intended usage. One sentence in docs.

### Tier B — Delete (high confidence after 5 games)

Each has zero or near-zero consumers across five genre-diverse games and isn't "the kind of utility that'll land in game 6" — they've been actively rejected or bypassed.

1. **`Core.Entity`** — 4 of 5 games skip it. Roguelike's `Actor` class is structurally incompatible. §4.3 and the Roguelike bullet above.
2. **`SpriteSheet.Animated`** — five games, zero consumers. All games use `SpriteSheet.Static` (for buttons) or draw rectangles directly.
3. **`Utilities.MathUtilities`** — five games, zero consumers. Callers reach for `Vector2.Normalize`, `Math.Clamp`, and `System.Random` directly. Keep those as the idiom.
4. **`Timing.Timer` (raw class)** — five games, zero consumers. Only `TimerManager.Every` gets used.
5. **`Events.EventManager.Subscribe<T>`/`Publish<T>` (typed API)** — five games, zero consumers. The string API handles every game's event set. Keep the string API; delete the typed pair.
6. **`Debugging.PerformanceMonitor`** — five games, zero consumers. If perf problems surface later, `GC.GetTotalMemory` + a frame counter is 10 lines. No need for a class.

**Estimated deletion**: ~400 lines across those six, plus matching test removals. Net simpler library.

### Tier C — Hold (unused but has a clear future trigger)

Don't build or delete. Reassess when the trigger fires.

1. **`Audio.SoundManager`** — triggers on the first game that wants sound. All five samples are silent; a sixth probably won't be.
2. **`Persistence.SaveSystem` / `SaveFile<T>`** — triggers on the first game with persistent progress (high score, run history, settings).
3. **`Content.AssetCatalog`** — triggers at ~10 content entries in one game. Currently each game has 1 (the font).
4. **`Tween.Tween<T>` / `Easing`** — triggers on the first polish pass. Gem-fall (Puzzle), sword flash fade (BattleGrid), camera shake easing all want this. If the next polish pass on any game doesn't reach for Tween, demote to Tier B.
5. **`Debugging.ILogger` / `ConsoleLogger`** — triggers on the first painful debugging session. BattleGrid's `ConsoleUI` partially fills this role but is UI-coupled.
6. **`Rendering.DrawManager`** — not in the delete list because it *is* used (BattleGrid + Platformer), but the 2-of-5 adoption rate says it's optional, not essential. Keep it; stop treating it as the default rendering pattern. Update CLAUDE.md accordingly.

### Tier D — Extract these patterns from game code (library wins from 5-game evidence)

Patterns that appeared in 3+ games as identical copy-paste. These are the highest-leverage additions the library can get from this exercise.

1. **`Lifecycle.TitleState` base class / `MenuBuilder` helper**. Five games, ~80 lines of identical copy-paste each. The variation is:
   - Background color (one `Color`).
   - Title + subtitle + hint strings.
   - Button labels + `Action` callbacks.
   - Button layout (center-stack in all 5 games).

   A base class `abstract class TitleScreenState : GameState` with overridable `Title`/`Subtitle`/`Hint`/`Buttons` properties would collapse each to ~30 lines of actual variation. **High priority — appears in every game.**

2. **`UI.HpBar(rect, current, max, fill, background?, border?)` drawing helper**. BattleGrid, Shooter, Roguelike all hand-roll the same 4-rectangle bar (bg + fill + 4 border strips). **High priority — appears in 3 of 5 games, zero variation in structure.**

3. **`UI.LogBox`** scrolling-text widget (§1.13). BattleGrid, Shooter, Roguelike all hand-roll ~20-line scrolling text panels. Minor variation in fade style. **Medium priority — genuinely shared code, but simple enough that hand-rolling isn't painful.**

4. **`Pooling.PooledEntitySet<T>`** helper (§1.6). Shooter's pattern. Only one consumer today — *defer until a second pooled-entity game appears*. Flagged here so it's not forgotten.

5. **`Timing.CountdownTimer` struct** — `float remaining; bool Tick(float dt) => (remaining -= dt) <= 0;`. BattleGrid's four hand-rolled countdowns (§5/§1.7). Extraction if BattleGrid is revisited; otherwise defer until a game ships ≥3 of these.

### Tier E — Do NOT build (explicitly rejected after evidence)

These are temptations that the data has disarmed.

1. **`TurnScheduler` library primitive** (§1.11). BattleGrid and Roguelike both have turn-ish logic, but implementations don't share meaningful code. Resist.
2. **Genre modules** (`Platformer`, `GridDuel`, etc.). All five games share almost nothing at the gameplay layer. Extraction today would be sample-of-one for every genre. *Revisit if a second platformer or second grid-duel game ever appears.*
3. **Procgen helpers in the library** (§1.12). Per-game. `TileLayer<T>` is the correct stopping point.
4. **Per-game state-visibility split** (§1.5 was open, now reassessed). BattleGrid's inline `Mode` enum works fine; three more games shipped without hitting this friction again. Demote from "build this" to "monitor". If a sixth game needs a pause overlay with a different mechanism than BattleGrid's, revisit.

### Tier F — Still trigger-driven (no change)

- Particle system — first game needing dust/sparks/trails.
- Input rebinding — first user friction.
- Pathfinding — first game with a smart AI.
- FOV — second roguelike or stealth game.
- Dev console overlay — first painful debugging session.
- GUI widgets beyond HpBar/LogBox — first menu-heavy screen.
- Shader / post-processing helpers — first screen-space effect.

### Summary

Concrete next commit plan if you build from here:

1. **One cleanup commit**: widen spritefont charset + add `TileLayer.Swap` + `TileMap.TryWorldToCell` + CLAUDE.md `GetLayer<T>` note. (Tier A, ~1 hour.)
2. **One deletion commit**: drop `Core.Entity`, `SpriteSheet.Animated`, `MathUtilities`, `Timing.Timer`, typed `EventManager` API, `PerformanceMonitor`, and their tests. Update consumers (really just BattleGrid's `Entity` subscribers). (Tier B, ~2 hours.)
3. **One extraction commit**: `TitleScreenState` base class; migrate all 5 games to inherit from it. (Tier D item 1, ~2 hours including the migrations. Biggest visible win.)
4. **Optional second extraction commit**: `HpBar` helper; migrate BattleGrid / Shooter / Roguelike. (Tier D item 2, ~1 hour.)

Everything else waits for a concrete real-project need to surface. The library will be materially smaller, better-understood, and more focused after those three commits than it is today — and the data to justify every change is on file in §§1–7.
