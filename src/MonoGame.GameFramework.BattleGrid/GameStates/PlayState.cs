using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.GameFramework.Events;
using MonoGame.GameFramework.Input;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Persistence;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.Text;
using MonoGame.GameFramework.BattleGrid.Components.Entities;
using MonoGame.GameFramework.BattleGrid.Scenes;

namespace MonoGame.GameFramework.BattleGrid.GameStates;

public class PlayState : GameState
{
  private enum Mode { Playing, SelectingChip, Won, Lost }

  private enum Chip { Cannon, WideShot, Sword, Recov }

  private static readonly Chip[] ChipPool = { Chip.Cannon, Chip.WideShot, Chip.Sword, Chip.Recov };
  private const float ChipCooldownDuration = 3f;
  private const int ChipCardWidth = 180;
  private const int ChipCardHeight = 220;
  private const int ChipCardGap = 24;

  private readonly ServiceProvider _serviceProvider;
  private readonly GameStateManager _gameStateManager;
  private readonly SceneManager _sceneManager;
  private readonly SettingsManager _settingsManager;
  private readonly EventManager _eventManager;
  private readonly DrawManager _drawManager;
  private readonly TextManager _textManager;
  private readonly KeyboardManager _keyboardManager;
  private readonly Random _random = new();

  private SpriteFont _font;
  private int _viewportWidth;
  private int _viewportHeight;

  private BattleScene _battleScene;
  private DebugState _debugState;
  private Mode _mode = Mode.Playing;
  private Chip[] _availableChips = new Chip[3];
  private float _chipCooldown = 0f;
  private float _swordFlashRemaining = 0f;

  public PlayState(ServiceProvider serviceProvider, SpriteFont font, int viewportWidth, int viewportHeight)
  {
    _serviceProvider = serviceProvider;
    _gameStateManager = serviceProvider.GetService<GameStateManager>();
    _sceneManager = serviceProvider.GetService<SceneManager>();
    _settingsManager = serviceProvider.GetService<SettingsManager>();
    _eventManager = serviceProvider.GetService<EventManager>();
    _drawManager = serviceProvider.GetService<DrawManager>();
    _textManager = serviceProvider.GetService<TextManager>();
    _keyboardManager = serviceProvider.GetService<KeyboardManager>();
    _font = font;
    _viewportWidth = viewportWidth;
    _viewportHeight = viewportHeight;
  }

  public override void Entered()
  {
    StartFreshBattle();
    IsActive = true;

    if (_settingsManager.DebugMode)
    {
      _debugState = new DebugState(_serviceProvider);
      _gameStateManager.PushState(_debugState);
    }
  }

  public override void Leaving() => _sceneManager.RemoveScene("Battle");
  public override void Obscuring() { /* keep IsActive true so Draw still runs behind overlays */ }
  public override void Revealed() => IsActive = true;

  public override void Update(GameTime gameTime)
  {
    if (_keyboardManager.WasKeyPressed(Keys.R))
    {
      RestartBattle();
      return;
    }

    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
    if (_chipCooldown > 0f) _chipCooldown = Math.Max(0f, _chipCooldown - dt);
    if (_swordFlashRemaining > 0f) _swordFlashRemaining = Math.Max(0f, _swordFlashRemaining - dt);

    switch (_mode)
    {
      case Mode.Playing: UpdatePlaying(gameTime); break;
      case Mode.SelectingChip: UpdateChipSelect(); break;
      case Mode.Won:
      case Mode.Lost: /* wait for R */ break;
    }
  }

  private void UpdatePlaying(GameTime gameTime)
  {
    if (_keyboardManager.WasKeyPressed(Keys.Tab) && _chipCooldown <= 0f)
    {
      OpenChipSelect();
      return;
    }

    _sceneManager.Update(gameTime);
    CheckCollisions();
    CheckOutcome();
  }

