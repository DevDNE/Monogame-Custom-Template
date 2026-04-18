using System;
using Microsoft.Xna.Framework;

namespace MonoGame.GameFramework.Utilities;
public static class MathUtilities
{
  private static readonly Random random = new Random();

  public static float Angle(Vector2 from, Vector2 to)
  {
    return (float)Math.Atan2(to.Y - from.Y, to.X - from.X);
  }

  public static float RandomFloat(float min, float max)
  {
    return (float)random.NextDouble() * (max - min) + min;
  }

  public static int RandomInt(int min, int max)
  {
    return random.Next(min, max);
  }

  public static Vector2 RandomVector2(Rectangle rectangle)
  {
    return new Vector2(RandomFloat(rectangle.Left, rectangle.Right), RandomFloat(rectangle.Top, rectangle.Bottom));
  }
}
