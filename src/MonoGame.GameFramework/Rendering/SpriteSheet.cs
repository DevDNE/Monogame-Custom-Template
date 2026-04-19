using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameFramework.Rendering;
public class SpriteSheet
{
  public string Name { get; init; }
  public Texture2D Texture { get; init; }
  public Vector2 Position { get; set; }
  public int Width { get; init; }
  public int Height { get; init; }
  public Rectangle SourceFrame { get; init; }
  public Rectangle DestinationFrame { get; set; }
  public Color Tint { get; set; } = Color.White;

  public static SpriteSheet Static(Texture2D texture, Rectangle destinationFrame, Rectangle? sourceFrame = null, string name = null)
  {
    Rectangle src = sourceFrame ?? new Rectangle(0, 0, texture.Width, texture.Height);
    return new SpriteSheet
    {
      Name = name,
      Texture = texture,
      Position = new Vector2(destinationFrame.X, destinationFrame.Y),
      Width = src.Width,
      Height = src.Height,
      SourceFrame = src,
      DestinationFrame = destinationFrame,
    };
  }
}
