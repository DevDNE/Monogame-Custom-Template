using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.GameFramework.Input;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Pooling;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.Shooter.Entities;
using MonoGame.GameFramework.Timing;

namespace MonoGame.GameFramework.Shooter.GameStates;

public class PlayState : GameState
{
  private const int ArenaWidth = 2400;
  private const int ArenaHeight = 1600;
  private const float EnemySpawnInterval = 1.5f;
  private const int EnemiesPerSpawn = 2;

  private readonly KeyboardManager _keyboard;
  private readonly MouseManager _mouse;
  private readonly TimerManager _timers;
  private readonly SpriteFont _font;
  private readonly int _viewportWidth;
  private readonly int _viewportHeight;
  private readonly Random _random = new();

  private readonly Player _player = new();
  private readonly ObjectPool<Projectile> _projectilePool;
  private readonly ObjectPool<Enemy> _enemyPool;
  private readonly List<Projectile> _liveProjectiles = new();
  private readonly List<Enemy> _liveEnemies = new();

  private Camera2D _camera;
  private int _score;
  private bool _gameOver;

  public PlayState(ServiceProvider serviceProvider, SpriteFont font, int viewportWidth, int viewportHeight)
  {
    _keyboard = serviceProvider.GetService<KeyboardManager>();
    _mouse = serviceProvider.GetService<MouseManager>();
    _timers = serviceProvider.GetService<TimerManager>();
    _font = font;
    _viewportWidth = viewportWidth;
    _viewportHeight = viewportHeight;

    _projectilePool = new ObjectPool<Projectile>(
      () => new Projectile(),
      prewarm: 32,
      onReturn: p => p.Alive = false);
    _enemyPool = new ObjectPool<Enemy>(
      () => new Enemy(),
      prewarm: 16,
      onReturn: e => e.Alive = false);
  }

  public override void Entered()
  {
    StartFresh();
    IsActive = true;
  }

  public override void Leaving()
  {
    foreach (Projectile p in _liveProjectiles) _projectilePool.Return(p);
    foreach (Enemy e in _liveEnemies) _enemyPool.Return(e);
    _liveProjectiles.Clear();
    _liveEnemies.Clear();
    _timers.Clear();
  }

  public override void Obscuring() => IsActive = false;
  public override void Revealed() => IsActive = true;

  private void StartFresh()
  {
    _liveProjectiles.Clear();
    _liveEnemies.Clear();
    _timers.Clear();
    _score = 0;
    _gameOver = false;
    _player.Spawn(new Vector2(ArenaWidth * 0.5f, ArenaHeight * 0.5f));
    _camera = new Camera2D(new Vector2(_viewportWidth, _viewportHeight))
    {
      Position = _player.Position,
      Target = _player.Position,
      FollowLerp = 0.15f,
    };
    _timers.Every(EnemySpawnInterval, SpawnWave);
  }

  public override void Update(GameTime gameTime)
  {
    if (_keyboard.WasKeyPressed(Keys.R)) { Leaving(); StartFresh(); return; }
    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
    _timers.Update(gameTime);

    if (!_gameOver)
    {
      _player.Update(dt, _keyboard);
      HandleFiring();
      UpdateProjectiles(dt);
      UpdateEnemies(dt);
      CheckCollisions();
      if (!_player.IsAlive) _gameOver = true;
    }

    _camera.Target = _player.Position;
    _camera.Update(gameTime);
  }

  private void HandleFiring()
  {
    bool firePressed = _mouse.IsLeftMouseButtonDown() || _keyboard.IsKeyDown(Keys.Space);
    if (!firePressed) return;
    if (!_player.TryConsumeFireCooldown()) return;

    Vector2 mouseScreen = _mouse.GetMousePosition();
    Vector2 mouseWorld = _camera.ScreenToWorld(mouseScreen);
    Vector2 dir = mouseWorld - _player.Position;
    if (dir.LengthSquared() < 0.01f) dir = new Vector2(1, 0);
    else dir.Normalize();

    Projectile p = _projectilePool.Rent();
    p.Launch(_player.Position, dir);
    _liveProjectiles.Add(p);
  }

  private void UpdateProjectiles(float dt)
  {
    for (int i = _liveProjectiles.Count - 1; i >= 0; i--)
    {
      _liveProjectiles[i].Update(dt);
      if (!_liveProjectiles[i].Alive)
      {
        _projectilePool.Return(_liveProjectiles[i]);
        _liveProjectiles.RemoveAt(i);
      }
    }
  }

  private void UpdateEnemies(float dt)
  {
    for (int i = _liveEnemies.Count - 1; i >= 0; i--)
    {
      _liveEnemies[i].Update(dt, _player.Position);
      if (!_liveEnemies[i].Alive)
      {
        _enemyPool.Return(_liveEnemies[i]);
        _liveEnemies.RemoveAt(i);
      }
    }
  }

  private void CheckCollisions()
  {
    // projectile vs enemy
    for (int pi = _liveProjectiles.Count - 1; pi >= 0; pi--)
    {
      Projectile p = _liveProjectiles[pi];
      if (!p.Alive) continue;
      for (int ei = _liveEnemies.Count - 1; ei >= 0; ei--)
      {
        Enemy e = _liveEnemies[ei];
        if (!e.Alive) continue;
        if (p.Bounds.Intersects(e.Bounds))
        {
          p.Alive = false;
          e.Alive = false;
          _score += 10;
          break;
        }
      }
    }

    // enemy vs player
    for (int ei = 0; ei < _liveEnemies.Count; ei++)
    {
      Enemy e = _liveEnemies[ei];
      if (!e.Alive) continue;
      if (e.Bounds.Intersects(_player.Bounds))
      {
        _player.TryDamage(Enemy.ContactDamage);
      }
    }
  }

