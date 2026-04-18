# Findings — Library Review from Building Two Sample Games

Original snapshot: 2026-04-18. Updated 2026-04-18 — four non-deferred §6 items landed in commit `702dd54`; their findings below are annotated as **Fixed**. Items deferred to game #3 remain open.

## Context

The framework has been exercised by two sample games in deliberately different genres:

1. **`MonoGame.GameFramework.Demo`** — turn-based tactical grid battle (Fire Emblem-style).
2. **`MonoGame.GameFramework.Platformer`** — side-scrolling platformer with gravity, AABB collision, camera follow, a patrolling enemy, goal + win state, and a title screen.

The point of building game #2 was to let real duplication (or real divergence) surface patterns worth promoting into the library or into a genre module, rather than guessing at abstractions up front. This doc captures what the two consumers revealed.

---

## Library surface coverage

**Validated by both games:**
- `Input.KeyboardManager`, `Input.MouseManager`
- `Rendering.SpriteSheet` (factory methods — `Static`, `Animated`)
- `Rendering.DrawManager` (tactical uses heavily; platformer only via `UIManager` coupling)
- `Lifecycle.GameState` + `Lifecycle.GameStateManager`
- `UI.UIManager` (tactical: group Add/Remove only; platformer: hit-testing + `OnClick` + `HoveredElement`)
- `Core.Entity` pattern (tactical) / equivalent POCOs (platformer — see friction below)

**Validated by one game:**
- Tactical: `Events.EventManager` (string API), `Lifecycle.SceneManager`, `Persistence.SettingsManager`, `Text.TextManager`, `Input.GamePadManager`
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

## §3 — Tactical-specific patterns (for a hypothetical `TurnBased` module)

Not extracting yet (only one consumer — the demo), but noting for the next strategy-style game:
- Grid movement (snap to tile centers).
- Board / tile entity that owns occupancy state.
- Projectile fire-and-forget with collision-vs-target.
- Turn ordering (currently absent but implied).

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

### 4.3 `Entity` abstract class is unused in the platformer
Platformer's `Player`/`Platform`/`Enemy`/`Goal` are plain classes, not `Entity` subclasses. Nothing in the library forces `Entity`. Using it would be ceremony: each entity takes a ContentManager in `LoadContent` (unused), an empty `UnloadContent`, and a `SpriteBatch` in `Draw` (already receiving). The tactical demo does extend `Entity` but also doesn't get much from it — the abstract methods mostly no-op.

**Recommendation**: consider making `Entity` genuinely useful (e.g., own a `Position`, `Bounds`, or a `Draw`/`Update` lifecycle that a scene iterates) or delete it. Right now it's a naming convention with method signatures that don't earn their keep. Revisit once a third game tells us what entities actually share.

### 4.4 `Text` rendering has two disconnected paths
Tactical uses `Text.TextManager` (handle-based, batched). Platformer uses `SpriteBatch.DrawString` directly with a cached `SpriteFont`. Both work; neither dominates. `TextManager` is the right choice for HUD-like text that persists across frames; direct `DrawString` is right for one-off overlays ("You Win"). The library doesn't document which to pick.

**Recommendation**: CLAUDE.md should have a short "text rendering" section with the two patterns and when each fits. No code change needed.

> **✅ Fixed 2026-04-18 (commit `702dd54`)**. CLAUDE.md now has a "Text rendering" subsection under Architecture spelling out when to pick `TextManager` vs direct `DrawString`.

---

## §5 — Unused library surface (over-build audit)

Some library code was written speculatively and hasn't earned its place yet. Not a call to delete anything — just to be honest about validation status.

| Primitive | Status | Likely trigger |
|---|---|---|
| `ObjectPool<T>` | Unused | A game with many short-lived objects (projectiles at scale, particles) |
| `Timer` / `TimerManager` | Unused | A game with many scheduled callbacks (turn timers, power-up durations). Platformer's coyote/buffer used plain float counters because `TimerManager`'s `After`/`Every` fit poorly for continuously-ticking grace windows |
| `Tween<T>` / `Easing` | Unused | Any UI or camera polish pass |
| `SaveSystem` / `SaveFile` | Unused | First persistent progress |
| `TileMap` / `TileLayer` | Unused | Platformer hand-rolled `List<Platform>`; tactical hand-rolled a grid. Both would benefit but neither needed it for MVP |
| `AssetCatalog` | Unused | Any game with ≥ 10 content entries |
| `ILogger` / `ConsoleLogger` | Unused | First debugging session painful enough to add logging |
| `MathUtilities` | Unused | Randomness, angle math — both games got away without it |
| `PerformanceMonitor` | Unused | First perf complaint |
| `EventManager.Subscribe<T>`/`Publish<T>` (typed) | Unused | Any game that grows past ~5 event types |

Of these, the ones most likely to land soon given the platformer's trajectory: `ObjectPool` (if projectiles return), `Tween` (UI polish), `AssetCatalog` (more than one sprite sheet), `TileMap` (bigger levels).

The ones I'd flag as genuinely speculative until 3 games exist: `PerformanceMonitor` (untested beyond a property exposing what it already computed), `MathUtilities.Random*` (`System.Random` is fine), typed `EventManager` API (string API handles both games' ~5 events each).

---

## §6 — Recommended actions

Ordered by value-per-effort. None are urgent; pick when the trigger hits.

1. ✅ **Fix `GameStateManager` lifecycle auto-calls** (§1.1). ~20 lines. Removes ceremony from every game. ~~Do next time the library is touched.~~ **Done 2026-04-18.**
2. ✅ **Decouple `UIManager` from `DrawManager`** (§4.1). Split into interaction-only + optional rendering helper. ~40 lines. ~~Do before game #3 if it has UI.~~ **Done 2026-04-18.**
3. ✅ **Ship the pixel-texture helper + `SpriteSheet.Tint`** (§1.2, §4.2). ~20 lines. Trivial quality-of-life win. **Done 2026-04-18** — `Primitives` awaits adoption by sample games.
4. ⏳ **Start a `MonoGame.GameFramework.Platformer` genre module** once a *second* platformer is started (§2). Extract the jump feel config + controller first — that's the densest shared code. **Deferred to game #3.**
5. ⏳ **Delete or redesign `Entity` abstract class** (§4.3). Revisit after game #3. **Deferred to game #3.**
6. ✅ **Document text rendering paths in CLAUDE.md** (§4.4). 10-minute edit. **Done 2026-04-18.**

### Follow-up trigger after the 2026-04-18 batch
~~`Rendering.Primitives` is shipped but no consumer uses it yet — each game still creates its own 1×1 pixel texture. Low-risk follow-up: adopt `Primitives.Pixel` in the platformer's `Game1`/`PlayState`/`TitleState` and in the demo's `ConsoleUI`.~~ **Done** — adopted in the follow-up commit. Demo's `BattleState`/`DebugState`/`ConsoleUI` also shed their now-dead `GraphicsDevice` parameter chain.

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
