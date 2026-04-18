using Microsoft.Xna.Framework;
using MonoGame.GameFramework.Rendering;
using Microsoft.Extensions.DependencyInjection;
using MonoGame.GameFramework.UI;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameFramework.Demo.Components.UI;
public class PlayerHealthUI
{
  private SpriteSheet _healthBar;
  private UIManager _uiManager;
  private DrawManager _drawManager;

  public PlayerHealthUI(ServiceProvider serviceProvider)
  {
    _uiManager = serviceProvider.GetService<UIManager>();
    _drawManager = serviceProvider.GetService<DrawManager>();
  }

  public void LoadContent(ContentManager content)
  {
    _healthBar = SpriteSheet.Static(
      content.Load<Texture2D>("gfx/Player_Idle"),
      new Rectangle(100, 100, 35, 40),
      sourceFrame: new Rectangle(0, 0, 35, 40),
      name: "projectileSprite");
    _uiManager.AddUIElement("player", _healthBar);
    _drawManager.AddSprite(_healthBar);
  }

  public void UnloadContent()
  {
    if (_healthBar != null)
    {
      _drawManager.RemoveSprite(_healthBar);
      _uiManager.RemoveUIElement("player", _healthBar);
      _healthBar = null;
    }
  }
}
