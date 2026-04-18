using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameFramework.Lifecycle;
public abstract class GameScene
{
  public abstract void LoadContent(ContentManager content);
  public abstract void UnloadContent();
  public abstract void Update(GameTime gameTime);
  public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime) { }
}
