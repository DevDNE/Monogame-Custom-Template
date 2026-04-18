using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.GameFramework.Rendering;

namespace MonoGame.GameFramework.Puzzle;

/// <summary>
/// Match-3 board built on Rendering.TileMap + TileLayer&lt;Gem&gt;. First real
/// consumer of those primitives — findings from this game are recorded in
/// FINDINGS §1.x after puzzle ships.
/// </summary>
public class Board
{
  public enum Gem { Empty = 0, Red, Green, Blue, Yellow, Purple }

  private static readonly Color[] GemColors =
  {
    Color.Transparent,              // Empty
    new(220, 80, 80),               // Red
    new(100, 200, 110),             // Green
    new(80, 160, 240),              // Blue
    new(240, 210, 90),              // Yellow
    new(180, 110, 220),             // Purple
  };

  public const int Columns = 7;
  public const int Rows = 7;
  public const int CellSize = 64;
  public const int GemInset = 6;

  public TileMap Map { get; }
  public TileLayer<Gem> Gems { get; }

  private readonly Random _rng = new();

  public int Score { get; private set; }

  public Board(Vector2 origin)
  {
    Map = new TileMap(Columns, Rows, CellSize, CellSize) { Origin = origin };
    Gems = Map.AddLayer<Gem>("gems");
    FillRandomNoMatches();
  }

  /// <summary>
  /// Fill the whole board with random gems, re-rolling any that would
  /// form an initial 3-in-a-row.
  /// </summary>
  public void FillRandomNoMatches()
  {
    for (int r = 0; r < Rows; r++)
    {
      for (int c = 0; c < Columns; c++)
      {
        Gem pick;
        do { pick = RandomGem(); }
        while (WouldCauseInitialMatch(c, r, pick));
        Gems[c, r] = pick;
      }
    }
    Score = 0;
  }

  private bool WouldCauseInitialMatch(int c, int r, Gem pick)
  {
    if (c >= 2 && Gems[c - 1, r] == pick && Gems[c - 2, r] == pick) return true;
    if (r >= 2 && Gems[c, r - 1] == pick && Gems[c, r - 2] == pick) return true;
    return false;
  }

  private Gem RandomGem() => (Gem)(_rng.Next(5) + 1);

  public bool AreAdjacent((int c, int r) a, (int c, int r) b)
    => (Math.Abs(a.c - b.c) == 1 && a.r == b.r)
    || (Math.Abs(a.r - b.r) == 1 && a.c == b.c);

  /// <summary>
  /// Try to commit a swap; returns true if it creates a match (and
  /// cascades clear + refill have been applied), false if it reverts.
  /// </summary>
  public bool TrySwap((int c, int r) a, (int c, int r) b)
  {
    if (!Map.GetLayer<Gem>("gems").InBounds(a.c, a.r)) return false;
    if (!Gems.InBounds(b.c, b.r)) return false;
    if (!AreAdjacent(a, b)) return false;

    (Gems[a.c, a.r], Gems[b.c, b.r]) = (Gems[b.c, b.r], Gems[a.c, a.r]);
    if (ResolveCascades()) return true;

    // revert
    (Gems[a.c, a.r], Gems[b.c, b.r]) = (Gems[b.c, b.r], Gems[a.c, a.r]);
    return false;
  }

  /// <summary>
  /// Repeatedly: detect matches → clear → gravity → refill, until no
  /// more matches exist. Returns true if at least one match was cleared.
  /// </summary>
  private bool ResolveCascades()
  {
    bool anyMatch = false;
    while (true)
    {
      HashSet<(int c, int r)> matched = FindMatches();
      if (matched.Count == 0) break;
      anyMatch = true;
      Score += matched.Count * 10;
      foreach ((int c, int r) in matched) Gems[c, r] = Gem.Empty;
      ApplyGravity();
      RefillTop();
    }
    return anyMatch;
  }

  private HashSet<(int c, int r)> FindMatches()
  {
    HashSet<(int, int)> matched = new();
    // Rows
    for (int r = 0; r < Rows; r++)
    {
      int runStart = 0;
      for (int c = 1; c <= Columns; c++)
      {
        bool endOfRun = c == Columns || Gems[c, r] != Gems[runStart, r] || Gems[c, r] == Gem.Empty;
        if (endOfRun)
        {
          int len = c - runStart;
          if (Gems[runStart, r] != Gem.Empty && len >= 3)
          {
            for (int k = runStart; k < c; k++) matched.Add((k, r));
          }
          runStart = c;
        }
      }
    }
    // Columns
    for (int c = 0; c < Columns; c++)
    {
      int runStart = 0;
      for (int r = 1; r <= Rows; r++)
      {
        bool endOfRun = r == Rows || Gems[c, r] != Gems[c, runStart] || Gems[c, r] == Gem.Empty;
        if (endOfRun)
        {
          int len = r - runStart;
          if (Gems[c, runStart] != Gem.Empty && len >= 3)
          {
            for (int k = runStart; k < r; k++) matched.Add((c, k));
          }
          runStart = r;
        }
      }
    }
    return matched;
  }

  private void ApplyGravity()
  {
    for (int c = 0; c < Columns; c++)
    {
      int write = Rows - 1;
      for (int r = Rows - 1; r >= 0; r--)
      {
        if (Gems[c, r] != Gem.Empty)
        {
          Gems[c, write] = Gems[c, r];
          if (write != r) Gems[c, r] = Gem.Empty;
          write--;
        }
      }
      // Anything above `write` is now Empty — will be refilled.
    }
  }

  private void RefillTop()
  {
    for (int c = 0; c < Columns; c++)
      for (int r = 0; r < Rows; r++)
        if (Gems[c, r] == Gem.Empty) Gems[c, r] = RandomGem();
  }

  public static Color ColorOf(Gem gem) => GemColors[(int)gem];
}
