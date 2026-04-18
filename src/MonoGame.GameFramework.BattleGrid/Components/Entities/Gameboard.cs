using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGame.GameFramework.Core;
using MonoGame.GameFramework.Rendering;

namespace MonoGame.GameFramework.BattleGrid.Components.Entities;

public class Gameboard : Entity
{
  private const int TileInset = 4;

  private readonly DrawManager _drawManager;
  public SpriteSheet[,] PlayerTiles { get; } = new SpriteSheet[3, 3];
  public SpriteSheet[,] EnemyTiles { get; } = new SpriteSheet[3, 3];

  public Gameboard(ServiceProvider serviceProvider)
  {
    _drawManager = serviceProvider.GetService<DrawManager>();
  }

  public override void LoadContent(ContentManager content)
  {
    Color playerTint = new(50, 90, 170);
    Color enemyTint = new(170, 60, 60);

    for (int row = 0; row < 3; row++)
    {
      for (int col = 0; col < 3; col++)
      {
        PlayerTiles[row, col] = MakeTile(
          BattleConfig.PlayerBoardX + col * BattleConfig.TileSize,
          BattleConfig.BoardY + row * BattleConfig.TileSize,
          playerTint, $"playerTile_{row}_{col}");
        _drawManager.AddSprite(PlayerTiles[row, col]);

        EnemyTiles[row, col] = MakeTile(
          BattleConfig.EnemyBoardX + col * BattleConfig.TileSize,
          BattleConfig.BoardY + row * BattleConfig.TileSize,
          enemyTint, $"enemyTile_{row}_{col}");
        _drawManager.AddSprite(EnemyTiles[row, col]);
      }
    }
  }

  private static SpriteSheet MakeTile(int x, int y, Color tint, string name)
  {
    SpriteSheet tile = SpriteSheet.Static(
      Primitives.Pixel,
      new Rectangle(x + TileInset / 2, y + TileInset / 2, BattleConfig.TileSize - TileInset, BattleConfig.TileSize - TileInset),
      name: name);
    tile.Tint = tint;
    return tile;
  }

  public override void UnloadContent()
  {
    for (int row = 0; row < 3; row++)
    {
      for (int col = 0; col < 3; col++)
      {
        if (PlayerTiles[row, col] != null) _drawManager.RemoveSprite(PlayerTiles[row, col]);
        if (EnemyTiles[row, col] != null) _drawManager.RemoveSprite(EnemyTiles[row, col]);
      }
    }
  }

  public override void Update(GameTime gameTime) { }
}
