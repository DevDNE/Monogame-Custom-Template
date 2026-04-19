using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Lifecycle;

namespace MonoGame.GameFramework.TowerDefense.GameStates;

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
  protected override string TitleText => "Tower Defense";
  protected override string SubtitleText => "Place towers, survive 3 waves";
  protected override string HintText => "Click an open cell to place a tower. Each tower costs 20 gold.";
  protected override int SubtitleY => 152;

  protected override IReadOnlyList<ButtonSpec> GetButtons() => new[]
  {
    new ButtonSpec("play", "Play", _onPlay),
    new ButtonSpec("quit", "Quit", _onQuit),
  };
}