  private void UpdateChipSelect()
  {
    if (_keyboardManager.WasKeyPressed(Keys.Tab) || _keyboardManager.WasKeyPressed(Keys.Escape))
    {
      _mode = Mode.Playing;
      return;
    }
    for (int i = 1; i <= 3; i++)
    {
      if (_keyboardManager.WasKeyPressed(Keys.D0 + i) || _keyboardManager.WasKeyPressed(Keys.NumPad0 + i))
      {
        ActivateChip(_availableChips[i - 1]);
        _chipCooldown = ChipCooldownDuration;
        _mode = Mode.Playing;
        return;
      }
    }
  }

  public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
  {
    spriteBatch.Begin();
    _drawManager.Draw(spriteBatch);
    _textManager.Draw(spriteBatch);
    DrawHud(spriteBatch);
    if (_swordFlashRemaining > 0f) DrawSwordFlash(spriteBatch);
    if (_mode == Mode.SelectingChip) DrawChipSelection(spriteBatch);
    if (_mode == Mode.Won || _mode == Mode.Lost) DrawOutcomeOverlay(spriteBatch);
    spriteBatch.End();
  }

  private void StartFreshBattle()
  {
    _battleScene = new BattleScene(_serviceProvider);
    _sceneManager.AddScene("Battle", _battleScene);
    _sceneManager.LoadScene("Battle");
    _mode = Mode.Playing;
    _chipCooldown = 0f;
    _swordFlashRemaining = 0f;
  }

  private void RestartBattle()
  {
    _sceneManager.RemoveScene("Battle");
    StartFreshBattle();
  }

  private void CheckCollisions()
  {
    Player player = _battleScene.GetPlayer();
    EnemyPlayer enemy = _battleScene.GetEnemyPlayer();

    foreach (Projectile p in player.GetProjectiles().ToList())
    {
      if (p.GetHurtbox().Intersects(enemy.GetHitbox()))
      {
        int dmg = p.GetDamageNumber();
        player.RemoveProjectileOnCollision(p);
        enemy.Damage(dmg);
      }
    }

    foreach (Projectile p in enemy.GetProjectiles().ToList())
    {
      if (p.GetHurtbox().Intersects(player.GetHitbox()))
      {
        int dmg = p.GetDamageNumber();
        enemy.RemoveProjectileOnCollision(p);
        player.Damage(dmg);
      }
    }
  }

  private void CheckOutcome()
  {
    Player player = _battleScene.GetPlayer();
    EnemyPlayer enemy = _battleScene.GetEnemyPlayer();
    if (!player.IsAlive) _mode = Mode.Lost;
    else if (!enemy.IsAlive) _mode = Mode.Won;
  }

  private void OpenChipSelect()
  {
    for (int i = 0; i < 3; i++) _availableChips[i] = ChipPool[_random.Next(ChipPool.Length)];
    _mode = Mode.SelectingChip;
  }

  private void ActivateChip(Chip chip)
  {
    Player player = _battleScene.GetPlayer();
    EnemyPlayer enemy = _battleScene.GetEnemyPlayer();
    switch (chip)
    {
      case Chip.Cannon:
        {
          Vector2 pos = new(
            player.GetCharacter().Position.X + BattleConfig.DisplayWidth,
            Grid.RowCenterY(player.GridRow) - 5f);
          Projectile shot = new(_drawManager, pos, new Vector2(16, 0), new Color(120, 220, 255), damage: 40);
          player.FireProjectile(shot);
          break;
        }
      case Chip.WideShot:
        for (int row = 0; row < 3; row++)
        {
          Vector2 pos = new(
            player.GetCharacter().Position.X + BattleConfig.DisplayWidth,
            Grid.RowCenterY(row) - 5f);
          Projectile shot = new(_drawManager, pos, new Vector2(12, 0), new Color(200, 140, 255), damage: 15);
          player.FireProjectile(shot);
        }
        break;
      case Chip.Sword:
        if (player.GridRow == enemy.GridRow) enemy.Damage(30);
        _swordFlashRemaining = 0.25f;
        break;
      case Chip.Recov:
        player.Heal(30);
        break;
    }
  }