  private void SpawnWave()
  {
    if (_gameOver) return;
    for (int i = 0; i < EnemiesPerSpawn; i++)
    {
      // Spawn just outside the current viewport, on a random edge.
      Vector2 camPos = _camera.Position;
      float half = _viewportWidth * 0.5f + 40;
      float halfY = _viewportHeight * 0.5f + 40;
      Vector2 pos = _random.Next(4) switch
      {
        0 => new Vector2(camPos.X - half, camPos.Y + (float)(_random.NextDouble() - 0.5) * _viewportHeight),
        1 => new Vector2(camPos.X + half, camPos.Y + (float)(_random.NextDouble() - 0.5) * _viewportHeight),
        2 => new Vector2(camPos.X + (float)(_random.NextDouble() - 0.5) * _viewportWidth, camPos.Y - halfY),
        _ => new Vector2(camPos.X + (float)(_random.NextDouble() - 0.5) * _viewportWidth, camPos.Y + halfY),
      };
      pos.X = Math.Clamp(pos.X, Enemy.Size, ArenaWidth - Enemy.Size);
      pos.Y = Math.Clamp(pos.Y, Enemy.Size, ArenaHeight - Enemy.Size);
      Enemy e = _enemyPool.Rent();
      e.Spawn(pos);
      _liveEnemies.Add(e);
    }
  }

  public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
  {
    // World pass
    spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);
    DrawArena(spriteBatch);
    foreach (Enemy e in _liveEnemies) e.Draw(spriteBatch);
    foreach (Projectile p in _liveProjectiles) p.Draw(spriteBatch);
    _player.Draw(spriteBatch);
    spriteBatch.End();

    // Screen-space HUD
    spriteBatch.Begin();
    DrawHud(spriteBatch);
    if (_gameOver) DrawGameOver(spriteBatch);
    spriteBatch.End();
  }

  private static void DrawArena(SpriteBatch spriteBatch)
  {
    // Simple checker-gray background to show camera motion.
    const int tile = 80;
    Color a = new(30, 35, 50);
    Color b = new(38, 44, 60);
    for (int y = 0; y < ArenaHeight; y += tile)
      for (int x = 0; x < ArenaWidth; x += tile)
      {
        Color c = (((x / tile) + (y / tile)) % 2 == 0) ? a : b;
        Primitives.DrawRectangle(spriteBatch, new Rectangle(x, y, tile, tile), c);
      }
    // Arena border
    Color border = new(90, 100, 130);
    Primitives.DrawRectangle(spriteBatch, new Rectangle(0, 0, ArenaWidth, 4), border);
    Primitives.DrawRectangle(spriteBatch, new Rectangle(0, ArenaHeight - 4, ArenaWidth, 4), border);
    Primitives.DrawRectangle(spriteBatch, new Rectangle(0, 0, 4, ArenaHeight), border);
    Primitives.DrawRectangle(spriteBatch, new Rectangle(ArenaWidth - 4, 0, 4, ArenaHeight), border);
  }

  private void DrawHud(SpriteBatch spriteBatch)
  {
    // HP bar top-left
    Rectangle hpBg = new(20, 20, 240, 22);
    Primitives.DrawRectangle(spriteBatch, hpBg, new Color(25, 30, 45));
    int fillW = (int)(hpBg.Width * (_player.Hp / (float)Player.MaxHp));
    Primitives.DrawRectangle(spriteBatch, new Rectangle(hpBg.X, hpBg.Y, fillW, hpBg.Height), new Color(80, 200, 140));
    spriteBatch.DrawString(_font, $"HP {_player.Hp}", new Vector2(20, 46), Color.White);

    // Score top-right
    string score = $"Score {_score}";
    Vector2 sz = _font.MeasureString(score);
    spriteBatch.DrawString(_font, score, new Vector2(_viewportWidth - sz.X - 20, 20), Color.White);

    // Hint bottom
    const string hint = "WASD move   Mouse aim   Click / Space fire   R restart   Esc quit";
    Vector2 hSz = _font.MeasureString(hint);
    spriteBatch.DrawString(_font, hint, new Vector2(_viewportWidth * 0.5f - hSz.X * 0.5f, _viewportHeight - 30), new Color(200, 200, 215));
  }

  private void DrawGameOver(SpriteBatch spriteBatch)
  {
    Primitives.DrawRectangle(spriteBatch, new Rectangle(0, 0, _viewportWidth, _viewportHeight), new Color(0, 0, 0, 180));
    const string heading = "Game Over";
    string final = $"Final score: {_score}";
    const string hint = "Press R to restart";
    Vector2 hSz = _font.MeasureString(heading);
    Vector2 fSz = _font.MeasureString(final);
    Vector2 iSz = _font.MeasureString(hint);
    Vector2 c = new(_viewportWidth * 0.5f, _viewportHeight * 0.5f);
    spriteBatch.DrawString(_font, heading, new Vector2(c.X - hSz.X * 0.5f, c.Y - 60), new Color(230, 90, 100));
    spriteBatch.DrawString(_font, final,   new Vector2(c.X - fSz.X * 0.5f, c.Y - 10), Color.White);
    spriteBatch.DrawString(_font, hint,    new Vector2(c.X - iSz.X * 0.5f, c.Y + 30), new Color(210, 210, 220));
  }
}
