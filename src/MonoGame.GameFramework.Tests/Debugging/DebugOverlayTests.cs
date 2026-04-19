using System;
using FluentAssertions;
using Microsoft.Xna.Framework;
using MonoGame.GameFramework.Debugging;
using MonoGame.GameFramework.Events;
using MonoGame.GameFramework.Input;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Timing;
using MonoGame.GameFramework.UI;
using Xunit;

namespace MonoGame.GameFramework.Tests.Debugging;

public class DebugOverlayTests
{
  private static DebugOverlay MakeOverlay(out EventManager events)
  {
    events = new EventManager();
    return new DebugOverlay(
      keyboard: new KeyboardManager(),
      events: events,
      states: new GameStateManager(),
      ui: new UIManager(mouseManager: null),
      timers: new TimerManager());
  }

  // ---- State machine ----

  [Fact]
  public void NewOverlay_StartsDisabledAndRunning()
  {
    DebugOverlay o = MakeOverlay(out _);
    o.Enabled.Should().BeFalse();
    o.IsPaused.Should().BeFalse();
    o.ShouldSkipUpdate.Should().BeFalse();
  }

  [Fact]
  public void ToggleEnabled_Flips()
  {
    DebugOverlay o = MakeOverlay(out _);
    o.ToggleEnabled();
    o.Enabled.Should().BeTrue();
    o.ToggleEnabled();
    o.Enabled.Should().BeFalse();
  }

  [Fact]
  public void Disabling_ClearsPauseAndStep()
  {
    DebugOverlay o = MakeOverlay(out _);
    o.ToggleEnabled();
    o.TogglePause();
    o.RequestStep();
    o.ToggleEnabled();  // disable
    o.IsPaused.Should().BeFalse();
  }

  [Fact]
  public void TogglePause_IgnoredWhileDisabled()
  {
    DebugOverlay o = MakeOverlay(out _);
    o.TogglePause();
    o.IsPaused.Should().BeFalse();
  }

  [Fact]
  public void ShouldSkipUpdate_TrueWhenPausedWithoutStep()
  {
    DebugOverlay o = MakeOverlay(out _);
    o.ToggleEnabled();
    o.TogglePause();
    o.Update(new GameTime());
    o.ShouldSkipUpdate.Should().BeTrue();
  }

  [Fact]
  public void StepRequest_AllowsOneUpdate_ThenResumesSkipping()
  {
    DebugOverlay o = MakeOverlay(out _);
    o.ToggleEnabled();
    o.TogglePause();
    o.RequestStep();
    o.Update(new GameTime());
    o.ShouldSkipUpdate.Should().BeFalse();   // stepped through
    o.Update(new GameTime());
    o.ShouldSkipUpdate.Should().BeTrue();    // back to paused
  }

  [Fact]
  public void ShouldSkipUpdate_FalseWhenRunning()
  {
    DebugOverlay o = MakeOverlay(out _);
    o.ToggleEnabled();
    o.Update(new GameTime());
    o.ShouldSkipUpdate.Should().BeFalse();
  }

  // ---- Watches ----

  [Fact]
  public void AddWatch_AppearsInKeysInOrder()
  {
    DebugOverlay o = MakeOverlay(out _);
    o.AddWatch("a", () => "1");
    o.AddWatch("b", () => "2");
    o.WatchKeys.Should().Equal("a", "b");
  }

  [Fact]
  public void AddWatch_DuplicateKey_ReplacesValueFn_PreservesPosition()
  {
    DebugOverlay o = MakeOverlay(out _);
    o.AddWatch("a", () => "old");
    o.AddWatch("b", () => "2");
    o.AddWatch("a", () => "new");
    o.WatchKeys.Should().Equal("a", "b");
  }

  [Fact]
  public void RemoveWatch_ReturnsTrueOnlyIfPresent()
  {
    DebugOverlay o = MakeOverlay(out _);
    o.AddWatch("a", () => "1");
    o.RemoveWatch("a").Should().BeTrue();
    o.RemoveWatch("a").Should().BeFalse();
    o.WatchKeys.Should().BeEmpty();
  }

  [Fact]
  public void AddWatch_RejectsNullValueFn()
  {
    DebugOverlay o = MakeOverlay(out _);
    o.Invoking(x => x.AddWatch("a", null)).Should().Throw<ArgumentNullException>();
  }

  // ---- Event tail ----

  [Fact]
  public void EventTail_BoundedTo12_NewestLast()
  {
    DebugOverlay o = MakeOverlay(out EventManager events);
    for (int i = 0; i < 20; i++)
    {
      events.TriggerEvent($"ev{i}", null, new GameEventArgs(""));
    }
    o.RecentEvents.Count.Should().Be(12);
    string[] arr = System.Linq.Enumerable.ToArray(o.RecentEvents);
    arr[0].Should().StartWith("ev8");    // oldest kept
    arr[^1].Should().StartWith("ev19");  // newest
  }

  [Fact]
  public void EventTail_CapturesTypedPublishByTypeName()
  {
    DebugOverlay o = MakeOverlay(out EventManager events);
    events.Publish(new MyPayload());
    o.RecentEvents.Should().ContainSingle(s => s.StartsWith(nameof(MyPayload)));
  }

  private sealed class MyPayload { }

  // ---- FPS ----

  [Fact]
  public void AverageFrameMs_AveragesRecentSamples()
  {
    DebugOverlay o = MakeOverlay(out _);
    o.ToggleEnabled();
    for (int i = 0; i < 30; i++)
    {
      o.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(16)));
    }
    o.AverageFrameMs().Should().BeApproximately(16, 0.5);
    o.AverageFps().Should().BeApproximately(62.5, 2);
  }
}
