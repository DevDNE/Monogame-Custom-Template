using System;
using FluentAssertions;
using MonoGame.GameFramework.Rendering;
using Xunit;

namespace MonoGame.GameFramework.Tests.Rendering;

public class TileLayerTests
{
  [Fact]
  public void Constructor_ExposesDimensions()
  {
    TileLayer<int> layer = new("ground", 4, 3);
    layer.Name.Should().Be("ground");
    layer.Columns.Should().Be(4);
    layer.Rows.Should().Be(3);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void Constructor_RejectsNonPositiveDimensions(int bad)
  {
    Action act = () => new TileLayer<int>("x", bad, 1);
    act.Should().Throw<ArgumentOutOfRangeException>();
    Action act2 = () => new TileLayer<int>("x", 1, bad);
    act2.Should().Throw<ArgumentOutOfRangeException>();
  }

  [Fact]
  public void Indexer_RoundtripsValue()
  {
    TileLayer<int> layer = new("ground", 2, 2);
    layer[0, 0] = 7;
    layer[1, 1] = 42;
    layer[0, 0].Should().Be(7);
    layer[1, 1].Should().Be(42);
    layer[0, 1].Should().Be(0);
  }

  [Theory]
  [InlineData(0, 0, true)]
  [InlineData(3, 2, true)]
  [InlineData(-1, 0, false)]
  [InlineData(0, -1, false)]
  [InlineData(4, 0, false)]
  [InlineData(0, 3, false)]
  public void InBounds_MatchesRectangleSemantics(int c, int r, bool expected)
  {
    TileLayer<int> layer = new("ground", 4, 3);
    layer.InBounds(c, r).Should().Be(expected);
  }

  [Fact]
  public void Fill_SetsEveryCell()
  {
    TileLayer<int> layer = new("ground", 3, 2);
    layer.Fill(9);
    for (int c = 0; c < 3; c++)
      for (int r = 0; r < 2; r++)
        layer[c, r].Should().Be(9);
  }
}
