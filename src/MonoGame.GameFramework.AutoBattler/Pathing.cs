using System.Collections.Generic;

namespace MonoGame.GameFramework.AutoBattler;

/// <summary>
/// Hand-rolled BFS on the 8×4 grid. Returns the first step the actor
/// should take to head toward the target, or null if no path exists.
/// Occupied cells (except the target itself) are walls.
///
/// FINDINGS observation (for the final §8 update): this is tiny (~40
/// lines). Not worth promoting to the library for a sample-of-one.
/// A second pathfinding consumer in a real project would be the trigger.
/// </summary>
public static class Pathing
{
  public static (int col, int row)? NextStepToward(Board board, Unit actor, Unit target)
  {
    if (actor == null || target == null || !actor.Alive || !target.Alive) return null;
    if (actor.Col == target.Col && actor.Row == target.Row) return null;

    (int, int) start = (actor.Col, actor.Row);
    (int, int) goal = (target.Col, target.Row);

    // BFS from start. Parent map lets us reconstruct the next step.
    Dictionary<(int, int), (int, int)> parent = new() { [start] = start };
    Queue<(int, int)> queue = new();
    queue.Enqueue(start);
    (int dc, int dr)[] dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    while (queue.Count > 0)
    {
      (int c, int r) = queue.Dequeue();
      if ((c, r) == goal)
      {
        (int tc, int tr) = goal;
        while (parent[(tc, tr)] != start) (tc, tr) = parent[(tc, tr)];
        return (tc, tr);
      }
      foreach ((int dc, int dr) in dirs)
      {
        int nc = c + dc, nr = r + dr;
        if (!board.InBounds(nc, nr)) continue;
        if (parent.ContainsKey((nc, nr))) continue;
        if ((nc, nr) != goal && board.UnitAt(nc, nr) != null) continue; // blocked by unit (except target cell)
        parent[(nc, nr)] = (c, r);
        queue.Enqueue((nc, nr));
      }
    }
    return null;
  }

  public static int ChebyshevDistance(Unit a, Unit b)
  {
    int dc = a.Col - b.Col; if (dc < 0) dc = -dc;
    int dr = a.Row - b.Row; if (dr < 0) dr = -dr;
    return dc > dr ? dc : dr;
  }
}
