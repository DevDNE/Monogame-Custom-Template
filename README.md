# Monogame-Custom-Template

## monogame-vscode-boilerplate

Use the MonoGame C# configuration for Visual Studio Code
See https://github.com/rewrking/monogame-vscode-boilerplate for instructions and downloads

## Setting Up Mac Environment:
- get both x64 and ARM .net (not optional)
- follow this closely, you need all of the vs code extensions (on vscode):
  - https://docs.monogame.net/articles/getting_started/1_setting_up_your_development_environment_unix.html
- brew install freetype
- brew install freeimage

You need to make 2 files for the the 2 previous commands
- for freetype on mac, first change: 
  - /opt/homebrew/lib/libfreetype.6.dylib -> 
  - /opt/homebrew/lib/libfreetype6.dylib
  - then run
  - sudo ln -s /opt/homebrew/lib/libfreetype6.dylib /usr/local/lib/libfreetype6
  - or
  - sudo ln -s /opt/homebrew/Cellar/freetype/2.13.2/lib/libfreetype.6.dylib /usr/local/lib/libfreetype6
  - based on location
- for freeimage on mac, run:
  - sudo ln -s /opt/homebrew/Cellar/freeimage/3.18.0/lib/libfreeimage.dylib /usr/local/lib/libfreeimage
- command to get editor open:
  - dotnet mgcb-editor ./Content/Content.mgcb (doesn't seem to be working rn with arm over x64)
- use content.mgcb, right click, and click in mgcb editor
- launch game (for now) by right clicking project, then go to debug, then start a new instance in solution explorer

## Project Structure

```
src/
  MonoGame.GameFramework/          ← Class library (reusable framework)
  MonoGame.GameFramework.Demo/     ← Executable demo consuming the library
```

### Library (`MonoGame.GameFramework`)

15 domain folders, each with a matching namespace. All services are registered via `Core/ServiceCollectionExtensions.AddGameFrameworkManagers()`.

| Folder | Contents |
|---|---|
| `Audio/` | `SoundManager` |
| `Content/` | `AssetCatalog` |
| `Core/` | `Entity`, `ServiceCollectionExtensions` |
| `Debugging/` | `ILogger`, `ConsoleLogger`, `PerformanceMonitor` |
| `Events/` | `EventManager` (string-keyed + typed `Subscribe<T>`/`Publish<T>`), `GameEventArgs` |
| `Input/` | `KeyboardManager`, `MouseManager`, `GamePadManager` |
| `Lifecycle/` | `GameState` + `GameStateManager`, `GameScene` + `SceneManager` |
| `Persistence/` | `SaveSystem`, `SaveFile<T>`, `SettingsManager` |
| `Pooling/` | `ObjectPool<T>` |
| `Rendering/` | `DrawManager`, `SpriteSheet`, `Camera2D`, `TileMap`, `TileLayer<T>` |
| `Text/` | `TextManager` (handle-based), `TextElement`, `TextHandle` |
| `Timing/` | `Timer`, `TimerManager` (`After`/`Every`/`Over`) |
| `Tween/` | `Tween<T>`, `Easing` |
| `UI/` | `UIManager` (hit-testing, focus, click handlers) |
| `Utilities/` | `MathUtilities` |

### Demo (`MonoGame.GameFramework.Demo`)

- `Program.cs` — entry point; loads dotenv, builds DI container, runs `Game1`.
- `Game1.cs` — resolves managers from DI and orchestrates the MonoGame lifecycle.
- `Components/Entities/` — `Player`, `EnemyPlayer`, `Projectile`, `Gameboard`.
- `Components/UI/` — `ConsoleUI`, `PlayerHealthUI`.
- `GameStates/` — `BattleState`, `DebugState`.
- `Scenes/` — `BattleScene`.
- `Engine/Rules/` — `GameRulesManager`.
