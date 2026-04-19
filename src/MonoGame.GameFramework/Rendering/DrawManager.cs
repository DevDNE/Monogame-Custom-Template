using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MonoGame.GameFramework.Rendering;
public class DrawManager
{
  private readonly List<SpriteSheet> sprites = new();

  public void Draw(SpriteBatch spriteBatch)
  {
    foreach (SpriteSheet sprite in sprites)
    {
      spriteBatch.Draw(sprite.Texture, sprite.DestinationFrame, sprite.SourceFrame, sprite.Tint);
    }
  }

  public void AddSprite(SpriteSheet sprite)
  {
    sprites.Add(sprite);
  }

  public void RemoveSprite(SpriteSheet sprite)
  {
    sprites.Remove(sprite);
  }
}
