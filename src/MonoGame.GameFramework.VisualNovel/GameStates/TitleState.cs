using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Persistence;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.UI;

namespace MonoGame.GameFramework.VisualNovel.GameStates;

public class TitleState : GameState
{
  private const int ButtonWidth = 280;
  private const int ButtonHeight = 56;

  private readonly UIManager _uiManager;
  private readonly SaveSystem _saves;
  private readonly SpriteFont _font;
  private readonly int _viewportWidth;
  private readonly int _viewportHeight;
  private readonly string _savePath;
  private readonly Action<DialogueState> _onPlay;
  private readonly Action _onQuit;

  private SpriteSheet _newButton;
  private SpriteSheet _continueButton;
  private SpriteSheet _quitButton;
  private bool _hasSave;

  public TitleState(
    ServiceProvider sp, SpriteFont font, int vw, int vh,
    string savePath,
    Action<DialogueState> onPlay, Action onQuit)
  {
    _uiManager = sp.GetService<UIManager>();
    _saves = sp.GetService<SaveSystem>();
    _font = font;
    _viewportWidth = vw;
    _viewportHeight = vh;
    _savePath = savePath;
    _onPlay = onPlay;
    _onQuit = onQuit;
  }

  public override void Entered()
  {
    _hasSave = _saves.Exists(_savePath);
    int cx = _viewportWidth / 2;
    int newY = _viewportHeight / 2 - 10;
    int contY = newY + ButtonHeight + 14;
    int quitY = contY + ButtonHeight + 14;

    _newButton = SpriteSheet.Static(Primitives.Pixel, new Rectangle(cx - ButtonWidth / 2, newY, ButtonWidth, ButtonHeight), name: "new");
    _continueButton = SpriteSheet.Static(Primitives.Pixel, new Rectangle(cx - ButtonWidth / 2, contY, ButtonWidth, ButtonHeight), name: "continue");
    _quitButton = SpriteSheet.Static(Primitives.Pixel, new Rectangle(cx - ButtonWidth / 2, quitY, ButtonWidth, ButtonHeight), name: "quit");

    _uiManager.AddUIElement("title", _newButton);
    _uiManager.AddUIElement("title", _continueButton);
    _uiManager.AddUIElement("title", _quitButton);

    _uiManager.OnClick(_newButton, () =>
    {
      if (_saves.Exists(_savePath)) _saves.Delete(_savePath);
      _onPlay(new DialogueState());
    });
    _uiManager.OnClick(_continueButton, () =>
    {
      if (!_hasSave) return;
      if (_saves.TryLoad(_savePath, out SaveFile<DialogueState> file)) _onPlay(file.Data);
    });
    _uiManager.OnClick(_quitButton, _onQuit);

    IsActive = true;
  }

  public override void Leaving()
  {
    _uiManager.RemoveUIElement("title", _newButton);
    _uiManager.RemoveUIElement("title", _continueButton);
    _uiManager.RemoveUIElement("title", _quitButton);
  }

  public override void Obscuring() => IsActive = false;
  public override void Revealed() { _hasSave = _saves.Exists(_savePath); IsActive = true; }
  public override void Update(GameTime gameTime) { }

  public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
  {
    spriteBatch.Begin();
    Primitives.DrawRectangle(spriteBatch, new Rectangle(0, 0, _viewportWidth, _viewportHeight), new Color(26, 22, 36));

    DrawButton(spriteBatch, _newButton, "New game", enabled: true);
    DrawButton(spriteBatch, _continueButton, _hasSave ? "Continue" : "(no save)", enabled: _hasSave);
    DrawButton(spriteBatch, _quitButton, "Quit", enabled: true);

    const string title = "A Meeting at the Diner";
    Vector2 ts = _font.MeasureString(title);
    spriteBatch.DrawString(_font, title, new Vector2(_viewportWidth / 2f - ts.X / 2f, 120), Color.White);
    const string sub = "A 3-choice visual novel";
    Vector2 ss = _font.MeasureString(sub);
    spriteBatch.DrawString(_font, sub, new Vector2(_viewportWidth / 2f - ss.X / 2f, 156), new Color(180, 180, 210));

    spriteBatch.End();
  }

  private void DrawButton(SpriteBatch spriteBatch, SpriteSheet button, string label, bool enabled)
  {
    bool hovered = enabled && _uiManager.HoveredElement == button;
    Color bg = !enabled
      ? new Color(40, 40, 55)
      : hovered ? new Color(100, 90, 160) : new Color(70, 62, 110);
    Primitives.DrawRectangle(spriteBatch, button.DestinationFrame, bg);
    Vector2 size = _font.MeasureString(label);
    spriteBatch.DrawString(_font, label,
      new Vector2(button.DestinationFrame.Center.X - size.X / 2f, button.DestinationFrame.Center.Y - size.Y / 2f),
      enabled ? Color.White : new Color(150, 150, 165));
  }
}
