using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameFramework.Text;

public readonly record struct TextHandle(int Id);

public class TextElement
{
  public int Id { get; init; }
  public string Group { get; init; }
  public string Text { get; set; }
  public Vector2 Position { get; set; }
  public Color Color { get; set; }
  public SpriteFont Font { get; init; }
}
