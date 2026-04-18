using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Events;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Persistence;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.Text;
using MonoGame.GameFramework.BattleGrid.Components.Entities;
using MonoGame.GameFramework.BattleGrid.Scenes;

namespace MonoGame.GameFramework.BattleGrid.GameStates;

public class PlayState : GameState
{
  private readonly ServiceProvider _serviceProvider;
  private readonly GameStateManager _gameStateManager;
  private readonly SceneManager _sceneManager;
  private readonly SettingsManager _settingsManager;
  private readonly EventManager _eventManager;
  private readonly DrawManager _drawManager;
  private readonly TextManager _textManager;

  private GameScene _battleScene;
  private DebugState _debugState;

  public PlayState(ServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
    _gameStateManager = serviceProvider.GetService<GameStateManager>();
    _sceneManager = serviceProvider.GetService<SceneManager>();
    _settingsManager = serviceProvider.GetService<SettingsManager>();
    _eventManager = serviceProvider.GetService<EventManager>();
    _drawManager = serviceProvider.GetService<DrawManager>();
    _textManager = serviceProvider.GetService<TextManager>();
  }

  public override void Entered()
  {
    _battleScene = new BattleScene(_serviceProvider);
    _sceneManager.AddScene("Battle", _battleScene);
    _sceneManager.LoadScene("Battle");
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
    _sceneManager.Update(gameTime);
    CheckCollisions();
  }

  public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
  {
    spriteBatch.Begin();
    _drawManager.Draw(spriteBatch);
    _textManager.Draw(spriteBatch);
    spriteBatch.End();
  }

  private void CheckCollisions()
  {
    BattleScene scene = _battleScene as BattleScene;
    Player player = scene.GetPlayer();
    EnemyPlayer enemyPlayer = scene.GetEnemyPlayer();

    foreach (Projectile p in player.GetProjectiles().ToList())
    {
      if (p.GetHurtbox().Intersects(enemyPlayer.GetHitbox()))
      {
        player.RemoveProjectileOnCollision(p);
        _eventManager.TriggerEvent("EnemyHit", this, new GameEventArgs("Enemy hit"));
      }

      if (p.GetSprite().Position.X == 800)
      {
        _eventManager.TriggerEvent("EnemyMiss", this, new GameEventArgs("Enemy miss"));
      }
    }
  }
}
