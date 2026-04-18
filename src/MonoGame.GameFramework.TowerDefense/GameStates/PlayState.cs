using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.GameFramework.Input;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Pooling;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.Timing;
using MonoGame.GameFramework.TowerDefense.Entities;

namespace MonoGame.GameFramework.TowerDefense.GameStates;

public class PlayState : GameState
{
  private const int StartGold = 40;
  private const int StartLives = 5;
  private const int GoldPerKill = 5;
  private const int GoldPerWaveClear = 15;
  private const float SpawnInterval = 0.8f;
  private const float BetweenWavesCooldown = 5f;
  private static readonly int[] WaveSizes = { 5, 8, 12 };

  private enum Status { Playing, Victory, Defeat }

  private readonly KeyboardManager _keyboard;
  private readonly MouseManager _mouse;
  private readonly TimerManager _timers;
  private readonly SpriteFont _font;
  private readonly int _viewportWidth;
  private readonly int _viewportHeight;

  private MapPath.TileMapOrigin _origin;
  private HashSet<(int col, int row)> _pathCells = new();
  private readonly Dictionary<(int col, int row), Tower> _towers = new();
  private readonly ObjectPool<Enemy> _enemyPool = new(() => new Enemy(), prewarm: 16);
  private readonly ObjectPool<Projectile> _projectilePool = new(() => new Projectile(), prewarm: 32);
  private readonly List<Enemy> _liveEnemies = new();
  private readonly List<Projectile> _liveProjectiles = new();

  private int _gold = StartGold;
  private int _lives = StartLives;
  private int _waveIndex;              // 0 = pre-wave 1
  private int _enemiesToSpawnInWave;
  private float _intermissionRemaining;
  private Status _status = Status.Playing;

  public PlayState(ServiceProvider sp, SpriteFont font, int vw, int vh)
  {
    _keyboard = sp.GetService<KeyboardManager>();
    _mouse = sp.GetService<MouseManager>();
    _timers = sp.GetService<TimerManager>();
    _font = font;
    _viewportWidth = vw;
    _viewportHeight = vh;
  }

  public override void Entered()
  {
    StartFresh();
    IsActive = true;
  }

  public override void Leaving()
  {
    ClearLiveObjects();
    _towers.Clear();
    _timers.Clear();
  }

  public override void Obscuring() => IsActive = false;
  public override void Revealed() => IsActive = true;

  private void StartFresh()
  {
    ClearLiveObjects();
    _towers.Clear();
    _timers.Clear();
    _gold = StartGold;
    _lives = StartLives;
    _status = Status.Playing;
    _waveIndex = 0;
    _intermissionRemaining = 3f;

    float mapW = MapPath.Columns * MapPath.CellSize;
    float mapH = MapPath.Rows * MapPath.CellSize;
    _origin = new MapPath.TileMapOrigin(
      (_viewportWidth - mapW) * 0.5f,
      (_viewportHeight - mapH) * 0.5f + 12f);
    _pathCells = MapPath.GetPathCells();
  }

  private void ClearLiveObjects()
  {
    foreach (Enemy e in _liveEnemies) _enemyPool.Return(e);
    foreach (Projectile p in _liveProjectiles) _projectilePool.Return(p);
    _liveEnemies.Clear();
    _liveProjectiles.Clear();
  }

  public override void Update(GameTime gameTime)
  {
    if (_keyboard.WasKeyPressed(Keys.R)) { StartFresh(); return; }
    if (_status != Status.Playing) return;

    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
    _timers.Update(gameTime);

    HandlePlacement();
    TickWaveLifecycle(dt);
    UpdateEnemies(dt);
    UpdateTowers(dt);
    UpdateProjectiles(dt);
    CheckOutcome();
  }

  private void HandlePlacement()
  {
    if (!_mouse.WasLeftMouseButtonPressed()) return;
    Vector2 m = _mouse.GetMousePosition();
    int col = (int)((m.X - _origin.X) / MapPath.CellSize);
    int row = (int)((m.Y - _origin.Y) / MapPath.CellSize);
    if (col < 0 || col >= MapPath.Columns || row < 0 || row >= MapPath.Rows) return;
    if (_pathCells.Contains((col, row))) return;
    if (_towers.ContainsKey((col, row))) return;
    if (_gold < Tower.Cost) return;

    Vector2 pos = MapPath.WorldPosition(_origin, col, row);
    _towers[(col, row)] = new Tower(col, row, pos);
    _gold -= Tower.Cost;
  }

  private void TickWaveLifecycle(float dt)
  {
    if (_intermissionRemaining > 0f)
    {
      _intermissionRemaining -= dt;
      if (_intermissionRemaining <= 0f) StartNextWave();
      return;
    }
    if (_enemiesToSpawnInWave == 0 && _liveEnemies.Count == 0)
    {
      // Wave complete
      _gold += GoldPerWaveClear;
      _intermissionRemaining = BetweenWavesCooldown;
    }
  }

  private void StartNextWave()
  {
    if (_waveIndex >= WaveSizes.Length)
    {
      // All waves survived = victory
      if (_liveEnemies.Count == 0) _status = Status.Victory;
      return;
    }
    _enemiesToSpawnInWave = WaveSizes[_waveIndex];
    _waveIndex++;
    _timers.Every(SpawnInterval, TrySpawnEnemy);
  }

