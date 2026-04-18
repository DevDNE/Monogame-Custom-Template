using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameFramework.Rendering;

public static class Primitives
{
  private static Texture2D _pixel;

  public static void Initialize(GraphicsDevice graphicsDevice)
  {
    _pixel = new Texture2D(graphicsDevice, 1, 1);
    _pixel.SetData(new[] { Color.White });
  }

  public static Texture2D Pixel
    => _pixel ?? throw new InvalidOperationException("Primitives.Initialize(GraphicsDevice) must be called before Pixel is accessed.");

  public static void DrawRectangle(SpriteBatch spriteBatch, Rectangle bounds, Color color)
    => spriteBatch.Draw(Pixel, bounds, color);
}
