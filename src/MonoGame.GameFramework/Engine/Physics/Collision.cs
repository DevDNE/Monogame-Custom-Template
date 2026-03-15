using Microsoft.Xna.Framework;

namespace MonoGame.GameFramework.Engine.Physics;
public class Collision
{
  public static bool CheckCollisions(Rectangle sprite1, Rectangle sprite2)
  {
    int left = sprite2.Left - sprite1.Right;
    int right = sprite2.Right - sprite1.Left;
    int top = sprite2.Top - sprite1.Bottom;
    int bottom = sprite2.Bottom - sprite1.Top;

    if (left <= 0 && right >= 0 && top <= 0 && bottom >= 0)
    {
      return true;
    }

    return false;
  }
}
