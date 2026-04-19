using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Lifecycle;

namespace MonoGame.GameFramework.Platformer.GameStates;

public class TitleState : TitleScreenState
{
  private readonly Action _onPlay;
  private readonly Action _onQuit;

  public TitleState(ServiceProvider sp, SpriteFont font, int vw, int vh, Action onPlay, Action onQuit)
    : base(sp, font, vw, vh)
  {
    _onPlay = onPlay;
    _onQuit = onQuit;
  }

  protected override Color BackgroundColor => new(20, 24, 40);
  protected override string TitleText => "Platformer Sample";
  protected override string HintText => "Click Play to begin, Esc to quit.";
  protected override int TitleY => 140;
  protected override int ButtonBlockStartY => ViewportHeight / 2 - 20;
  protected override Color NormalButtonColor => new(60, 72, 100);
  protected override Color HoverButtonColor => new(95, 110, 150);

  protected override IReadOnlyList<ButtonSpec> GetButtons() => new[]
  {
    new ButtonSpec("play", "Play", _onPlay),
    new ButtonSpec("quit", "Quit", _onQuit),
  };
}
