using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Persistence;

namespace MonoGame.GameFramework.VisualNovel.GameStates;

public class TitleState : TitleScreenState
{
  private readonly SaveSystem _saves;
  private readonly string _savePath;
  private readonly Action<DialogueState> _onPlay;
  private readonly Action _onQuit;

  public TitleState(
    ServiceProvider sp, SpriteFont font, int vw, int vh,
    string savePath,
    Action<DialogueState> onPlay, Action onQuit)
    : base(sp, font, vw, vh)
  {
    _saves = sp.GetService<SaveSystem>();
    _savePath = savePath;
    _onPlay = onPlay;
    _onQuit = onQuit;
  }

  protected override Color BackgroundColor => new(26, 22, 36);
  protected override string TitleText => "A Meeting at the Diner";
  protected override string SubtitleText => "A 3-choice visual novel";
  protected override int ButtonWidth => 280;
  protected override int ButtonGap => 14;
  protected override Color NormalButtonColor => new(70, 62, 110);
  protected override Color HoverButtonColor => new(100, 90, 160);

  protected override IReadOnlyList<ButtonSpec> GetButtons()
  {
    bool hasSave = _saves.Exists(_savePath);
    return new[]
    {
      new ButtonSpec("new", "New game", StartNew),
      new ButtonSpec("continue", hasSave ? "Continue" : "(no save)", ContinueSave, Enabled: hasSave),
      new ButtonSpec("quit", "Quit", _onQuit),
    };
  }

  private void StartNew()
  {
    if (_saves.Exists(_savePath)) _saves.Delete(_savePath);
    _onPlay(new DialogueState());
  }

  private void ContinueSave()
  {
    if (_saves.TryLoad(_savePath, out SaveFile<DialogueState> file)) _onPlay(file.Data);
  }
}
