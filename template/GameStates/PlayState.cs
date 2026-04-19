using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.GameFramework.Input;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Rendering;

namespace MonoGame.GameFramework.__SAMPLE__.GameStates;

public class PlayState : GameState
{
  private readonly KeyboardManager _keyboard;
  private readonly MouseManager _mouse;
  private readonly SpriteFont _font;
  private readonly int _viewportWidth;
  private readonly int _viewportHeight;

  public PlayState(ServiceProvider sp, SpriteFont font, int vw, int vh)
  {
    _keyboard = sp.GetService<KeyboardManager>();
    _mouse = sp.GetService<MouseManager>();
    _font = font;
    _viewportWidth = vw;
    _viewportHeight = vh;
  }

  public override void Entered() => IsActive = true;
  public override void Leaving() { }
  public override void Obscuring() => IsActive = false;
  public override void Revealed() => IsActive = true;

  public override void Update(GameTime gameTime)
  {
    // Your gameplay Update logic.
  }

  public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
  {
    spriteBatch.Begin();
    Primitives.DrawRectangle(spriteBatch, new Rectangle(0, 0, _viewportWidth, _viewportHeight), new Color(18, 22, 34));
    const string placeholder = "__SAMPLE__ PlayState — replace this with your game.";
    Vector2 sz = _font.MeasureString(placeholder);
    spriteBatch.DrawString(_font, placeholder,
      new Vector2(_viewportWidth / 2f - sz.X / 2f, _viewportHeight / 2f - sz.Y / 2f),
      Color.White);
    spriteBatch.End();
  }
}
