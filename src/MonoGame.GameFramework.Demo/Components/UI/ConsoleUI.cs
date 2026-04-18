using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Events;
using MonoGame.GameFramework.Persistence;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.Text;
using MonoGame.GameFramework.UI;

namespace MonoGame.GameFramework.Demo.Components.UI;
public class ConsoleUI
{
  private UIManager _uiManager;
  private TextManager _textManager;
  private SettingsManager _settingsManager;
  private Texture2D backgroundTexture;
  private Rectangle backgroundRectangle;
  private SpriteSheet consoleSpriteSheet;
  private int consoleHeight = 125;
  private EventManager _eventManager;

  public ConsoleUI(GraphicsDevice graphicsDevice, ServiceProvider serviceProvider)
  {
    _uiManager = serviceProvider.GetService<UIManager>();
    _textManager = serviceProvider.GetService<TextManager>();
    _settingsManager = serviceProvider.GetService<SettingsManager>();
    _eventManager = serviceProvider.GetService<EventManager>();

    backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
    backgroundTexture.SetData(new[] { Color.Black * 0.5f });

    backgroundRectangle = new Rectangle(0, 0, _settingsManager.WindowWidth, consoleHeight);

    consoleSpriteSheet = SpriteSheet.Static(backgroundTexture, backgroundRectangle, sourceFrame: backgroundRectangle, name: "console");
    _uiManager.AddUIElement("console", consoleSpriteSheet);

    _eventManager.Subscribe("toggleConsole", ToggleConsole);
  }

  public void UnloadContent()
  {
    _eventManager.Unsubscribe("toggleConsole", ToggleConsole);
    if (backgroundTexture != null)
    {
      backgroundTexture.Dispose();
      backgroundTexture = null;
    }
    _uiManager.RemoveUIElement("console", consoleSpriteSheet);
    backgroundTexture = null;
  }

  public void ToggleConsole(object sender, GameEventArgs e)
  {
    consoleSpriteSheet.DestinationFrame = new Rectangle(0, 0, consoleSpriteSheet.DestinationFrame.Width, consoleSpriteSheet.DestinationFrame.Height == 0 ? consoleHeight : 0);
    _textManager.ClearGroup("console");
  }

  public void Log(string message)
  {
    if (IsConsoleOpen())
    {
      _textManager.AddText("console", message, new Vector2(0, 0), Color.White);
      _textManager.ScrollText("console", 20, 5);
    }
  }

  private bool IsConsoleOpen(){
    return consoleSpriteSheet.DestinationFrame.Height > 0;
  }
}
