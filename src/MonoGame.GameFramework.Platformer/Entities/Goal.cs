using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameFramework.Platformer.Entities;

public class Goal
{
  public const int Width = 28;
  public const int Height = 48;

  public Rectangle Bounds { get; }

  public Goal(Vector2 position)
  {
    Bounds = new Rectangle((int)position.X, (int)position.Y, Width, Height);
  }

  public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
  {
    spriteBatch.Draw(pixel, Bounds, new Color(80, 220, 130));
  }
}
