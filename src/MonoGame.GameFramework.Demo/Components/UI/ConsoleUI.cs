using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using MonoGame.GameFramework.Events;
using MonoGame.GameFramework.Persistence;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.Text;
using MonoGame.GameFramework.UI;

namespace MonoGame.GameFramework.Demo.Components.UI;
public class ConsoleUI
{
  private UIManager _uiManager;
  private DrawManager _drawManager;
  private TextManager _textManager;
  private SettingsManager _settingsManager;
  private Rectangle backgroundRectangle;
  private SpriteSheet consoleSpriteSheet;
  private int consoleHeight = 125;
  private EventManager _eventManager;

  public ConsoleUI(ServiceProvider serviceProvider)
  {
    _uiManager = serviceProvider.GetService<UIManager>();
    _drawManager = serviceProvider.GetService<DrawManager>();
    _textManager = serviceProvider.GetService<TextManager>();
    _settingsManager = serviceProvider.GetService<SettingsManager>();
    _eventManager = serviceProvider.GetService<EventManager>();

    backgroundRectangle = new Rectangle(0, 0, _settingsManager.WindowWidth, consoleHeight);

    consoleSpriteSheet = SpriteSheet.Static(Primitives.Pixel, backgroundRectangle, sourceFrame: backgroundRectangle, name: "console");
    consoleSpriteSheet.Tint = Color.Black * 0.5f;
    _uiManager.AddUIElement("console", consoleSpriteSheet);
    _drawManager.AddSprite(consoleSpriteSheet);

    _eventManager.Subscribe("toggleConsole", ToggleConsole);
  }

  public void UnloadContent()
  {
    _eventManager.Unsubscribe("toggleConsole", ToggleConsole);
    _drawManager.RemoveSprite(consoleSpriteSheet);
    _uiManager.RemoveUIElement("console", consoleSpriteSheet);
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
