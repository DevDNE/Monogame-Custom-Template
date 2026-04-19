using Microsoft.Xna.Framework;

namespace MonoGame.GameFramework.Rendering;

public static class GridMath
{
  public static bool TryMouseToCell(Vector2 mouse, Vector2 origin, int cellSize, int columns, int rows, out int column, out int row)
  {
    float localX = mouse.X - origin.X;
    float localY = mouse.Y - origin.Y;
    if (localX < 0 || localY < 0 || cellSize <= 0)
    {
      column = -1;
      row = -1;
      return false;
    }
    column = (int)(localX / cellSize);
    row = (int)(localY / cellSize);
    return column < columns && row < rows;
  }
}
