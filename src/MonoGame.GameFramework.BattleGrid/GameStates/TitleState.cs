using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Lifecycle;

namespace MonoGame.GameFramework.BattleGrid.GameStates;

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

  protected override Color BackgroundColor => new(18, 22, 34);
  protected override string TitleText => "BattleGrid";
  protected override string SubtitleText => "Grid-based dueling";
  protected override string HintText => "Click Play to begin, Esc to quit.";

  protected override IReadOnlyList<ButtonSpec> GetButtons() => new[]
  {
    new ButtonSpec("play", "Play", _onPlay),
    new ButtonSpec("quit", "Quit", _onQuit),
  };
}
