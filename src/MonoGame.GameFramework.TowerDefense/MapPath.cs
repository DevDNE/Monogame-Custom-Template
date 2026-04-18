using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoGame.GameFramework.TowerDefense;

/// <summary>
/// Fixed waypoint-path the enemies follow. Hard-coded in tile coordinates
/// to keep MVP scope tight — no pathfinding, no editor, no variation.
/// </summary>
public static class MapPath
{
  public const int Columns = 20;
  public const int Rows = 14;
  public const int CellSize = 44;

  public static readonly (int col, int row)[] Waypoints =
  {
    (-1, 2), (4, 2), (4, 7), (10, 7), (10, 3), (15, 3), (15, 10), (20, 10)
  };

  public static HashSet<(int col, int row)> GetPathCells()
  {
    HashSet<(int, int)> cells = new();
    for (int i = 0; i + 1 < Waypoints.Length; i++)
    {
      (int c0, int r0) = Waypoints[i];
      (int c1, int r1) = Waypoints[i + 1];
      if (c0 == c1)
      {
        int minR = System.Math.Min(r0, r1);
        int maxR = System.Math.Max(r0, r1);
        for (int r = minR; r <= maxR; r++)
          if (r >= 0 && r < Rows) cells.Add((c0, r));
      }
      else
      {
        int minC = System.Math.Min(c0, c1);
        int maxC = System.Math.Max(c0, c1);
        for (int c = minC; c <= maxC; c++)
          if (c >= 0 && c < Columns) cells.Add((c, r0));
      }
    }
    return cells;
  }

  public static Vector2 WorldPosition(TileMapOrigin origin, int col, int row)
    => new(origin.X + col * CellSize + CellSize * 0.5f,
           origin.Y + row * CellSize + CellSize * 0.5f);

  public readonly record struct TileMapOrigin(float X, float Y);
}
