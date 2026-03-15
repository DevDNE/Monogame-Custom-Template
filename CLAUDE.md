# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A reusable MonoGame DesktopGL framework library (`MonoGame.GameFramework`) with a demo project (`MonoGame.GameFramework.Demo`) that showcases the framework. Uses Microsoft.Extensions.DependencyInjection for wiring managers together.

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
- **Serialization**: Newtonsoft.Json (settings, library only)
- **Config**: dotenv.net (demo only)
- **Optional**: Steamworks.NET (currently commented out in Game1.cs)

## Architecture

### Library (`MonoGame.GameFramework`)

**Namespace root**: `MonoGame.GameFramework`

**Managers** (`Managers/`) — all 12 singletons resolved via DI:
- Input: `KeyboardManager`, `MouseManager`, `GamePadManager`
- Rendering: `DrawManager`, `TextManager`, `UIManager`
- State: `GameStateManager`, `SceneManager`
- Other: `EventManager`, `SettingsManager`, `SoundManager`, `SteamworksManager`

**DI Registration**: `ServiceCollectionExtensions.AddGameFrameworkManagers()` registers all managers in one call. Accepts optional `settingsFilePath` parameter.

**Base classes**:
- `GameStates/GameState` — abstract state with `Entered/Leaving/Obscuring/Revealed/Update`
- `Scenes/GameScene` — abstract scene with `LoadContent/UnloadContent/Update`
- `Components/Entities/Entity` — abstract entity with `LoadContent/UnloadContent/Update`

**Other**: `Graphics/` (SpriteSheet, TextElement), `Events/` (GameEventArgs), `Engine/Physics/` (Collision), `Engine/Utilities/` (MathHelperExtensions), `Debugging/` (PerformanceMonitor), `Components/Entities/Tile`

### Demo (`MonoGame.GameFramework.Demo`)

**Namespace root**: `MonoGame.GameFramework.Demo`

**Entry point**: `Program.cs` loads dotenv, calls `AddGameFrameworkManagers()`, creates `Game1`.

**Game1** resolves managers from DI, runs the standard MonoGame lifecycle.

**Game-specific code**: `Components/Entities/` (Player, EnemyPlayer, Projectile, Gameboard, Character), `Components/UI/` (ConsoleUI, PlayerHealthUI), `GameStates/` (BattleState, DebugState), `Scenes/` (BattleScene, FirstScene), `Engine/Rules/` (GameRulesManager)

## Mac Setup Notes

Requires both x64 and ARM .NET SDKs. Also needs `brew install freetype freeimage` with symlinks to `/usr/local/lib/` (see README.md for exact paths).
