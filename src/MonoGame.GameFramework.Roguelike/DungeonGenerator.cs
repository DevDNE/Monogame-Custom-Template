using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.GameFramework.Rendering;

namespace MonoGame.GameFramework.Roguelike;

public enum TileKind { Wall, Floor, StairsDown }

/// <summary>
/// Simple rooms-and-corridors generator. Produces a TileMap + populated
/// TileLayer&lt;TileKind&gt; plus a list of room rectangles and a spawn point.
/// </summary>
public static class DungeonGenerator
{
  public const int Columns = 60;
  public const int Rows = 34;
  public const int CellSize = 18;
  public const int RoomCountTarget = 10;
  public const int RoomMin = 4;
  public const int RoomMax = 9;

  public record Result(TileMap Map, TileLayer<TileKind> Tiles, List<Rectangle> Rooms, Point Spawn);

  public static Result Generate(int seed)
  {
    Random rng = new(seed);
    TileMap map = new(Columns, Rows, CellSize, CellSize);
    TileLayer<TileKind> tiles = map.AddLayer<TileKind>("tiles");
    tiles.Fill(TileKind.Wall);

    List<Rectangle> rooms = new();
    for (int attempt = 0; attempt < RoomCountTarget * 5 && rooms.Count < RoomCountTarget; attempt++)
    {
      int w = rng.Next(RoomMin, RoomMax + 1);
      int h = rng.Next(RoomMin, RoomMax + 1);
      int x = rng.Next(1, Columns - w - 1);
      int y = rng.Next(1, Rows - h - 1);
      Rectangle candidate = new(x, y, w, h);
      bool overlap = false;
      foreach (Rectangle existing in rooms)
      {
        if (existing.Intersects(new Rectangle(candidate.X - 1, candidate.Y - 1, candidate.Width + 2, candidate.Height + 2)))
        {
          overlap = true;
          break;
        }
      }
      if (overlap) continue;
      rooms.Add(candidate);
      CarveRoom(tiles, candidate);
    }

    // Connect every room to the previous one with an L-shaped corridor.
    for (int i = 1; i < rooms.Count; i++)
    {
      Point a = RoomCenter(rooms[i - 1]);
      Point b = RoomCenter(rooms[i]);
      if (rng.Next(2) == 0)
      {
        CarveHCorridor(tiles, a.X, b.X, a.Y);
        CarveVCorridor(tiles, a.Y, b.Y, b.X);
      }
      else
      {
        CarveVCorridor(tiles, a.Y, b.Y, a.X);
        CarveHCorridor(tiles, a.X, b.X, b.Y);
      }
    }

    // Stairs in the last room.
    Point stairs = RoomCenter(rooms[^1]);
    tiles[stairs.X, stairs.Y] = TileKind.StairsDown;

    Point spawn = RoomCenter(rooms[0]);
    return new Result(map, tiles, rooms, spawn);
  }

  private static Point RoomCenter(Rectangle r) => new(r.X + r.Width / 2, r.Y + r.Height / 2);

  private static void CarveRoom(TileLayer<TileKind> tiles, Rectangle room)
  {
    for (int y = room.Top; y < room.Bottom; y++)
      for (int x = room.Left; x < room.Right; x++)
        tiles[x, y] = TileKind.Floor;
  }

  private static void CarveHCorridor(TileLayer<TileKind> tiles, int x1, int x2, int y)
  {
    int min = Math.Min(x1, x2), max = Math.Max(x1, x2);
    for (int x = min; x <= max; x++) tiles[x, y] = TileKind.Floor;
  }

  private static void CarveVCorridor(TileLayer<TileKind> tiles, int y1, int y2, int x)
  {
    int min = Math.Min(y1, y2), max = Math.Max(y1, y2);
    for (int y = min; y <= max; y++) tiles[x, y] = TileKind.Floor;
  }
}