  // ----- HUD -----

  private void DrawHud(SpriteBatch spriteBatch)
  {
    Player player = _battleScene.GetPlayer();
    EnemyPlayer enemy = _battleScene.GetEnemyPlayer();

    DrawHpBar(spriteBatch, new Rectangle(20, 20, 240, 22), player.Hp, Player.MaxHp, new Color(80, 200, 140));
    spriteBatch.DrawString(_font, $"HP {player.Hp}", new Vector2(20, 46), Color.White);

    DrawHpBar(spriteBatch, new Rectangle(_viewportWidth - 260, 20, 240, 22), enemy.Hp, EnemyPlayer.MaxHp, new Color(240, 110, 110));
    Vector2 enemySize = _font.MeasureString($"HP {enemy.Hp}");
    spriteBatch.DrawString(_font, $"HP {enemy.Hp}", new Vector2(_viewportWidth - 20 - enemySize.X, 46), Color.White);

    string hint = "WASD = move   Space = shoot   Tab = chip   R = restart   Esc = quit";
    Vector2 hintSize = _font.MeasureString(hint);
    spriteBatch.DrawString(_font, hint, new Vector2(_viewportWidth * 0.5f - hintSize.X * 0.5f, _viewportHeight - 30), new Color(200, 200, 215));

    if (_chipCooldown > 0f)
    {
      string cd = $"Chip ready in {_chipCooldown:0.0}s";
      Vector2 cdSize = _font.MeasureString(cd);
      spriteBatch.DrawString(_font, cd, new Vector2(_viewportWidth * 0.5f - cdSize.X * 0.5f, 20), new Color(255, 220, 140));
    }
    else
    {
      const string ready = "Tab: chip ready";
      Vector2 rSize = _font.MeasureString(ready);
      spriteBatch.DrawString(_font, ready, new Vector2(_viewportWidth * 0.5f - rSize.X * 0.5f, 20), new Color(140, 220, 180));
    }
  }

