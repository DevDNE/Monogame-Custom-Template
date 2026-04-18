# Findings — Library Review from Building Two Sample Games

Original snapshot: 2026-04-18. Updated 2026-04-18 (a) — four non-deferred §6 items landed in commit `702dd54`; their findings below are annotated as **Fixed**. Updated 2026-04-18 (b) — BattleGrid expanded from a tactical-grid stub into a real game with chip selection, enemy AI patterns, and a full HUD. New findings recorded inline and in §7.

## Context

The framework has been exercised by two sample games in deliberately different genres:

1. **`MonoGame.GameFramework.BattleGrid`** — real-time grid duel inspired by Mega Man Battle Network. 3×3 grid per side, WASD movement, space-bar buster, Tab-triggered chip selection from a pool of four (Cannon / Wide Shot / Sword / Recov), enemy AI alternating between movement and two attack patterns, HP bars + controls hint HUD. (Originally a tactical-grid stub before the 2026-04-18b expansion.)
2. **`MonoGame.GameFramework.Platformer`** — side-scrolling platformer with gravity, AABB collision, camera follow, a patrolling enemy, goal + win state, and a title screen.

The point of building game #2 was to let real duplication (or real divergence) surface patterns worth promoting into the library or into a genre module, rather than guessing at abstractions up front. This doc captures what the two consumers revealed.

---

## Library surface coverage

