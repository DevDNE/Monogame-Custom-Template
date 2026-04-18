using Microsoft.Xna.Framework;

namespace MonoGame.GameFramework.BattleGrid;

/// <summary>
/// Shared coordinate helpers for the 3×3 player/enemy grids.
/// </summary>
internal static class Grid
{
  public static Vector2 PlayerCellCenter(int col, int row)
    => new(
      BattleConfig.PlayerBoardX + col * BattleConfig.TileSize + BattleConfig.TileSize * 0.5f,
      BattleConfig.BoardY + row * BattleConfig.TileSize + BattleConfig.TileSize * 0.5f);

  public static Vector2 EnemyCellCenter(int col, int row)
    => new(
      BattleConfig.EnemyBoardX + col * BattleConfig.TileSize + BattleConfig.TileSize * 0.5f,
      BattleConfig.BoardY + row * BattleConfig.TileSize + BattleConfig.TileSize * 0.5f);

  public static Vector2 PlayerCellTopLeft(int col, int row)
  {
    Vector2 c = PlayerCellCenter(col, row);
    return new Vector2(c.X - BattleConfig.DisplayWidth * 0.5f, c.Y - BattleConfig.DisplayHeight * 0.5f);
  }

  public static Vector2 EnemyCellTopLeft(int col, int row)
  {
    Vector2 c = EnemyCellCenter(col, row);
    return new Vector2(c.X - BattleConfig.DisplayWidth * 0.5f, c.Y - BattleConfig.DisplayHeight * 0.5f);
  }

  public static float RowCenterY(int row)
    => BattleConfig.BoardY + row * BattleConfig.TileSize + BattleConfig.TileSize * 0.5f;
}
