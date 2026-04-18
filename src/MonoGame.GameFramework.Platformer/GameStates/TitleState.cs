using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.UI;

namespace MonoGame.GameFramework.Platformer.GameStates;

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

  public TitleState(
    ServiceProvider serviceProvider,
    SpriteFont font,
    int viewportWidth,
    int viewportHeight,
    Action onPlay,
    Action onQuit)
  {
    _uiManager = serviceProvider.GetService<UIManager>();
    _font = font;
    _viewportWidth = viewportWidth;
    _viewportHeight = viewportHeight;
    _onPlay = onPlay;
    _onQuit = onQuit;
  }

  public override void Entered()
  {
    int cx = _viewportWidth / 2;
    int playY = _viewportHeight / 2 - 20;
    int quitY = playY + ButtonHeight + 16;

    _playButton = SpriteSheet.Static(
      Primitives.Pixel,
      new Rectangle(cx - ButtonWidth / 2, playY, ButtonWidth, ButtonHeight),
      name: "play");
    _quitButton = SpriteSheet.Static(
      Primitives.Pixel,
      new Rectangle(cx - ButtonWidth / 2, quitY, ButtonWidth, ButtonHeight),
      name: "quit");

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

  public override void Update(GameTime gameTime)
  {
    // UIManager.Update (driven by Game1) handles hover + click dispatch.
  }

  public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
  {
    spriteBatch.Begin();
    Primitives.DrawRectangle(spriteBatch, new Rectangle(0, 0, _viewportWidth, _viewportHeight), new Color(20, 24, 40));

    DrawButton(spriteBatch, _playButton, "Play");
    DrawButton(spriteBatch, _quitButton, "Quit");

    const string title = "Platformer Sample";
    Vector2 titleSize = _font.MeasureString(title);
    Vector2 titlePos = new(_viewportWidth / 2f - titleSize.X / 2f, 140);
    spriteBatch.DrawString(_font, title, titlePos, Color.White);

    const string hint = "Click Play to begin, Esc to quit.";
    Vector2 hintSize = _font.MeasureString(hint);
    Vector2 hintPos = new(_viewportWidth / 2f - hintSize.X / 2f, _viewportHeight - 60);
    spriteBatch.DrawString(_font, hint, hintPos, new Color(180, 180, 200));

    spriteBatch.End();
  }

  private void DrawButton(SpriteBatch spriteBatch, SpriteSheet button, string label)
  {
    bool hovered = _uiManager.HoveredElement == button;
    Color bg = hovered ? new Color(95, 110, 150) : new Color(60, 72, 100);
    Primitives.DrawRectangle(spriteBatch, button.DestinationFrame, bg);

    Vector2 size = _font.MeasureString(label);
    Vector2 pos = new(
      button.DestinationFrame.Center.X - size.X / 2f,
      button.DestinationFrame.Center.Y - size.Y / 2f);
    spriteBatch.DrawString(_font, label, pos, Color.White);
  }
}
