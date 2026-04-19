using FluentAssertions;
using MonoGame.GameFramework.Tweening;
using Xunit;

namespace MonoGame.GameFramework.Tests.Tweening;

public class EasingTests
{
  [Theory]
  [InlineData(0f)]
  [InlineData(1f)]
  public void Linear_IsIdentityAtEndpoints(float t)
  {
    Easing.Linear(t).Should().Be(t);
  }

  [Fact]
  public void AllCurves_ReturnZeroAtZero()
  {
    Easing.Linear(0).Should().Be(0);
    Easing.QuadIn(0).Should().Be(0);
    Easing.QuadOut(0).Should().Be(0);
    Easing.QuadInOut(0).Should().Be(0);
    Easing.CubicIn(0).Should().Be(0);
    Easing.CubicOut(0).Should().Be(0);
    Easing.CubicInOut(0).Should().Be(0);
    Easing.SineIn(0).Should().BeApproximately(0f, 1e-5f);
    Easing.SineOut(0).Should().BeApproximately(0f, 1e-5f);
    Easing.SineInOut(0).Should().BeApproximately(0f, 1e-5f);
    Easing.BounceOut(0).Should().BeApproximately(0f, 1e-3f);
  }

  [Fact]
  public void AllCurves_ReturnOneAtOne()
  {
    Easing.Linear(1).Should().Be(1);
    Easing.QuadIn(1).Should().Be(1);
    Easing.QuadOut(1).Should().Be(1);
    Easing.QuadInOut(1).Should().BeApproximately(1f, 1e-5f);
    Easing.CubicIn(1).Should().Be(1);
    Easing.CubicOut(1).Should().BeApproximately(1f, 1e-5f);
    Easing.CubicInOut(1).Should().BeApproximately(1f, 1e-5f);
    Easing.SineIn(1).Should().BeApproximately(1f, 1e-5f);
    Easing.SineOut(1).Should().BeApproximately(1f, 1e-5f);
    Easing.SineInOut(1).Should().BeApproximately(1f, 1e-5f);
    Easing.BounceOut(1).Should().BeApproximately(1f, 1e-3f);
  }

  [Fact]
  public void QuadIn_IsSlowAtStart()
  {
    // At t=0.5, ease-in should still be below linear's 0.5.
    Easing.QuadIn(0.5f).Should().BeLessThan(0.5f);
  }

  [Fact]
  public void QuadOut_IsFastAtStart()
  {
    // At t=0.5, ease-out should be above linear's 0.5.
    Easing.QuadOut(0.5f).Should().BeGreaterThan(0.5f);
  }

  [Theory]
  [InlineData(0.1f)]
  [InlineData(0.5f)]
  [InlineData(0.9f)]
  public void CubicInOut_SymmetricAroundHalf(float t)
  {
    float left = Easing.CubicInOut(t);
    float right = Easing.CubicInOut(1f - t);
    (left + right).Should().BeApproximately(1f, 1e-5f);
  }
}
