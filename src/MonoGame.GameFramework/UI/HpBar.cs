using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Rendering;

namespace MonoGame.GameFramework.UI;

public static class HpBar
{
  public static readonly Color DefaultBackground = new(25, 30, 45);

  public static void Draw(SpriteBatch spriteBatch, Rectangle bounds, int current, int max, Color fill)
    => Draw(spriteBatch, bounds, current, max, fill, DefaultBackground);

  public static void Draw(SpriteBatch spriteBatch, Rectangle bounds, int current, int max, Color fill, Color background)
  {
    Primitives.DrawRectangle(spriteBatch, bounds, background);
    int fillWidth = ComputeFillWidth(bounds.Width, current, max);
    if (fillWidth > 0)
    {
      Primitives.DrawRectangle(spriteBatch, new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height), fill);
    }
  }

  public static void DrawWithBorder(SpriteBatch spriteBatch, Rectangle bounds, int current, int max, Color fill, Color border, int borderThickness = 2)
    => DrawWithBorder(spriteBatch, bounds, current, max, fill, border, DefaultBackground, borderThickness);

  public static void DrawWithBorder(SpriteBatch spriteBatch, Rectangle bounds, int current, int max, Color fill, Color border, Color background, int borderThickness = 2)
  {
    Draw(spriteBatch, bounds, current, max, fill, background);
    Primitives.DrawRectangle(spriteBatch, new Rectangle(bounds.X, bounds.Y, bounds.Width, borderThickness), border);
    Primitives.DrawRectangle(spriteBatch, new Rectangle(bounds.X, bounds.Bottom - borderThickness, bounds.Width, borderThickness), border);
    Primitives.DrawRectangle(spriteBatch, new Rectangle(bounds.X, bounds.Y, borderThickness, bounds.Height), border);
    Primitives.DrawRectangle(spriteBatch, new Rectangle(bounds.Right - borderThickness, bounds.Y, borderThickness, bounds.Height), border);
  }

  public static int ComputeFillWidth(int totalWidth, int current, int max)
  {
    if (max <= 0 || totalWidth <= 0) return 0;
    int clamped = Math.Clamp(current, 0, max);
    return (int)(totalWidth * (clamped / (float)max));
  }
}
