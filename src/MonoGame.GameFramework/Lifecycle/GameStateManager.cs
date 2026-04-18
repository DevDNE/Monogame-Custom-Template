using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameFramework.Lifecycle;
public class GameStateManager
{
  private Stack<GameState> stateStack = new Stack<GameState>();
  // Scratch buffer reused across frames so Update/Draw are allocation-free
  // once the stack stops growing. We snapshot into this because a state's
  // Update can legitimately call ChangeState/PushState/PopState on this
  // manager mid-iteration (e.g. a combat state transitions to post-combat
  // when the battle ends), and that would otherwise invalidate a foreach
  // over the stack.
  private readonly List<GameState> _iterationBuffer = new();

  public void PushState(GameState newState)
  {
    if (stateStack.Count > 0) stateStack.Peek().Obscuring();
    stateStack.Push(newState);
    newState.Entered();
  }

  public void PopState()
  {
    if (stateStack.Count == 0) return;
    GameState popped = stateStack.Pop();
    popped.Leaving();
    if (stateStack.Count > 0) stateStack.Peek().Revealed();
  }

  public void ChangeState(GameState newState)
  {
    if (stateStack.Count > 0)
    {
      GameState old = stateStack.Pop();
      old.Leaving();
    }
    stateStack.Push(newState);
    newState.Entered();
  }

  public GameState PeekState()
  {
    return stateStack.Peek();
  }

  public void Update(GameTime gameTime)
  {
    _iterationBuffer.Clear();
    _iterationBuffer.AddRange(stateStack);
    foreach (GameState state in _iterationBuffer)
    {
      if (state.IsActive)
      {
        state.Update(gameTime);
      }
    }
  }

  public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
  {
    _iterationBuffer.Clear();
    _iterationBuffer.AddRange(stateStack);
    foreach (GameState state in _iterationBuffer)
    {
      if (state.IsActive)
      {
        state.Draw(spriteBatch, gameTime);
      }
    }
  }
}
