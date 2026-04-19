using System;
using FluentAssertions;
using Microsoft.Xna.Framework;
using MonoGame.GameFramework.Timing;
using Xunit;

namespace MonoGame.GameFramework.Tests.Timing;

public class TimerManagerTests
{
  private static GameTime Step(double seconds)
    => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(seconds));

  [Fact]
  public void After_FiresOnceAndIsRemoved()
  {
    TimerManager tm = new();
    int fired = 0;
    Timer t = tm.After(0.5f, () => fired++);

    tm.Update(Step(0.6));
    fired.Should().Be(1);
    t.IsComplete.Should().BeTrue();

    tm.Update(Step(10));
    fired.Should().Be(1);
  }

  [Fact]
  public void Every_FiresRepeatedlyAndStaysInList()
  {
    TimerManager tm = new();
    int fired = 0;
    tm.Every(0.25f, () => fired++);

    tm.Update(Step(0.25));
    tm.Update(Step(0.25));
    tm.Update(Step(0.25));
    fired.Should().Be(3);
  }

  [Fact]
  public void Over_InvokesProgressAndCompletion()
  {
    TimerManager tm = new();
    float lastProgress = -1f;
    bool done = false;
    tm.Over(1f, p => lastProgress = p, () => done = true);

    tm.Update(Step(0.5));
    lastProgress.Should().BeApproximately(0.5f, 1e-5f);
    done.Should().BeFalse();

    tm.Update(Step(0.6));
    lastProgress.Should().Be(1f);
    done.Should().BeTrue();
  }

  [Fact]
  public void Cancelled_TimerRemovedNextTick()
  {
    TimerManager tm = new();
    int fired = 0;
    Timer t = tm.After(1f, () => fired++);
    t.Cancel();

    tm.Update(Step(2));
    fired.Should().Be(0);
  }

  [Fact]
  public void MultipleTimers_IndependentLifecycle()
  {
    TimerManager tm = new();
    int shortFires = 0, longFires = 0;
    tm.After(0.25f, () => shortFires++);
    tm.After(1f, () => longFires++);

    tm.Update(Step(0.3));
    shortFires.Should().Be(1);
    longFires.Should().Be(0);

    tm.Update(Step(0.8));
    shortFires.Should().Be(1);
    longFires.Should().Be(1);
  }

  [Fact]
  public void Clear_RemovesAllPendingTimers()
  {
    TimerManager tm = new();
    int fired = 0;
    tm.After(0.5f, () => fired++);
    tm.Every(0.25f, () => fired++);

    tm.Clear();
    tm.Update(Step(5));
    fired.Should().Be(0);
  }

  [Fact]
  public void ActiveTimerCount_TracksAddAndCompletion()
  {
    TimerManager tm = new();
    tm.ActiveTimerCount.Should().Be(0);
    tm.After(0.5f, () => { });
    tm.Every(1f, () => { });
    tm.ActiveTimerCount.Should().Be(2);

    tm.Update(Step(0.6));
    tm.ActiveTimerCount.Should().Be(1);

    tm.Clear();
    tm.ActiveTimerCount.Should().Be(0);
  }
}