  private static void DrawHpBar(SpriteBatch spriteBatch, Rectangle bounds, int hp, int max, Color fill)
  {
    Primitives.DrawRectangle(spriteBatch, bounds, new Color(25, 30, 45));
    int fillWidth = (int)(bounds.Width * (hp / (float)max));
    Primitives.DrawRectangle(spriteBatch, new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height), fill);
    Rectangle border = bounds;
    Primitives.DrawRectangle(spriteBatch, new Rectangle(border.X, border.Y, border.Width, 2), new Color(220, 220, 230));
    Primitives.DrawRectangle(spriteBatch, new Rectangle(border.X, border.Bottom - 2, border.Width, 2), new Color(220, 220, 230));
    Primitives.DrawRectangle(spriteBatch, new Rectangle(border.X, border.Y, 2, border.Height), new Color(220, 220, 230));
    Primitives.DrawRectangle(spriteBatch, new Rectangle(border.Right - 2, border.Y, 2, border.Height), new Color(220, 220, 230));
  }

  // ----- Chip selection overlay -----

  private void DrawChipSelection(SpriteBatch spriteBatch)
  {
    Primitives.DrawRectangle(spriteBatch, new Rectangle(0, 0, _viewportWidth, _viewportHeight), new Color(0, 0, 0, 170));

    string heading = "Select a chip (1 / 2 / 3)";
    Vector2 headingSize = _font.MeasureString(heading);
    spriteBatch.DrawString(_font, heading, new Vector2(_viewportWidth * 0.5f - headingSize.X * 0.5f, 80), Color.White);

    int totalWidth = ChipCardWidth * 3 + ChipCardGap * 2;
    int startX = (_viewportWidth - totalWidth) / 2;
    int cardY = (_viewportHeight - ChipCardHeight) / 2;

    for (int i = 0; i < 3; i++)
    {
      Rectangle cardRect = new(startX + i * (ChipCardWidth + ChipCardGap), cardY, ChipCardWidth, ChipCardHeight);
      DrawChipCard(spriteBatch, cardRect, _availableChips[i], i + 1);
    }

    const string cancel = "Tab / Esc to cancel";
    Vector2 cancelSize = _font.MeasureString(cancel);
    spriteBatch.DrawString(_font, cancel, new Vector2(_viewportWidth * 0.5f - cancelSize.X * 0.5f, _viewportHeight - 80), new Color(200, 200, 215));
  }

  private void DrawChipCard(SpriteBatch spriteBatch, Rectangle bounds, Chip chip, int keyNumber)
  {
    (string name, string effect, Color color) = DescribeChip(chip);
    Primitives.DrawRectangle(spriteBatch, bounds, new Color(45, 55, 80));
    Primitives.DrawRectangle(spriteBatch, new Rectangle(bounds.X + 8, bounds.Y + 8, bounds.Width - 16, 56), color);

    Vector2 nameSize = _font.MeasureString(name);
    spriteBatch.DrawString(_font, name, new Vector2(bounds.Center.X - nameSize.X * 0.5f, bounds.Y + 20), Color.White);

    Vector2 effectSize = _font.MeasureString(effect);
    spriteBatch.DrawString(_font, effect, new Vector2(bounds.Center.X - effectSize.X * 0.5f, bounds.Y + 90), new Color(220, 220, 235));

    string key = $"[{keyNumber}]";
    Vector2 keySize = _font.MeasureString(key);
    spriteBatch.DrawString(_font, key, new Vector2(bounds.Center.X - keySize.X * 0.5f, bounds.Bottom - 40), new Color(255, 220, 120));
  }

  private static (string name, string effect, Color color) DescribeChip(Chip chip) => chip switch
  {
    Chip.Cannon => ("Cannon", "40 dmg single shot", new Color(80, 160, 220)),
    Chip.WideShot => ("Wide Shot", "15 dmg x 3 rows", new Color(160, 100, 220)),
    Chip.Sword => ("Sword", "30 dmg same row", new Color(220, 90, 90)),
    Chip.Recov => ("Recov", "Heal 30 HP", new Color(120, 200, 120)),
    _ => ("?", "", Color.Gray),
  };

  private void DrawSwordFlash(SpriteBatch spriteBatch)
  {
    Player player = _battleScene.GetPlayer();
    int x = BattleConfig.PlayerBoardX + 3 * BattleConfig.TileSize;
    int y = BattleConfig.BoardY + player.GridRow * BattleConfig.TileSize;
    Primitives.DrawRectangle(spriteBatch, new Rectangle(x, y, BattleConfig.EnemyBoardX - x + 3 * BattleConfig.TileSize, BattleConfig.TileSize), new Color(255, 220, 100, 120));
  }

  // ----- Outcome overlay -----

  private void DrawOutcomeOverlay(SpriteBatch spriteBatch)
  {
    string heading = _mode == Mode.Won ? "You Win!" : "Defeated";
    Color headingColor = _mode == Mode.Won ? new Color(120, 220, 140) : new Color(230, 90, 100);
    const string hint = "Press R to restart";

    Primitives.DrawRectangle(spriteBatch, new Rectangle(0, 0, _viewportWidth, _viewportHeight), new Color(0, 0, 0, 180));

    Vector2 hSize = _font.MeasureString(heading);
    Vector2 hintSize = _font.MeasureString(hint);
    Vector2 center = new(_viewportWidth * 0.5f, _viewportHeight * 0.5f);
    spriteBatch.DrawString(_font, heading, new Vector2(center.X - hSize.X * 0.5f, center.Y - hSize.Y - 6), headingColor);
    spriteBatch.DrawString(_font, hint, new Vector2(center.X - hintSize.X * 0.5f, center.Y + 6), new Color(210, 210, 220));
  }
}
