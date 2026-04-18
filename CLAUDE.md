# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A reusable MonoGame DesktopGL framework library (`MonoGame.GameFramework`) with a demo project (`MonoGame.GameFramework.Demo`) that showcases the framework. Uses Microsoft.Extensions.DependencyInjection for wiring services together.

## Solution Structure

```
Game.sln
src/
  MonoGame.GameFramework/          ← Class library (reusable framework)
  MonoGame.GameFramework.Demo/     ← Executable (demo game consuming the library)
```

## Build & Run

```bash
dotnet build Game.sln                                                                    # Build both projects
dotnet run --project src/MonoGame.GameFramework.Demo/MonoGame.GameFramework.Demo.csproj   # Run the demo
dotnet restore                                                                           # Restore NuGet packages
```

Content pipeline (sprites, fonts, sounds) is managed via `src/MonoGame.GameFramework.Demo/Content/Content.mgcb`. Edit with:
```bash
dotnet mgcb-editor ./src/MonoGame.GameFramework.Demo/Content/Content.mgcb
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

### Demo (`MonoGame.GameFramework.Demo`)

**Namespace root**: `MonoGame.GameFramework.Demo`

**Entry point**: `Program.cs` loads dotenv, calls `AddGameFrameworkManagers()`, creates `Game1`.

**Game1** resolves managers from DI, runs the standard MonoGame lifecycle. `Game1.Update` order: input managers → `UIManager.Update` → `GameStateManager.Update`. `Game1.Draw` order: `DrawManager.Draw` (registered sprites) → `GameStateManager.Draw` (state/scene/entity custom draw) → `TextManager.Draw`.

**Game-specific code**:
- `Components/Entities/` — `Player`, `EnemyPlayer`, `Projectile`, `Gameboard`
- `Components/UI/` — `ConsoleUI`, `PlayerHealthUI`
- `GameStates/` — `BattleState`, `DebugState`
- `Scenes/` — `BattleScene`
- `Engine/Rules/` — `GameRulesManager`

## Mac Setup Notes

Requires both x64 and ARM .NET SDKs. Also needs `brew install freetype freeimage` with symlinks to `/usr/local/lib/` (see README.md for exact paths).
