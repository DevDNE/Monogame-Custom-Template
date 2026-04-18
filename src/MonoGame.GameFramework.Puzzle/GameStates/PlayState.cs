using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.GameFramework.Input;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Rendering;

namespace MonoGame.GameFramework.Puzzle.GameStates;

public class PlayState : GameState
{
  private readonly KeyboardManager _keyboard;
  private readonly MouseManager _mouse;
  private readonly SpriteFont _font;
  private readonly int _viewportWidth;
  private readonly int _viewportHeight;

  private Board _board;
  private (int c, int r)? _selected;
  private string _lastEvent = "";

  public PlayState(ServiceProvider sp, SpriteFont font, int vw, int vh)
  {
    _keyboard = sp.GetService<KeyboardManager>();
    _mouse = sp.GetService<MouseManager>();
    _font = font;
    _viewportWidth = vw;
    _viewportHeight = vh;
  }

  public override void Entered()
  {
    Vector2 origin = new(
      (_viewportWidth - Board.Columns * Board.CellSize) * 0.5f,
      (_viewportHeight - Board.Rows * Board.CellSize) * 0.5f + 16);
    _board = new Board(origin);
    _selected = null;
    _lastEvent = "";
    IsActive = true;
  }

  public override void Leaving() { }
  public override void Obscuring() => IsActive = false;
  public override void Revealed() => IsActive = true;

  public override void Update(GameTime gameTime)
  {
    if (_keyboard.WasKeyPressed(Keys.R)) { _board.FillRandomNoMatches(); _selected = null; _lastEvent = "New board"; return; }

    if (_mouse.WasLeftMouseButtonPressed())
    {
      Vector2 mouse = _mouse.GetMousePosition();
      (int col, int row) = _board.Map.WorldToCell(mouse);
      if (col < 0 || col >= Board.Columns || row < 0 || row >= Board.Rows) { _selected = null; return; }

      if (_selected is null)
      {
        _selected = (col, row);
        _lastEvent = $"Selected ({col},{row})";
      }
      else
      {
        (int c, int r) first = _selected.Value;
        if (first == (col, row))
        {
          _selected = null;
          _lastEvent = "Deselected";
        }
        else if (_board.AreAdjacent(first, (col, row)))
        {
          bool matched = _board.TrySwap(first, (col, row));
          _lastEvent = matched ? $"Match! Score {_board.Score}" : "No match - reverted";
          _selected = null;
        }
        else
        {
          // Not adjacent: treat as new selection.
          _selected = (col, row);
          _lastEvent = $"Selected ({col},{row})";
        }
      }
    }
  }

  public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
  {
    spriteBatch.Begin();
    Primitives.DrawRectangle(spriteBatch, new Rectangle(0, 0, _viewportWidth, _viewportHeight), new Color(22, 24, 36));
    DrawBoard(spriteBatch);
    DrawHud(spriteBatch);
    spriteBatch.End();
  }

  private void DrawBoard(SpriteBatch spriteBatch)
  {
    // Board background
    Rectangle bg = _board.Map.GetCellRect(0, 0);
    Rectangle last = _board.Map.GetCellRect(Board.Columns - 1, Board.Rows - 1);
    Rectangle fullBoard = new(bg.X - 6, bg.Y - 6, last.Right - bg.X + 12, last.Bottom - bg.Y + 12);
    Primitives.DrawRectangle(spriteBatch, fullBoard, new Color(40, 48, 66));

    for (int r = 0; r < Board.Rows; r++)
    {
      for (int c = 0; c < Board.Columns; c++)
      {
        Rectangle cell = _board.Map.GetCellRect(c, r);
        Primitives.DrawRectangle(spriteBatch, cell, new Color(30, 36, 52));
        Board.Gem g = _board.Gems[c, r];
        if (g == Board.Gem.Empty) continue;
        Rectangle inset = new(
          cell.X + Board.GemInset,
          cell.Y + Board.GemInset,
          cell.Width - Board.GemInset * 2,
          cell.Height - Board.GemInset * 2);
        Primitives.DrawRectangle(spriteBatch, inset, Board.ColorOf(g));
      }
    }

    if (_selected is { } sel)
    {
      Rectangle cell = _board.Map.GetCellRect(sel.c, sel.r);
      // Draw a 2-pixel white outline.
      Primitives.DrawRectangle(spriteBatch, new Rectangle(cell.X, cell.Y, cell.Width, 2), Color.White);
      Primitives.DrawRectangle(spriteBatch, new Rectangle(cell.X, cell.Bottom - 2, cell.Width, 2), Color.White);
      Primitives.DrawRectangle(spriteBatch, new Rectangle(cell.X, cell.Y, 2, cell.Height), Color.White);
      Primitives.DrawRectangle(spriteBatch, new Rectangle(cell.Right - 2, cell.Y, 2, cell.Height), Color.White);
    }
  }

  private void DrawHud(SpriteBatch spriteBatch)
  {
    spriteBatch.DrawString(_font, $"Score {_board.Score}", new Vector2(20, 20), Color.White);
    if (!string.IsNullOrEmpty(_lastEvent))
    {
      Vector2 sz = _font.MeasureString(_lastEvent);
      spriteBatch.DrawString(_font, _lastEvent, new Vector2(_viewportWidth - sz.X - 20, 20), new Color(200, 210, 230));
    }
    const string hint = "Click two adjacent gems to swap   R reshuffle   Esc quit";
    Vector2 hs = _font.MeasureString(hint);
    spriteBatch.DrawString(_font, hint, new Vector2(_viewportWidth * 0.5f - hs.X * 0.5f, _viewportHeight - 30), new Color(180, 180, 200));
  }
}
