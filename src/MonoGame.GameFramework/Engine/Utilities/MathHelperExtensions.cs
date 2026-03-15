
using System;
using Microsoft.Xna.Framework;

namespace MonoGame.GameFramework.Engine.Utilities;
public static class MathHelperExtensions
{
  public static float Distance(Vector2 point1, Vector2 point2)
  {
    return Vector2.Distance(point1, point2);
  }

  public static float Angle(Vector2 point1, Vector2 point2)
  {
    return (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
  }

  public static float Clamp(float value, float min, float max)
  {
    return MathHelper.Clamp(value, min, max);
  }

  public static float ToRadians(float degrees)
  {
    return MathHelper.ToRadians(degrees);
  }

  public static float ToDegrees(float radians)
  {
    return MathHelper.ToDegrees(radians);
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

  private static readonly Random random = new Random();
}
