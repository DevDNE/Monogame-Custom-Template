using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameFramework.Lifecycle;
public abstract class GameState
{
    public bool IsActive { get; set; }
    public abstract void Entered();
    public abstract void Leaving();
    public abstract void Obscuring();
    public abstract void Revealed();
    public abstract void Update(GameTime gameTime);
    public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime) { }
}
