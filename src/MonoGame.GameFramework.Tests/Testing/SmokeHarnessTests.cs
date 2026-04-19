using FluentAssertions;
using MonoGame.GameFramework.Testing;
using Xunit;

namespace MonoGame.GameFramework.Tests.Testing;

public class SmokeHarnessTests
{
  [Fact]
  public void Tick_ReturnsFalse_WhenDisabled()
  {
    SmokeHarness h = new();
    h.Enabled.Should().BeFalse();
    h.Tick().Should().BeFalse();
    h.Tick().Should().BeFalse();
  }

  [Fact]
  public void Tick_ReturnsTrue_OnceBudgetReached()
  {
    SmokeHarness h = new() { ExitAfterFrames = 3 };
    h.Tick().Should().BeFalse();
    h.Tick().Should().BeFalse();
    h.Tick().Should().BeTrue();
    h.Tick().Should().BeTrue();
  }

  [Fact]
  public void FramesSeen_IncrementsOnlyWhenEnabled()
  {
    SmokeHarness h = new();
    h.Tick();
    h.Tick();
    h.FramesSeen.Should().Be(0);
    h.ExitAfterFrames = 5;
    h.Tick();
    h.Tick();
    h.FramesSeen.Should().Be(2);
  }

  [Fact]
  public void ParseExitAfter_EmptyArgs_ReturnsNull()
    => SmokeHarness.ParseExitAfter(new string[0]).Should().BeNull();

  [Fact]
  public void ParseExitAfter_UnrelatedFlag_ReturnsNull()
    => SmokeHarness.ParseExitAfter(new[] { "--other" }).Should().BeNull();

  [Fact]
  public void ParseExitAfter_FlagWithoutValue_ReturnsNull()
    => SmokeHarness.ParseExitAfter(new[] { "--exit-after" }).Should().BeNull();

  [Fact]
  public void ParseExitAfter_NonIntegerValue_ReturnsNull()
    => SmokeHarness.ParseExitAfter(new[] { "--exit-after", "abc" }).Should().BeNull();

  [Fact]
  public void ParseExitAfter_ZeroValue_ReturnsNull()
    => SmokeHarness.ParseExitAfter(new[] { "--exit-after", "0" }).Should().BeNull();

  [Fact]
  public void ParseExitAfter_NegativeValue_ReturnsNull()
    => SmokeHarness.ParseExitAfter(new[] { "--exit-after", "-5" }).Should().BeNull();

  [Fact]
  public void ParseExitAfter_ReadsValidFlag()
  {
    SmokeHarness.ParseExitAfter(new[] { "--exit-after", "60" }).Should().Be(60);
  }

  [Fact]
  public void ParseExitAfter_ReadsFlagAmongOtherArgs()
  {
    SmokeHarness.ParseExitAfter(new[] { "-v", "--exit-after", "30", "--verbose" }).Should().Be(30);
  }

  [Fact]
  public void ParseExitAfter_NullArgs_ReturnsNull()
  {
    SmokeHarness.ParseExitAfter(null).Should().BeNull();
  }
}
