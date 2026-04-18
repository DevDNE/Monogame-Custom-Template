using System;
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
  public Rectangle[] Frames { get; init; } = Array.Empty<Rectangle>();
  public Rectangle DestinationFrame { get; set; }
  public float FrameInterval { get; init; }
  public int CurrentFrame { get; set; }
  private float elapsedTime;

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
      Frames = new[] { src },
      DestinationFrame = destinationFrame,
    };
  }

  public static SpriteSheet Animated(Texture2D texture, Rectangle[] frames, Rectangle destinationFrame, float frameInterval, string name = null, int startFrame = 0)
  {
    Rectangle first = frames[0];
    return new SpriteSheet
    {
      Name = name,
      Texture = texture,
      Position = new Vector2(destinationFrame.X, destinationFrame.Y),
      Width = first.Width,
      Height = first.Height,
      Frames = frames,
      DestinationFrame = destinationFrame,
      FrameInterval = frameInterval,
      CurrentFrame = startFrame,
    };
  }

  public SpriteSheet Clone() => new SpriteSheet
  {
    Name = Name,
    Texture = Texture,
    Position = Position,
    Width = Width,
    Height = Height,
    Frames = (Rectangle[])Frames.Clone(),
    DestinationFrame = DestinationFrame,
    FrameInterval = FrameInterval,
    CurrentFrame = CurrentFrame,
  };

  public void Update(GameTime gameTime)
  {
    elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
    if (elapsedTime >= FrameInterval && FrameInterval > 0f)
    {
      CurrentFrame = (CurrentFrame + 1) % Frames.Length;
      elapsedTime -= FrameInterval;
    }
  }
}
