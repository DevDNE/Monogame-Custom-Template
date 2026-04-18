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
  MonoGame.GameFramework/             ← Class library (reusable framework)
  MonoGame.GameFramework.BattleGrid/  ← Sample game: grid-based duel
  MonoGame.GameFramework.Platformer/  ← Sample game: side-scroller
  MonoGame.GameFramework.Tests/       ← xUnit tests for the library
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
| `Rendering/` | `DrawManager`, `SpriteSheet`, `Camera2D`, `TileMap`, `TileLayer<T>`, `Primitives` |
| `Text/` | `TextManager` (handle-based), `TextElement`, `TextHandle` |
| `Timing/` | `Timer`, `TimerManager` (`After`/`Every`/`Over`) |
| `Tween/` | `Tween<T>`, `Easing` |
| `UI/` | `UIManager` (hit-testing, focus, click handlers) |
| `Utilities/` | `MathUtilities` |

### Sample: `MonoGame.GameFramework.BattleGrid`

Grid-based duel inspired by Mega Man Battle Network. Player (blue, left 3×3) vs. enemy (red, right 3×3), all colored rectangles. WASD to move, Space to shoot, Tab opens a chip-selection overlay with three random chips from a pool (Cannon / Wide Shot / Sword / Recov), R restarts, Esc quits. Enemy AI alternates between movement and one of two attack patterns (single-row or wide-across-all-rows). HP bars, controls hint, and chip-ready indicator are drawn as HUD. Tilde toggles a debug console overlay.

### Sample: `MonoGame.GameFramework.Platformer`

Side-scrolling platformer. Colored-rectangle player with gravity, variable-height jump, coyote time, and jump buffer. Separate-axis AABB collision against a list of platforms. Patrolling red enemy, green goal, death plane. Camera follows the player with lerp and snaps on respawn. Title screen with Play/Quit buttons; win overlay on touching the goal. A/D or arrows to move, Space to jump, R to respawn, Esc to quit.

### Tests (`MonoGame.GameFramework.Tests`)

99 xUnit tests with FluentAssertions covering pure-logic library pieces: `ObjectPool`, `Timer`/`TimerManager`, `Tween`/`Easing`, `MathUtilities`, `TileMap`/`TileLayer`, `EventManager` (string + typed), `SaveSystem`, `Camera2D` math, `GameStateManager` lifecycle, `UIManager` independence, `SpriteSheet.Tint`. Rendering-dependent code (SpriteBatch/SpriteFont/GraphicsDevice) is smoke-tested via the two sample games rather than unit tests.
