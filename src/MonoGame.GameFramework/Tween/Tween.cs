using System;
using Microsoft.Xna.Framework;

namespace MonoGame.GameFramework.Tween;

public class Tween<T>
{
  public T From { get; }
  public T To { get; }
  public float Duration { get; }
  public float Elapsed { get; private set; }
  public bool IsComplete => Elapsed >= Duration;
  public T Current { get; private set; }

  private readonly Func<T, T, float, T> _interpolator;
  private readonly Func<float, float> _easing;

  public Tween(T from, T to, float duration, Func<T, T, float, T> interpolator, Func<float, float> easing = null)
  {
    From = from;
    To = to;
    Duration = duration;
    _interpolator = interpolator ?? throw new ArgumentNullException(nameof(interpolator));
    _easing = easing ?? Easing.Linear;
    Current = duration <= 0f ? to : from;
  }

  public T Update(float deltaSeconds)
  {
    if (IsComplete) return Current;
    Elapsed = Math.Min(Elapsed + deltaSeconds, Duration);
    float t = Duration <= 0f ? 1f : Elapsed / Duration;
    Current = _interpolator(From, To, _easing(t));
    return Current;
  }

  public void Reset() => Elapsed = 0f;
}

public static class Tween
{
  public static Tween<float> Float(float from, float to, float duration, Func<float, float> easing = null)
    => new(from, to, duration, MathHelper.Lerp, easing);

  public static Tween<Vector2> Vec2(Vector2 from, Vector2 to, float duration, Func<float, float> easing = null)
    => new(from, to, duration, Vector2.Lerp, easing);

  public static Tween<Color> Color(Color from, Color to, float duration, Func<float, float> easing = null)
    => new(from, to, duration, Microsoft.Xna.Framework.Color.Lerp, easing);
}
