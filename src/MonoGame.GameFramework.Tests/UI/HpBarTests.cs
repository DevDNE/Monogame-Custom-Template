using FluentAssertions;
using MonoGame.GameFramework.UI;
using Xunit;

namespace MonoGame.GameFramework.Tests.UI;

public class HpBarTests
{
  [Fact]
  public void ComputeFillWidth_FullHp_IsFullWidth()
  {
    HpBar.ComputeFillWidth(100, 50, 50).Should().Be(100);
  }

  [Fact]
  public void ComputeFillWidth_Zero_IsZero()
  {
    HpBar.ComputeFillWidth(100, 0, 50).Should().Be(0);
  }

  [Fact]
  public void ComputeFillWidth_Half_IsHalf()
  {
    HpBar.ComputeFillWidth(100, 25, 50).Should().Be(50);
  }

  [Fact]
  public void ComputeFillWidth_Negative_ClampsToZero()
  {
    HpBar.ComputeFillWidth(100, -5, 50).Should().Be(0);
  }

  [Fact]
  public void ComputeFillWidth_Overflow_ClampsToMax()
  {
    HpBar.ComputeFillWidth(100, 80, 50).Should().Be(100);
  }

  [Fact]
  public void ComputeFillWidth_ZeroMax_ReturnsZero()
  {
    HpBar.ComputeFillWidth(100, 10, 0).Should().Be(0);
  }

  [Fact]
  public void ComputeFillWidth_ZeroWidth_ReturnsZero()
  {
    HpBar.ComputeFillWidth(0, 50, 100).Should().Be(0);
  }
}