**Validated by both games:**
- `Input.KeyboardManager`, `Input.MouseManager`
- `Rendering.SpriteSheet` factory methods (`Static`), `SpriteSheet.Tint`
- `Rendering.Primitives` (`Initialize` + `Pixel` + `DrawRectangle`) — now the universal pattern for colored rectangles in both samples
- `Rendering.DrawManager`
- `Lifecycle.GameState` + `Lifecycle.GameStateManager` (auto-lifecycle calls from the 2026-04-18 fix)
- `UI.UIManager` (BattleGrid & platformer both use hit-testing + `OnClick` + `HoveredElement` for title buttons; BattleGrid also uses `AddUIElement` without drawing for the console overlay)
- `Core.Entity` pattern (BattleGrid subclasses it for all its entities; platformer uses plain POCOs — the abstract class still doesn't earn its keep, see §4.3)

**Validated by one game:**
- BattleGrid: `Events.EventManager` (string API), `Lifecycle.SceneManager`, `Persistence.SettingsManager`, `Text.TextManager` (only the tilde-console log; HP moved to the HUD), `Input.GamePadManager`, `SpriteSheet.Animated` no longer needed — all static rects
- Platformer: `Rendering.Camera2D` (follow, view matrix, snap-on-respawn)

**Not used by either game (speculative):**
- `Audio.SoundManager`
- `Content.AssetCatalog`
- `Debugging.ILogger` / `ConsoleLogger` / `PerformanceMonitor`
- `Engine/Pooling.ObjectPool<T>`
- `Engine/Timing.Timer` / `TimerManager`
- `Persistence.SaveSystem` / `SaveFile<T>`
- `Rendering.TileMap` / `TileLayer<T>`
- `Tween.Tween<T>` / `Easing`
- `Utilities.MathUtilities`
- `Events.EventManager` typed API (`Subscribe<T>`/`Publish<T>`)

That unused surface is roughly 30% of the public API. Some is genuinely "just-in-case" utility that'll land when a game needs it (SaveSystem, Pooling, Timer, Tween). Some may be over-built relative to the sample-game scale — flagged in §5.

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

### 4.3 `Entity` abstract class is unused in the platformer and adds no value in BattleGrid
Platformer's `Player`/`Platform`/`Enemy`/`Goal` are plain classes, not `Entity` subclasses. BattleGrid's `Player`/`EnemyPlayer`/`Projectile`/`Gameboard` *do* subclass `Entity`, but they only inherit the method signatures (`LoadContent`/`UnloadContent`/`Update`/virtual `Draw`) — most entities pass `ContentManager` through to do nothing, and their `Draw` is empty because rendering is via `DrawManager`.

The abstract class is not forcing any real shape. Each game could delete it and work fine.

**Recommendation**: consider making `Entity` genuinely useful (e.g., own a `Position`, `Bounds`, or a `Draw`/`Update` lifecycle that a scene iterates and disposes) or delete it. Right now it's a naming convention with method signatures that don't earn their keep. Revisit once a third game tells us what entities actually share — two games haven't produced signal.

### 4.4 `Text` rendering has two disconnected paths
Tactical uses `Text.TextManager` (handle-based, batched). Platformer uses `SpriteBatch.DrawString` directly with a cached `SpriteFont`. Both work; neither dominates. `TextManager` is the right choice for HUD-like text that persists across frames; direct `DrawString` is right for one-off overlays ("You Win"). The library doesn't document which to pick.

**Recommendation**: CLAUDE.md should have a short "text rendering" section with the two patterns and when each fits. No code change needed.

> **✅ Fixed 2026-04-18 (commit `702dd54`)**. CLAUDE.md now has a "Text rendering" subsection under Architecture spelling out when to pick `TextManager` vs direct `DrawString`.

---

## §5 — Unused library surface (over-build audit)

Some library code was written speculatively and hasn't earned its place yet. Not a call to delete anything — just to be honest about validation status.

| Primitive | Status | Likely trigger |
|---|---|---|
| `ObjectPool<T>` | Unused | A game with many short-lived objects. BattleGrid allocates a new `Projectile` per shot; wide shots + rapid fire fire multiple per frame. Still not painful enough to pool, but the first "many projectiles" game will hit this. |
| `Timer` / `TimerManager` | Unused | BattleGrid uses plain `float` counters for `_actionTimer`, `_shootTimer`, `_chipCooldown`, `_swordFlashRemaining`. That's 4 timers hand-rolled in one file — a data point that `TimerManager`'s `After`/`Every` shape doesn't match continuously-ticking grace windows well. The library's `Timer` should probably grow a simpler "tick me every frame and fire callback when elapsed" form. |
| `Tween<T>` / `Easing` | Unused | BattleGrid's sword-flash alpha fade would be a natural customer (currently a binary on/off). Any UI polish pass would use this. |
| `SaveSystem` / `SaveFile` | Unused | First persistent progress |
| `TileMap` / `TileLayer` | Unused | BattleGrid hand-rolls a 3×3×2 grid via two `SpriteSheet[,]` arrays plus `Grid.cs` helpers. Platformer hand-rolls a `List<Platform>`. Both would fit `TileMap`/`TileLayer` almost perfectly — the only reason neither uses it is inertia. |
| `AssetCatalog` | Unused | Any game with ≥ 10 content entries. Both samples are down to one asset each (the font). |
| `ILogger` / `ConsoleLogger` | Unused | First debugging session painful enough to add logging. `ConsoleUI` partially fills this role in BattleGrid but doesn't use the library's logger. |
| `MathUtilities` | Unused | Randomness, angle math — both games got away without it. BattleGrid uses `System.Random` directly. |
| `PerformanceMonitor` | Unused | First perf complaint |
| `EventManager.Subscribe<T>`/`Publish<T>` (typed) | Unused | Any game that grows past ~5 event types. BattleGrid's event set (PlayerMoved/PlayerHit/EnemyHit/FiredProjectile) stayed small enough that the string API is still fine. |

After two full games, the unused surface is *the same 10 primitives* as before. That's a strong signal: the library has ~30% speculative code that two genre-diverse games didn't need. Not a call to delete — several are the kind of utility that lands in game 3 or 4 and pays for itself then — but an honest data point. Most aggressive candidates to remove if a third game still doesn't use them: `PerformanceMonitor`, typed `EventManager` API, `MathUtilities.Random*`.

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
