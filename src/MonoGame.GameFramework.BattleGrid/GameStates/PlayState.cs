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
  private enum Outcome { None, Win, Lose }

  private readonly ServiceProvider _serviceProvider;
  private readonly GameStateManager _gameStateManager;
  private readonly SceneManager _sceneManager;
  private readonly SettingsManager _settingsManager;
  private readonly EventManager _eventManager;
  private readonly DrawManager _drawManager;
  private readonly TextManager _textManager;
  private readonly KeyboardManager _keyboardManager;

  private SpriteFont _font;
  private int _viewportWidth;
  private int _viewportHeight;

  private BattleScene _battleScene;
  private DebugState _debugState;
  private Outcome _outcome = Outcome.None;

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

  public override void Leaving()
  {
    _sceneManager.RemoveScene("Battle");
  }

  public override void Obscuring() => IsActive = false;
  public override void Revealed() => IsActive = true;

  public override void Update(GameTime gameTime)
  {
    if (_keyboardManager.WasKeyPressed(Keys.R))
    {
      RestartBattle();
      return;
    }

    if (_outcome != Outcome.None) return;

    _sceneManager.Update(gameTime);
    CheckCollisions();
    CheckOutcome();
  }

  public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
  {
    spriteBatch.Begin();
    _drawManager.Draw(spriteBatch);
    _textManager.Draw(spriteBatch);

    if (_outcome != Outcome.None)
    {
      string heading = _outcome == Outcome.Win ? "You Win!" : "Defeated";
      Color headingColor = _outcome == Outcome.Win ? new Color(120, 220, 140) : new Color(230, 90, 100);
      const string hint = "Press R to restart";

      Primitives.DrawRectangle(spriteBatch, new Rectangle(0, 0, _viewportWidth, _viewportHeight), new Color(0, 0, 0, 160));

      Vector2 hSize = _font.MeasureString(heading);
      Vector2 hintSize = _font.MeasureString(hint);
      Vector2 center = new(_viewportWidth * 0.5f, _viewportHeight * 0.5f);
      spriteBatch.DrawString(_font, heading, new Vector2(center.X - hSize.X * 0.5f, center.Y - hSize.Y - 6), headingColor);
      spriteBatch.DrawString(_font, hint, new Vector2(center.X - hintSize.X * 0.5f, center.Y + 6), new Color(210, 210, 220));
    }
    spriteBatch.End();
  }

  private void StartFreshBattle()
  {
    _battleScene = new BattleScene(_serviceProvider);
    _sceneManager.AddScene("Battle", _battleScene);
    _sceneManager.LoadScene("Battle");
    _outcome = Outcome.None;
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

    // Player projectiles → Enemy
    foreach (Projectile p in player.GetProjectiles().ToList())
    {
      if (p.GetHurtbox().Intersects(enemy.GetHitbox()))
      {
        player.RemoveProjectileOnCollision(p);
        _eventManager.TriggerEvent("EnemyHit", this, new GameEventArgs("Enemy hit"));
      }
    }

    // Enemy projectiles → Player
    foreach (Projectile p in enemy.GetProjectiles().ToList())
    {
      if (p.GetHurtbox().Intersects(player.GetHitbox()))
      {
        enemy.RemoveProjectileOnCollision(p);
        player.Damage(p.GetDamageNumber());
      }
    }
  }

  private void CheckOutcome()
  {
    Player player = _battleScene.GetPlayer();
    EnemyPlayer enemy = _battleScene.GetEnemyPlayer();
    if (!player.IsAlive) _outcome = Outcome.Lose;
    else if (!enemy.IsAlive) _outcome = Outcome.Win;
  }
}
