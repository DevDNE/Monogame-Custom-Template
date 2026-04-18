using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoGame.GameFramework.Timing;

public class TimerManager
{
  private readonly List<Timer> _timers = new();

  public Timer After(float seconds, Action callback)
  {
    Timer timer = new(seconds, callback);
    _timers.Add(timer);
    return timer;
  }

  public Timer Every(float interval, Action callback)
  {
    Timer timer = new(interval, callback, repeating: true);
    _timers.Add(timer);
    return timer;
  }

  public Timer Over(float duration, Action<float> progressCallback, Action onComplete = null)
  {
    Timer timer = new(duration, onComplete, repeating: false, onProgress: progressCallback);
    _timers.Add(timer);
    return timer;
  }

  public void Update(GameTime gameTime)
  {
    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
    for (int i = _timers.Count - 1; i >= 0; i--)
    {
      Timer t = _timers[i];
      t.Update(dt);
      if (t.IsCancelled || (t.IsComplete && !t.IsRepeating))
      {
        _timers.RemoveAt(i);
      }
    }
  }

  public void Clear() => _timers.Clear();
}
