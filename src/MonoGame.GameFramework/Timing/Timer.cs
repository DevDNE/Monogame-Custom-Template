using System;

namespace MonoGame.GameFramework.Timing;

public class Timer
{
  public float Duration { get; }
  public float Elapsed { get; private set; }
  public bool IsRepeating { get; }
  public bool IsCancelled { get; private set; }
  public bool IsComplete => !IsRepeating && Elapsed >= Duration;
  public float Progress => Duration <= 0f ? 1f : Math.Clamp(Elapsed / Duration, 0f, 1f);

  private readonly Action _onComplete;
  private readonly Action<float> _onProgress;

  internal Timer(float duration, Action onComplete, bool repeating = false, Action<float> onProgress = null)
  {
    Duration = duration;
    IsRepeating = repeating;
    _onComplete = onComplete;
    _onProgress = onProgress;
  }

  public void Update(float deltaSeconds)
  {
    if (IsCancelled || IsComplete) return;

    Elapsed += deltaSeconds;
    _onProgress?.Invoke(Progress);

    if (Elapsed >= Duration)
    {
      _onComplete?.Invoke();
      if (IsRepeating)
      {
        Elapsed -= Duration;
      }
    }
  }

  public void Cancel() => IsCancelled = true;
}
