using System;
using FluentAssertions;
using Microsoft.Xna.Framework;
using MonoGame.GameFramework.Utilities;
using Xunit;

namespace MonoGame.GameFramework.Tests.Utilities;

public class MathUtilitiesTests
{
  [Fact]
  public void Angle_EastIsZero()
  {
    MathUtilities.Angle(Vector2.Zero, new Vector2(1, 0))
      .Should().BeApproximately(0f, 1e-5f);
  }

  [Fact]
  public void Angle_SouthIsPositiveHalfPi()
  {
    // MonoGame Y axis points down on screen, but Atan2 treats (y=1) as +π/2.
    MathUtilities.Angle(Vector2.Zero, new Vector2(0, 1))
      .Should().BeApproximately(MathF.PI / 2f, 1e-5f);
  }

  [Fact]
  public void Angle_WestIsPi()
  {
    MathUtilities.Angle(Vector2.Zero, new Vector2(-1, 0))
      .Should().BeApproximately(MathF.PI, 1e-5f);
  }

  [Fact]
  public void Angle_NorthIsNegativeHalfPi()
  {
    MathUtilities.Angle(Vector2.Zero, new Vector2(0, -1))
      .Should().BeApproximately(-MathF.PI / 2f, 1e-5f);
  }

  [Fact]
  public void RandomFloat_WithinBounds()
  {
    for (int i = 0; i < 200; i++)
    {
      float v = MathUtilities.RandomFloat(-3f, 7f);
      v.Should().BeGreaterThanOrEqualTo(-3f).And.BeLessThan(7f);
    }
  }

  [Fact]
  public void RandomInt_WithinBounds()
  {
    for (int i = 0; i < 200; i++)
    {
      int v = MathUtilities.RandomInt(10, 20);
      v.Should().BeInRange(10, 19);
    }
  }

  [Fact]
  public void RandomVector2_WithinRectangle()
  {
    Rectangle rect = new(50, 60, 100, 200);
    for (int i = 0; i < 200; i++)
    {
      Vector2 v = MathUtilities.RandomVector2(rect);
      v.X.Should().BeGreaterThanOrEqualTo(rect.Left).And.BeLessThan(rect.Right);
      v.Y.Should().BeGreaterThanOrEqualTo(rect.Top).And.BeLessThan(rect.Bottom);
    }
  }
}
