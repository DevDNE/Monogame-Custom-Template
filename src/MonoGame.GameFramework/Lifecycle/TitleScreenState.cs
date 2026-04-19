using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.UI;

namespace MonoGame.GameFramework.Lifecycle;

public abstract class TitleScreenState : GameState
{
  public sealed record ButtonSpec(string Id, string Label, Action OnClick, bool Enabled = true);

  protected readonly UIManager UI;
  protected readonly SpriteFont Font;
  protected readonly int ViewportWidth;
  protected readonly int ViewportHeight;

  private readonly List<(ButtonSpec Spec, SpriteSheet Sprite)> _buttons = new();

  public IReadOnlyList<SpriteSheet> RegisteredButtons
  {
    get
    {
      SpriteSheet[] copy = new SpriteSheet[_buttons.Count];
      for (int i = 0; i < _buttons.Count; i++) copy[i] = _buttons[i].Sprite;
      return copy;
    }
  }

  protected virtual int ButtonWidth => 240;
  protected virtual int ButtonHeight => 56;
  protected virtual int ButtonGap => 16;
  protected virtual int ButtonBlockStartY => ViewportHeight / 2 - 10;
  protected virtual string GroupName => "title";
  protected virtual int TitleY => 120;
  protected virtual int SubtitleY => 156;
  protected virtual Color TitleColor => Color.White;
  protected virtual Color SubtitleColor => new(170, 180, 210);
  protected virtual Color HintColor => new(180, 180, 200);
  protected virtual Color NormalButtonColor => new(55, 70, 110);
  protected virtual Color HoverButtonColor => new(90, 120, 170);
  protected virtual Color DisabledButtonColor => new(40, 40, 55);
  protected virtual Color ButtonLabelColor => Color.White;
  protected virtual Color DisabledLabelColor => new(150, 150, 165);

  protected abstract Color BackgroundColor { get; }
  protected abstract string TitleText { get; }
  protected virtual string SubtitleText => "";
  protected virtual string HintText => "";

  protected abstract IReadOnlyList<ButtonSpec> GetButtons();

  protected TitleScreenState(IServiceProvider sp, SpriteFont font, int viewportWidth, int viewportHeight)
  {
    UI = sp.GetService<UIManager>();
    Font = font;
    ViewportWidth = viewportWidth;
    ViewportHeight = viewportHeight;
  }

  public override void Entered()
  {
    RegisterButtons();
    IsActive = true;
  }

  public override void Leaving()
  {
    UnregisterButtons();
  }

  public override void Obscuring() => IsActive = false;

  public override void Revealed()
  {
    RefreshButtons();
    IsActive = true;
  }

  public override void Update(GameTime gameTime) { }

  protected void RefreshButtons()
  {
    UnregisterButtons();
    RegisterButtons();
  }

  private void RegisterButtons()
  {
    IReadOnlyList<ButtonSpec> specs = GetButtons();
    int cx = ViewportWidth / 2;
    int y = ButtonBlockStartY;
    foreach (ButtonSpec spec in specs)
    {
      Rectangle bounds = new(cx - ButtonWidth / 2, y, ButtonWidth, ButtonHeight);
      SpriteSheet sprite = CreateButtonSprite(spec.Id, bounds);
      UI.AddUIElement(GroupName, sprite);
      if (spec.Enabled && spec.OnClick != null)
      {
        UI.OnClick(sprite, spec.OnClick);
      }
      _buttons.Add((spec, sprite));
      y += ButtonHeight + ButtonGap;
    }
  }

  protected virtual SpriteSheet CreateButtonSprite(string id, Rectangle bounds)
    => SpriteSheet.Static(Primitives.Pixel, bounds, name: id);

  private void UnregisterButtons()
  {
    foreach ((_, SpriteSheet sprite) in _buttons)
    {
      UI.RemoveUIElement(GroupName, sprite);
    }
    _buttons.Clear();
  }

  public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
  {
    spriteBatch.Begin();
    Primitives.DrawRectangle(spriteBatch, new Rectangle(0, 0, ViewportWidth, ViewportHeight), BackgroundColor);

    foreach ((ButtonSpec spec, SpriteSheet sprite) in _buttons)
    {
      DrawButton(spriteBatch, sprite, spec);
    }

    if (!string.IsNullOrEmpty(TitleText))
    {
      Vector2 ts = Font.MeasureString(TitleText);
      spriteBatch.DrawString(Font, TitleText, new Vector2(ViewportWidth / 2f - ts.X / 2f, TitleY), TitleColor);
    }
    if (!string.IsNullOrEmpty(SubtitleText))
    {
      Vector2 ss = Font.MeasureString(SubtitleText);
      spriteBatch.DrawString(Font, SubtitleText, new Vector2(ViewportWidth / 2f - ss.X / 2f, SubtitleY), SubtitleColor);
    }
    if (!string.IsNullOrEmpty(HintText))
    {
      Vector2 hs = Font.MeasureString(HintText);
      spriteBatch.DrawString(Font, HintText, new Vector2(ViewportWidth / 2f - hs.X / 2f, ViewportHeight - 60), HintColor);
    }

    spriteBatch.End();
  }

  private void DrawButton(SpriteBatch spriteBatch, SpriteSheet sprite, ButtonSpec spec)
  {
    bool hovered = spec.Enabled && UI.HoveredElement == sprite;
    Color bg = !spec.Enabled
      ? DisabledButtonColor
      : hovered ? HoverButtonColor : NormalButtonColor;
    Primitives.DrawRectangle(spriteBatch, sprite.DestinationFrame, bg);
    Vector2 size = Font.MeasureString(spec.Label);
    spriteBatch.DrawString(Font, spec.Label,
      new Vector2(sprite.DestinationFrame.Center.X - size.X / 2f, sprite.DestinationFrame.Center.Y - size.Y / 2f),
      spec.Enabled ? ButtonLabelColor : DisabledLabelColor);
  }
}
