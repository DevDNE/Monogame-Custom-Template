using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Lifecycle;

namespace MonoGame.GameFramework.Rhythm.GameStates;

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

  protected override Color BackgroundColor => new(18, 20, 32);
  protected override string TitleText => "Beat Lanes";
  protected override string SubtitleText => "4-lane rhythm — hit notes as they cross the line";
  protected override string HintText => "Keys: D  F  J  K    Esc to quit";
  protected override int SubtitleY => 160;

  protected override IReadOnlyList<ButtonSpec> GetButtons() => new[]
  {
    new ButtonSpec("play", "Play", _onPlay),
    new ButtonSpec("quit", "Quit", _onQuit),
  };
}
