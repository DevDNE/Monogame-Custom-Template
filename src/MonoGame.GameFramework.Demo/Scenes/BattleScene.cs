// Scene Class: Responsible for creating and managing game objects.
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.Demo.Components.Entities;

namespace MonoGame.GameFramework.Demo.Scenes;
public class BattleScene : GameScene
{
  private Player _player;
  private EnemyPlayer _enemyPlayer;
  private Gameboard _gameboard;
  private DrawManager _drawManager;

  public BattleScene (ServiceProvider serviceProvider)
  {
    _drawManager = serviceProvider.GetService<DrawManager>();
    _gameboard = new Gameboard(serviceProvider);
    _player = new Player(serviceProvider);
    _enemyPlayer = new EnemyPlayer(serviceProvider);
  }
  public override void LoadContent(ContentManager content)
  {
    _gameboard.LoadContent(content);
    _player.LoadContent(content);
    _enemyPlayer.LoadContent(content);
  }

  public override void UnloadContent()
  {
    _gameboard.UnloadContent();
    _player.UnloadContent();
    _enemyPlayer.UnloadContent();
    foreach(Projectile p in _player.GetProjectiles().ToList())
      {
        p.UnloadContent();
        _drawManager.RemoveSprite(p.GetSprite());
      }
  }

  public Player GetPlayer()
  {
    return _player;
  }

  public EnemyPlayer GetEnemyPlayer()
  {
    return _enemyPlayer;
  }

  public override void Update(GameTime gameTime)
  {
    _gameboard.Update(gameTime);
    _player.Update(gameTime);
    _enemyPlayer.Update(gameTime);
  }

  public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
  {
    _gameboard.Draw(spriteBatch, gameTime);
    _player.Draw(spriteBatch, gameTime);
    _enemyPlayer.Draw(spriteBatch, gameTime);
    foreach (Projectile p in _player.GetProjectiles())
    {
      p.Draw(spriteBatch, gameTime);
    }
  }
}
