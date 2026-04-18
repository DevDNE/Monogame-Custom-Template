// Game State Class: Responsible for handling game logic, including collision detection.
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using MonoGame.GameFramework.Events;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Persistence;
using MonoGame.GameFramework.Demo.Components.Entities;
using MonoGame.GameFramework.Demo.Scenes;

namespace MonoGame.GameFramework.Demo.GameStates;
public class BattleState : GameState
{
    private GraphicsDevice _graphicsDevice;
  private ServiceProvider _serviceProvider;
  private GameStateManager _gameStateManager;
  private SettingsManager _settingsManager;
  private SceneManager _sceneManager;
  private GameScene _battleScene;
  private DebugState debugState;
  private EventManager _eventManager;
  public BattleState(GraphicsDevice graphicsDevice, ServiceProvider serviceProvider)
  {
    _graphicsDevice = graphicsDevice;
    _serviceProvider = serviceProvider;
    _sceneManager = serviceProvider.GetService<SceneManager>();
    _gameStateManager = _serviceProvider.GetService<GameStateManager>();
    _settingsManager = serviceProvider.GetService<SettingsManager>();
    _eventManager = serviceProvider.GetService<EventManager>();
    _battleScene = new BattleScene(serviceProvider);
  }

  public override void Entered()
  {
    _sceneManager.AddScene("InitialScene", _battleScene);
    _sceneManager.LoadScene("InitialScene");
    IsActive = true;

    if (_settingsManager.DebugMode)
    {
      debugState = new DebugState(_graphicsDevice, _serviceProvider);
      _gameStateManager.PushState(debugState);
    }
  }

  public override void Leaving()
  {
    _sceneManager.RemoveScene("InitialScene");
  }

  public override void Obscuring()
  {
    IsActive = false;
  }

  public override void Revealed()
  {
    IsActive = true;
  }

  public override void Update(GameTime gameTime)
  {
    _sceneManager.Update(gameTime);
    CheckCollisions();
  }

  public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
  {
    _sceneManager.Draw(spriteBatch, gameTime);
  }

  private void CheckCollisions()
  {
    BattleScene battleScene = _battleScene as BattleScene;
    Player player = battleScene.GetPlayer();
    EnemyPlayer enemyPlayer = battleScene.GetEnemyPlayer();

    foreach (Projectile p in player.GetProjectiles().ToList())
    {
      if (p.GetHurtbox().Intersects(enemyPlayer.GetHitbox()))
      {
        player.RemoveProjectileOnCollision(p);
        _eventManager.TriggerEvent("EnemyHit", this, new GameEventArgs("Enemy  hit"));
      }

      if (p.GetSprite().Position.X == 800)
      {
        _eventManager.TriggerEvent("EnemyMiss", this, new GameEventArgs("Enemy  miss"));
      }
    }
  }
}
