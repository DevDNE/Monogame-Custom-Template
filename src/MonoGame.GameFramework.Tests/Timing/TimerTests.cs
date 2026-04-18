using FluentAssertions;
using MonoGame.GameFramework.Timing;
using Xunit;

namespace MonoGame.GameFramework.Tests.Timing;

public class TimerTests
{
  [Fact]
  public void After_FiresOnceAtDuration()
  {
    int fired = 0;
    Timer t = new(1f, () => fired++);
    t.Update(0.5f);
    fired.Should().Be(0);
    t.Update(0.6f);
    fired.Should().Be(1);
    t.IsComplete.Should().BeTrue();
  }

  [Fact]
  public void Repeating_FiresEveryInterval()
  {
    int fired = 0;
    Timer t = new(1f, () => fired++, repeating: true);
    t.Update(1f);
    t.Update(1f);
    t.Update(1f);
    fired.Should().Be(3);
    t.IsComplete.Should().BeFalse();
  }

  [Fact]
  public void Progress_ClampedBetweenZeroAndOne()
  {
    Timer t = new(2f, null, onProgress: _ => { });
    t.Progress.Should().Be(0f);
    t.Update(1f);
    t.Progress.Should().Be(0.5f);
    t.Update(5f);
    t.Progress.Should().Be(1f);
  }

  [Fact]
  public void OnProgress_ReceivesClampedProgress()
  {
    float last = -1f;
    Timer t = new(1f, null, onProgress: p => last = p);
    t.Update(0.25f);
    last.Should().BeApproximately(0.25f, 1e-5f);
    t.Update(5f);
    last.Should().Be(1f);
  }

  [Fact]
  public void Cancel_StopsProgressionAndCallbacks()
  {
    int fired = 0;
    Timer t = new(1f, () => fired++);
    t.Cancel();
    t.Update(2f);
    fired.Should().Be(0);
    t.IsCancelled.Should().BeTrue();
  }

  [Fact]
  public void ZeroDuration_ProgressIsOne()
  {
    Timer t = new(0f, null);
    t.Progress.Should().Be(1f);
  }
}
