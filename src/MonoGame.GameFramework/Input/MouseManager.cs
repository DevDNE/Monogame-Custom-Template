using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGame.GameFramework.Input;
public class MouseManager
{
  private MouseState previousMouseState;
  private MouseState currentMouseState;

  public void Update()
  {
    previousMouseState = currentMouseState;
    currentMouseState = Mouse.GetState();
  }
  public Vector2 GetMousePosition()
  {
    return new Vector2(currentMouseState.X, currentMouseState.Y);
  }
  public bool IsLeftMouseButtonDown() => currentMouseState.LeftButton == ButtonState.Pressed;
  public bool IsRightMouseButtonDown() => currentMouseState.RightButton == ButtonState.Pressed;
  public bool IsMiddleMouseButtonDown() => currentMouseState.MiddleButton == ButtonState.Pressed;
  public bool WasLeftMouseButtonPressed()
    => previousMouseState.LeftButton == ButtonState.Released && currentMouseState.LeftButton == ButtonState.Pressed;
  public bool WasRightMouseButtonPressed()
    => previousMouseState.RightButton == ButtonState.Released && currentMouseState.RightButton == ButtonState.Pressed;
  public bool WasMiddleMouseButtonPressed()
    => previousMouseState.MiddleButton == ButtonState.Released && currentMouseState.MiddleButton == ButtonState.Pressed;
  public bool WasLeftMouseButtonReleased()
    => previousMouseState.LeftButton == ButtonState.Pressed && currentMouseState.LeftButton == ButtonState.Released;
  public bool WasRightMouseButtonReleased()
    => previousMouseState.RightButton == ButtonState.Pressed && currentMouseState.RightButton == ButtonState.Released;
  public bool WasMiddleMouseButtonReleased()
    => previousMouseState.MiddleButton == ButtonState.Pressed && currentMouseState.MiddleButton == ButtonState.Released;
  public bool IsMouseScrollWheelUp()
    => previousMouseState.ScrollWheelValue < currentMouseState.ScrollWheelValue;
  public bool IsMouseScrollWheelDown()
    => previousMouseState.ScrollWheelValue > currentMouseState.ScrollWheelValue;
}
