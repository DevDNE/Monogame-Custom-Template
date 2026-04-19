using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Lifecycle;

namespace MonoGame.GameFramework.Roguelike.GameStates;

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

  protected override Color BackgroundColor => new(14, 16, 24);
  protected override string TitleText => "Dungeon Run";
  protected override string SubtitleText => "Turn-based dungeon crawler";
  protected override string HintText => "WASD / Arrows move   Bump monsters to attack   > on stairs to descend";
  protected override int SubtitleY => 152;

  protected override IReadOnlyList<ButtonSpec> GetButtons() => new[]
  {
    new ButtonSpec("play", "Play", _onPlay),
    new ButtonSpec("quit", "Quit", _onQuit),
  };
}
