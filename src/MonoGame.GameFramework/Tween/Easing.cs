using System;

namespace MonoGame.GameFramework.Tweening;

public static class Easing
{
  public static float Linear(float t) => t;

  public static float QuadIn(float t) => t * t;
  public static float QuadOut(float t) => 1f - (1f - t) * (1f - t);
  public static float QuadInOut(float t) => t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) * 0.5f;

  public static float CubicIn(float t) => t * t * t;
  public static float CubicOut(float t) => 1f - MathF.Pow(1f - t, 3f);
  public static float CubicInOut(float t) => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) * 0.5f;

  public static float SineIn(float t) => 1f - MathF.Cos(t * MathF.PI * 0.5f);
  public static float SineOut(float t) => MathF.Sin(t * MathF.PI * 0.5f);
  public static float SineInOut(float t) => -(MathF.Cos(MathF.PI * t) - 1f) * 0.5f;

  public static float BounceOut(float t)
  {
    const float n1 = 7.5625f;
    const float d1 = 2.75f;
    if (t < 1f / d1) return n1 * t * t;
    if (t < 2f / d1) { t -= 1.5f / d1; return n1 * t * t + 0.75f; }
    if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }
    t -= 2.625f / d1; return n1 * t * t + 0.984375f;
  }
}
