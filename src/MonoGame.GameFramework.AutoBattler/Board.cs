using System.Collections.Generic;

namespace MonoGame.GameFramework.AutoBattler;

/// <summary>
/// 8-wide × 4-tall shared battlefield. Cols 0..3 are the player's side
/// (used for placement during shop phase); cols 4..7 are the enemy's.
/// During combat all cells are fair game for movement.
/// </summary>
public class Board
{
  public const int Columns = 8;
  public const int Rows = 4;
  public const int PlayerSideEndColExclusive = 4;

  public readonly List<Unit> Units = new();

  public bool IsPlayerCell(int col, int row) => col >= 0 && col < PlayerSideEndColExclusive && row >= 0 && row < Rows;
  public bool IsEnemyCell(int col, int row) => col >= PlayerSideEndColExclusive && col < Columns && row >= 0 && row < Rows;
  public bool InBounds(int col, int row) => col >= 0 && col < Columns && row >= 0 && row < Rows;

  public Unit UnitAt(int col, int row)
  {
    foreach (Unit u in Units)
      if (u.Alive && u.Col == col && u.Row == row) return u;
    return null;
  }

  public IEnumerable<Unit> AliveOnSide(Side side)
  {
    foreach (Unit u in Units)
      if (u.Alive && u.Side == side) yield return u;
  }

  public int AliveCount(Side side)
  {
    int n = 0;
    foreach (Unit u in Units) if (u.Alive && u.Side == side) n++;
    return n;
  }

  public void Reset() => Units.Clear();
}
