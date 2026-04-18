using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.BattleGrid.Components.Entities;

namespace MonoGame.GameFramework.BattleGrid.Scenes;

public class BattleScene : GameScene
{
  private readonly Player _player;
  private readonly EnemyPlayer _enemyPlayer;
  private readonly Gameboard _gameboard;

  public BattleScene(ServiceProvider serviceProvider)
  {
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
  }

  public Player GetPlayer() => _player;
  public EnemyPlayer GetEnemyPlayer() => _enemyPlayer;

  public override void Update(GameTime gameTime)
  {
    _gameboard.Update(gameTime);
    _player.Update(gameTime);
    _enemyPlayer.Update(gameTime);
  }
}
