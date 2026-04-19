using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoGame.GameFramework.Rendering;

public class TileMap
{
  public int Columns { get; }
  public int Rows { get; }
  public int TileWidth { get; }
  public int TileHeight { get; }
  public Vector2 Origin { get; set; } = Vector2.Zero;

  private readonly Dictionary<string, object> _layers = new();

  public TileMap(int columns, int rows, int tileWidth, int tileHeight)
  {
    Columns = columns;
    Rows = rows;
    TileWidth = tileWidth;
    TileHeight = tileHeight;
  }

  public TileLayer<T> AddLayer<T>(string name)
  {
    TileLayer<T> layer = new(name, Columns, Rows);
    _layers[name] = layer;
    return layer;
  }

  public TileLayer<T> GetLayer<T>(string name)
    => _layers.TryGetValue(name, out object layer) ? (TileLayer<T>)layer : null;

  public bool RemoveLayer(string name) => _layers.Remove(name);

  public Vector2 GetWorldPosition(int column, int row)
    => Origin + new Vector2(column * TileWidth, row * TileHeight);

  public Rectangle GetCellRect(int column, int row)
  {
    Vector2 pos = GetWorldPosition(column, row);
    return new Rectangle((int)pos.X, (int)pos.Y, TileWidth, TileHeight);
  }

  public (int column, int row) WorldToCell(Vector2 world)
  {
    Vector2 local = world - Origin;
    return ((int)(local.X / TileWidth), (int)(local.Y / TileHeight));
  }

  public bool TryWorldToCell(Vector2 world, out int column, out int row)
  {
    Vector2 local = world - Origin;
    if (local.X < 0 || local.Y < 0)
    {
      column = -1;
      row = -1;
      return false;
    }
    column = (int)(local.X / TileWidth);
    row = (int)(local.Y / TileHeight);
    return column < Columns && row < Rows;
  }
}
