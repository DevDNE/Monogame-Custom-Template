using System.Linq;
using FluentAssertions;
using MonoGame.GameFramework.UI;
using Xunit;

namespace MonoGame.GameFramework.Tests.UI;

public class LogBoxTests
{
  [Fact]
  public void Add_TrimsToMaxLines()
  {
    LogBox box = new(maxLines: 3);
    box.Add("a");
    box.Add("b");
    box.Add("c");
    box.Add("d");
    box.Count.Should().Be(3);
    box.Lines.ToArray().Should().Equal("b", "c", "d");
  }

  [Fact]
  public void Clear_EmptiesQueue()
  {
    LogBox box = new();
    box.Add("a");
    box.Add("b");
    box.Clear();
    box.Count.Should().Be(0);
  }

  [Fact]
  public void Add_Preserves_Order_OldestFirst()
  {
    LogBox box = new(maxLines: 6);
    box.Add("first");
    box.Add("second");
    box.Add("third");
    box.Lines.ToArray().Should().Equal("first", "second", "third");
  }

  [Fact]
  public void DefaultParameters_AreSensible()
  {
    LogBox box = new();
    box.MaxLines.Should().Be(6);
    box.FadeStart.Should().BeApproximately(0.55f, 1e-5f);
    box.FadeStep.Should().BeApproximately(0.08f, 1e-5f);
  }
}
