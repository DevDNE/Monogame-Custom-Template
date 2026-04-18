# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A reusable MonoGame DesktopGL framework library (`MonoGame.GameFramework`) with two sample games in different genres (`MonoGame.GameFramework.BattleGrid` — grid-duel; `MonoGame.GameFramework.Platformer` — side-scroller) that showcase it. Uses Microsoft.Extensions.DependencyInjection for wiring services together.

## Solution Structure

```
Game.sln
src/
  MonoGame.GameFramework/             ← Class library (reusable framework)
  MonoGame.GameFramework.BattleGrid/  ← Sample game: grid-based duel
  MonoGame.GameFramework.Platformer/  ← Sample game: side-scroller
  MonoGame.GameFramework.Tests/       ← xUnit tests for the library
```

## Build & Run

```bash
dotnet build Game.sln                                                                              # Build all projects
dotnet test  Game.sln                                                                              # Run the 99 library tests
dotnet run --project src/MonoGame.GameFramework.BattleGrid/MonoGame.GameFramework.BattleGrid.csproj   # Run the grid-duel sample
dotnet run --project src/MonoGame.GameFramework.Platformer/MonoGame.GameFramework.Platformer.csproj   # Run the platformer sample
dotnet restore                                                                                     # Restore NuGet packages
```

Each sample game has its own `Content/Content.mgcb`. Both currently reference only a single font (`fonts/Arial.spritefont`) — all entities render as colored rectangles via `Rendering.Primitives`, so no sprite atlases are needed. Edit a content file with:
```bash
dotnet mgcb-editor ./src/MonoGame.GameFramework.BattleGrid/Content/Content.mgcb
```

## Tech Stack

- **Framework**: MonoGame 3.8.4.1 (DesktopGL)
- **Target**: .NET 9.0
- **DI**: Microsoft.Extensions.DependencyInjection 9.0.0
- **Serialization**: Newtonsoft.Json (library only — used by SaveSystem and SettingsManager)
- **Config**: dotenv.net (demo only)

## Architecture

### Library (`MonoGame.GameFramework`)

**Namespace root**: `MonoGame.GameFramework`

The library is organized into 15 domain folders, each with a matching namespace. Services are registered by `Core/ServiceCollectionExtensions.AddGameFrameworkManagers()` and resolved via DI.

| Folder | Namespace | Contents |
|---|---|---|
| `Audio/` | `MonoGame.GameFramework.Audio` | `SoundManager` |
| `Content/` | `MonoGame.GameFramework.Content` | `AssetCatalog` |
| `Core/` | `MonoGame.GameFramework.Core` | `Entity`, `ServiceCollectionExtensions` |
| `Debugging/` | `MonoGame.GameFramework.Debugging` | `ILogger`, `ConsoleLogger`, `PerformanceMonitor` |
| `Events/` | `MonoGame.GameFramework.Events` | `EventManager` (string-keyed + typed `Subscribe<T>`/`Publish<T>`), `GameEventArgs` |
| `Input/` | `MonoGame.GameFramework.Input` | `KeyboardManager`, `MouseManager`, `GamePadManager` |
| `Lifecycle/` | `MonoGame.GameFramework.Lifecycle` | `GameState` + `GameStateManager`, `GameScene` + `SceneManager` |
| `Persistence/` | `MonoGame.GameFramework.Persistence` | `SaveSystem`, `SaveFile<T>`, `SettingsManager` |
| `Pooling/` | `MonoGame.GameFramework.Pooling` | `ObjectPool<T>` |
| `Rendering/` | `MonoGame.GameFramework.Rendering` | `DrawManager`, `SpriteSheet`, `Camera2D`, `TileMap`, `TileLayer<T>` |
| `Text/` | `MonoGame.GameFramework.Text` | `TextManager` (handle-based), `TextElement`, `TextHandle` |
| `Timing/` | `MonoGame.GameFramework.Timing` | `Timer`, `TimerManager` (`After`/`Every`/`Over`) |
| `Tween/` | `MonoGame.GameFramework.Tween` | `Tween<T>` + `Tween.Float/Vec2/Color` factories, `Easing` |
| `UI/` | `MonoGame.GameFramework.UI` | `UIManager` (hit-testing, focus, click handlers) |
| `Utilities/` | `MonoGame.GameFramework.Utilities` | `MathUtilities` (`Angle`, `RandomFloat`, `RandomInt`, `RandomVector2`) |

**Base classes** (all in `Lifecycle/` or `Core/`):
- `Core/Entity` — abstract entity with `LoadContent`/`UnloadContent`/`Update`, plus virtual `Draw` with empty default.
- `Lifecycle/GameState` — stack-based state with `Entered`/`Leaving`/`Obscuring`/`Revealed`/`Update`/virtual `Draw`.
- `Lifecycle/GameScene` — scene with `LoadContent`/`UnloadContent`/`Update`/virtual `Draw`.

