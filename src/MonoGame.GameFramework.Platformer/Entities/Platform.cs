using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameFramework.Platformer.Entities;

public class Platform
{
  public Rectangle Bounds { get; }
  public Color Color { get; set; } = new(90, 100, 120);

  public Platform(Rectangle bounds)
  {
    Bounds = bounds;
  }

  public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
  {
    spriteBatch.Draw(pixel, Bounds, Color);
  }
}
