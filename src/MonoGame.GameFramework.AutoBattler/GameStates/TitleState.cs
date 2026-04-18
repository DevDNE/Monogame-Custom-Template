using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.UI;

namespace MonoGame.GameFramework.AutoBattler.GameStates;

public class TitleState : GameState
{
  private const int ButtonWidth = 240;
  private const int ButtonHeight = 56;

  private readonly UIManager _uiManager;
  private readonly SpriteFont _font;
  private readonly int _viewportWidth;
  private readonly int _viewportHeight;
  private readonly Action _onPlay;
  private readonly Action _onQuit;

  private SpriteSheet _playButton;
  private SpriteSheet _quitButton;

  public TitleState(ServiceProvider sp, SpriteFont font, int vw, int vh, Action onPlay, Action onQuit)
  {
    _uiManager = sp.GetService<UIManager>();
    _font = font;
    _viewportWidth = vw;
    _viewportHeight = vh;
    _onPlay = onPlay;
    _onQuit = onQuit;
  }

  public override void Entered()
  {
    int cx = _viewportWidth / 2;
    int playY = _viewportHeight / 2 - 10;
    int quitY = playY + ButtonHeight + 16;
    _playButton = SpriteSheet.Static(Primitives.Pixel, new Rectangle(cx - ButtonWidth / 2, playY, ButtonWidth, ButtonHeight), name: "play");
    _quitButton = SpriteSheet.Static(Primitives.Pixel, new Rectangle(cx - ButtonWidth / 2, quitY, ButtonWidth, ButtonHeight), name: "quit");
    _uiManager.AddUIElement("title", _playButton);
    _uiManager.AddUIElement("title", _quitButton);
    _uiManager.OnClick(_playButton, _onPlay);
    _uiManager.OnClick(_quitButton, _onQuit);
    IsActive = true;
  }

  public override void Leaving()
  {
    _uiManager.RemoveUIElement("title", _playButton);
    _uiManager.RemoveUIElement("title", _quitButton);
  }

  public override void Obscuring() => IsActive = false;
  public override void Revealed() => IsActive = true;
  public override void Update(GameTime gameTime) { }

  public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
  {
    spriteBatch.Begin();
    Primitives.DrawRectangle(spriteBatch, new Rectangle(0, 0, _viewportWidth, _viewportHeight), new Color(18, 22, 34));

    DrawButton(spriteBatch, _playButton, "Play");
    DrawButton(spriteBatch, _quitButton, "Quit");

    const string title = "Auto Battler";
    Vector2 ts = _font.MeasureString(title);
    spriteBatch.DrawString(_font, title, new Vector2(_viewportWidth / 2f - ts.X / 2f, 120), Color.White);
    const string sub = "Buy units, let them fight, survive the enemy";
    Vector2 ss = _font.MeasureString(sub);
    spriteBatch.DrawString(_font, sub, new Vector2(_viewportWidth / 2f - ss.X / 2f, 156), new Color(170, 180, 210));
    const string hint = "Drag cards onto your side of the board. Space to start combat.";
    Vector2 hs = _font.MeasureString(hint);
    spriteBatch.DrawString(_font, hint, new Vector2(_viewportWidth / 2f - hs.X / 2f, _viewportHeight - 60), new Color(180, 180, 200));
    spriteBatch.End();
  }

  private void DrawButton(SpriteBatch spriteBatch, SpriteSheet button, string label)
  {
    bool hovered = _uiManager.HoveredElement == button;
    Color bg = hovered ? new Color(90, 120, 170) : new Color(55, 70, 110);
    Primitives.DrawRectangle(spriteBatch, button.DestinationFrame, bg);
    Vector2 size = _font.MeasureString(label);
    spriteBatch.DrawString(_font, label,
      new Vector2(button.DestinationFrame.Center.X - size.X / 2f, button.DestinationFrame.Center.Y - size.Y / 2f),
      Color.White);
  }
}