**SpriteSheet construction**: use the factories, not the old 8-arg ctor.
- `SpriteSheet.Static(texture, destinationFrame, sourceFrame: ..., name: ...)` — single-frame.
- `SpriteSheet.Animated(texture, frames, destinationFrame, frameInterval, name: ..., startFrame: ...)` — multi-frame.
- `SpriteSheet.Tint` is mutable (defaults to `Color.White`); `DrawManager` respects it, so runtime tint/flash/fade works without replacing the sprite.

**Pixel texture**: call `Rendering.Primitives.Initialize(GraphicsDevice)` once in `Game1.LoadContent`, then use `Primitives.Pixel` or `Primitives.DrawRectangle(spriteBatch, rect, color)` anywhere a solid-color rectangle is needed. Avoids re-creating 1×1 textures in every entity.

**GameStateManager lifecycle**: `PushState`, `PopState`, and `ChangeState` automatically call the right hooks — `Entered`, `Leaving`, `Obscuring`, `Revealed`. Consumers should not invoke these manually after a transition. Initial push fires `Entered` only (nothing to reveal from).

**Text rendering** — the library offers two paths, by design:
- **`Text.TextManager`** — handle-based, batched text with group management (`ClearGroup`, `ScrollText`). Use for HUD text that persists frame-to-frame: score counters, chat logs, status labels. The tactical demo's enemy HP counter and console overlay use this.
- **Direct `SpriteBatch.DrawString(font, ...)`** — one-off labels drawn inline in a `GameState.Draw` override. Use for title screens, win/lose banners, button labels, anything whose position is computed per-frame. The platformer's "You Win!" overlay and title buttons use this.

Rule of thumb: if you'd register it and mutate occasionally, use `TextManager`. If you'd recompute position every frame, use `DrawString`.

### Sample games

Two sample games live in `src/` next to the library. Both are playable end-to-end and exist to validate the library against different genres.

#### `MonoGame.GameFramework.BattleGrid`
Grid-based duel inspired by Mega Man Battle Network. 3×3 grid per side, player on the left (blue), enemy on the right (red), all drawn as colored rectangles via `Rendering.Primitives`.

- **Controls**: WASD to move on the player grid; Space to fire a buster shot; Tab to open chip selection; number keys `1`/`2`/`3` pick a chip; R restarts; Esc quits.
- **Enemy AI**: `EnemyPlayer.Update` alternates every 1.5 s between moving to a random cell and firing one of two rotating patterns (single-row shot vs. wide shot across all rows).
- **Chip system**: `PlayState` holds a `Mode` enum (`Playing`/`SelectingChip`/`Won`/`Lost`). Tab switches to `SelectingChip`, which pauses the scene update but keeps drawing the world behind an overlay. Four chips in the pool: Cannon (40 dmg), Wide Shot (15×3), Sword (30 same-row), Recov (heal 30). 3 s cooldown after use.
- **HUD**: `PlayState.DrawHud` renders HP bars with numeric labels, a controls hint strip at the bottom, and a chip-ready / cooldown indicator at top center.
- **File layout**:
  - `Game1.cs` — thin shell; resolves managers, initializes `Primitives`, pushes `TitleState`.
  - `BattleConfig.cs` — grid constants (tile size, board origins, default projectile damage).
  - `Grid.cs` — static (col, row) → pixel coordinate helpers.
  - `GameStates/` — `TitleState`, `PlayState`, `DebugState` (tilde-toggled console overlay).
  - `Scenes/BattleScene.cs` — owns `Player`, `EnemyPlayer`, `Gameboard`.
  - `Components/Entities/` — `Player`, `EnemyPlayer`, `Projectile`, `Gameboard`.
  - `Components/UI/ConsoleUI.cs` — debug log overlay.
  - `Engine/Rules/GameRulesManager.cs` — grid boundary validation helpers.

#### `MonoGame.GameFramework.Platformer`
Side-scrolling platformer. Colored-rectangle player, platforms, patrolling enemy, green goal.

- **Controls**: A/D or arrow keys to move; Space to jump (hold for higher jump); R to respawn; Esc to quit.
- **Physics**: gravity + horizontal acceleration in `Player.Update`; separate-axis AABB resolution against a `List<Platform>`. Coyote time 150 ms, jump buffer 150 ms, variable-height jump via `JumpCutVelocity`.
- **Camera**: `Rendering.Camera2D` with `Target` follow and lerp; `PlayState.Respawn` snaps the camera to the player to avoid a long pan.
- **Win/Lose**: death plane at y=800 triggers respawn; touching the green goal enters `_won` state with "Press R to play again" overlay.
- **File layout**:
  - `Game1.cs` — thin shell; `Primitives.Initialize`; pushes `TitleState`.
  - `GameStates/` — `TitleState`, `PlayState`.
  - `Entities/` — `Player`, `Platform`, `Enemy`, `Goal`.
  - `Content/fonts/Arial.spritefont`.

## Mac Setup Notes

Requires both x64 and ARM .NET SDKs. Also needs `brew install freetype freeimage` with symlinks to `/usr/local/lib/` (see README.md for exact paths).
