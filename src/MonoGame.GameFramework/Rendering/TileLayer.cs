using System;

namespace MonoGame.GameFramework.Rendering;

public class TileLayer<T>
{
  public int Columns { get; }
  public int Rows { get; }
  public string Name { get; }
  private readonly T[,] _cells;

  public TileLayer(string name, int columns, int rows)
  {
    if (columns <= 0) throw new ArgumentOutOfRangeException(nameof(columns));
    if (rows <= 0) throw new ArgumentOutOfRangeException(nameof(rows));
    Name = name;
    Columns = columns;
    Rows = rows;
    _cells = new T[columns, rows];
  }

  public T this[int column, int row]
  {
    get => _cells[column, row];
    set => _cells[column, row] = value;
  }

  public bool InBounds(int column, int row)
    => column >= 0 && column < Columns && row >= 0 && row < Rows;

  public void Fill(T value)
  {
    for (int c = 0; c < Columns; c++)
      for (int r = 0; r < Rows; r++)
        _cells[c, r] = value;
  }
}
