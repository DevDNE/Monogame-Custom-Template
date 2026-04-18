using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Core;
using MonoGame.GameFramework.Rendering;

namespace MonoGame.GameFramework.Demo.Components.Entities;
public class Gameboard : Entity
{
  private SpriteSheet topTile;
  private SpriteSheet midTile;
  private SpriteSheet botTile;
  private SpriteSheet enemyTopTile;
  private SpriteSheet enemyMidTile;
  private SpriteSheet enemyBotTile;
  private DrawManager drawManager;
  public SpriteSheet[,] boardTiles = new SpriteSheet[3, 3];
  public SpriteSheet[,] enemyBoardTiles = new SpriteSheet[3, 3];
  public Gameboard(ServiceProvider serviceProvider)
  {
    drawManager = serviceProvider.GetService<DrawManager>();
  }

  public override void LoadContent(ContentManager content)
  {
    Texture2D tileTexture = content.Load<Texture2D>("gfx/Battlefield_Tile");
    topTile = SpriteSheet.Static(tileTexture, new Rectangle(200, 250, 80, 48), sourceFrame: new Rectangle(0, 0, 40, 24), name: "TopTile");
    midTile = SpriteSheet.Static(tileTexture, new Rectangle(200, 298, 80, 48), sourceFrame: new Rectangle(48, 0, 40, 24), name: "MidTile");
    botTile = SpriteSheet.Static(tileTexture, new Rectangle(200, 346, 80, 52), sourceFrame: new Rectangle(96, 0, 40, 32), name: "BotTile");
    enemyTopTile = SpriteSheet.Static(tileTexture, new Rectangle(200, 250, 80, 48), sourceFrame: new Rectangle(0, 0, 40, 24), name: "TopTile");
    enemyMidTile = SpriteSheet.Static(tileTexture, new Rectangle(200, 298, 80, 48), sourceFrame: new Rectangle(48, 0, 40, 24), name: "MidTile");
    enemyBotTile = SpriteSheet.Static(tileTexture, new Rectangle(200, 346, 80, 52), sourceFrame: new Rectangle(96, 0, 40, 32), name: "BotTile");

    for (int i = 0; i < 3; i++)
    {
      for (int j = 0; j < 3; j++)
      {
        if (i == 0)
        {
          boardTiles[i, j] = topTile.Clone() as SpriteSheet;
          enemyBoardTiles[i, j] = enemyTopTile.Clone() as SpriteSheet;
        }
        else if (i == 1)
        {
          boardTiles[i, j] = midTile.Clone() as SpriteSheet;
          enemyBoardTiles[i, j] = enemyMidTile.Clone() as SpriteSheet;
        }
        else
        {
          boardTiles[i, j] = botTile.Clone() as SpriteSheet;
          enemyBoardTiles[i, j] = enemyBotTile.Clone() as SpriteSheet;
        }

        boardTiles[i, j].Position = new Vector2(j * BattleConfig.TileSize + BattleConfig.PlayerBoardX, i * BattleConfig.TileSize + BattleConfig.BoardY);
        boardTiles[i, j].DestinationFrame = new Rectangle((int)boardTiles[i, j].Position.X, (int)boardTiles[i, j].Position.Y, BattleConfig.TileSize, BattleConfig.TileSize);

        enemyBoardTiles[i, j].Position = new Vector2(j * BattleConfig.TileSize + BattleConfig.EnemyBoardX, i * BattleConfig.TileSize + BattleConfig.BoardY);
        enemyBoardTiles[i, j].DestinationFrame = new Rectangle((int)enemyBoardTiles[i, j].Position.X, (int)enemyBoardTiles[i, j].Position.Y, BattleConfig.TileSize, BattleConfig.TileSize);

        drawManager.AddSprite(boardTiles[i, j]);
        drawManager.AddSprite(enemyBoardTiles[i, j]);
      }
    }
  }

  public override void UnloadContent()
  {
    throw new System.NotImplementedException();
  }

  public override void Update(GameTime gameTime)
  {

  }
}