  private void TrySpawnEnemy()
  {
    if (_enemiesToSpawnInWave <= 0) return;
    (int c, int r) = MapPath.Waypoints[0];
    Vector2 spawn = MapPath.WorldPosition(_origin, c, r);
    Enemy e = _enemyPool.Rent();
    e.Spawn(spawn);
    _liveEnemies.Add(e);
    _enemiesToSpawnInWave--;
  }

  private void UpdateEnemies(float dt)
  {
    for (int i = _liveEnemies.Count - 1; i >= 0; i--)
    {
      Enemy e = _liveEnemies[i];
      e.Update(dt, _origin);
      if (e.Leaked)
      {
        _lives--;
        _enemyPool.Return(e);
        _liveEnemies.RemoveAt(i);
      }
      else if (!e.Alive)
      {
        _gold += GoldPerKill;
        _enemyPool.Return(e);
        _liveEnemies.RemoveAt(i);
      }
    }
  }

  private void UpdateTowers(float dt)
  {
    foreach (Tower t in _towers.Values)
    {
      Enemy target = t.TryFire(dt, _liveEnemies);
      if (target != null)
      {
        Projectile p = _projectilePool.Rent();
        p.Launch(t.Position, target);
        _liveProjectiles.Add(p);
      }
    }
  }

  private void UpdateProjectiles(float dt)
  {
    for (int i = _liveProjectiles.Count - 1; i >= 0; i--)
    {
      Projectile p = _liveProjectiles[i];
      p.Update(dt);
      if (!p.Alive)
      {
        _projectilePool.Return(p);
        _liveProjectiles.RemoveAt(i);
      }
    }
  }

  private void CheckOutcome()
  {
    if (_lives <= 0) { _status = Status.Defeat; return; }
    if (_waveIndex >= WaveSizes.Length && _enemiesToSpawnInWave == 0 && _liveEnemies.Count == 0 && _intermissionRemaining <= 0f)
    {
      _status = Status.Victory;
    }
  }

  public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
  {
    spriteBatch.Begin();
    DrawMap(spriteBatch);
    foreach (Tower t in _towers.Values) t.Draw(spriteBatch);
    foreach (Enemy e in _liveEnemies) e.Draw(spriteBatch);
    foreach (Projectile p in _liveProjectiles) p.Draw(spriteBatch);
    DrawHud(spriteBatch);
    if (_status != Status.Playing) DrawOutcome(spriteBatch);
    spriteBatch.End();
  }

  private void DrawMap(SpriteBatch spriteBatch)
  {
    Primitives.DrawRectangle(spriteBatch, new Rectangle(0, 0, _viewportWidth, _viewportHeight), new Color(18, 22, 34));
    Color buildable = new(46, 56, 78);
    Color path = new(120, 100, 70);
    for (int r = 0; r < MapPath.Rows; r++)
    {
      for (int c = 0; c < MapPath.Columns; c++)
      {
        Rectangle rect = new(
          (int)_origin.X + c * MapPath.CellSize + 1,
          (int)_origin.Y + r * MapPath.CellSize + 1,
          MapPath.CellSize - 2, MapPath.CellSize - 2);
        Color fill = _pathCells.Contains((c, r)) ? path : buildable;
        Primitives.DrawRectangle(spriteBatch, rect, fill);
      }
    }
  }

  private void DrawHud(SpriteBatch spriteBatch)
  {
    spriteBatch.DrawString(_font, $"Gold {_gold}",  new Vector2(20, 12), new Color(255, 230, 120));
    spriteBatch.DrawString(_font, $"Lives {_lives}", new Vector2(160, 12), new Color(230, 120, 120));
    string wave = _waveIndex == 0
      ? $"Next wave in {_intermissionRemaining:0.0}s"
      : _intermissionRemaining > 0f
        ? $"Wave {_waveIndex}/{WaveSizes.Length} cleared. Next in {_intermissionRemaining:0.0}s"
        : $"Wave {_waveIndex}/{WaveSizes.Length}";
    spriteBatch.DrawString(_font, wave, new Vector2(320, 12), Color.White);

    const string hint = "Click grid cell to place tower (20g)   R restart   Esc quit";
    Vector2 hs = _font.MeasureString(hint);
    spriteBatch.DrawString(_font, hint, new Vector2(_viewportWidth * 0.5f - hs.X * 0.5f, _viewportHeight - 28), new Color(180, 180, 200));
  }

  private void DrawOutcome(SpriteBatch spriteBatch)
  {
    Primitives.DrawRectangle(spriteBatch, new Rectangle(0, 0, _viewportWidth, _viewportHeight), new Color(0, 0, 0, 180));
    string heading = _status == Status.Victory ? "Victory!" : "Defeat";
    Color color = _status == Status.Victory ? new Color(120, 220, 140) : new Color(230, 90, 100);
    const string hint = "Press R to play again";
    Vector2 hs = _font.MeasureString(heading);
    Vector2 ih = _font.MeasureString(hint);
    Vector2 c = new(_viewportWidth * 0.5f, _viewportHeight * 0.5f);
    spriteBatch.DrawString(_font, heading, new Vector2(c.X - hs.X * 0.5f, c.Y - 30), color);
    spriteBatch.DrawString(_font, hint, new Vector2(c.X - ih.X * 0.5f, c.Y + 10), new Color(210, 210, 220));
  }
}
