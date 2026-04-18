using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameFramework.Lifecycle;
public class GameStateManager
{
  private Stack<GameState> stateStack = new Stack<GameState>();

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
    foreach (GameState state in stateStack)
    {
      if (state.IsActive)
      {
        state.Update(gameTime);
      }
    }
  }

  public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
  {
    foreach (GameState state in stateStack)
    {
      if (state.IsActive)
      {
        state.Draw(spriteBatch, gameTime);
      }
    }
  }
}
