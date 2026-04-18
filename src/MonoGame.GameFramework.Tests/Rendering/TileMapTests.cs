using FluentAssertions;
using Microsoft.Xna.Framework;
using MonoGame.GameFramework.Rendering;
using Xunit;

namespace MonoGame.GameFramework.Tests.Rendering;

public class TileMapTests
{
  [Fact]
  public void GetWorldPosition_WithZeroOrigin()
  {
    TileMap map = new(10, 10, 32, 32);
    map.GetWorldPosition(2, 3).Should().Be(new Vector2(64, 96));
  }

  [Fact]
  public void GetWorldPosition_RespectsOrigin()
  {
    TileMap map = new(10, 10, 32, 32) { Origin = new Vector2(100, 50) };
    map.GetWorldPosition(1, 1).Should().Be(new Vector2(132, 82));
  }

  [Fact]
  public void GetCellRect_ReturnsTileSizedRectangle()
  {
    TileMap map = new(10, 10, 16, 24);
    map.GetCellRect(3, 2).Should().Be(new Rectangle(48, 48, 16, 24));
  }

  [Fact]
  public void WorldToCell_RoundtripsWithGetWorldPosition()
  {
    TileMap map = new(10, 10, 32, 32);
    for (int c = 0; c < 5; c++)
    {
      for (int r = 0; r < 5; r++)
      {
        Vector2 pos = map.GetWorldPosition(c, r);
        (int col, int row) = map.WorldToCell(pos);
        col.Should().Be(c);
        row.Should().Be(r);
      }
    }
  }

  [Fact]
  public void WorldToCell_TruncatesFractionalWithinCell()
  {
    TileMap map = new(10, 10, 32, 32);
    (int col, int row) = map.WorldToCell(new Vector2(31, 33));
    col.Should().Be(0);
    row.Should().Be(1);
  }

  [Fact]
  public void AddLayer_ReturnsUsableLayerOfMapDimensions()
  {
    TileMap map = new(5, 4, 10, 10);
    TileLayer<int> layer = map.AddLayer<int>("ground");
    layer.Columns.Should().Be(5);
    layer.Rows.Should().Be(4);
    layer.Name.Should().Be("ground");
  }

  [Fact]
  public void GetLayer_ReturnsSameInstance()
  {
    TileMap map = new(5, 4, 10, 10);
    TileLayer<int> a = map.AddLayer<int>("ground");
    TileLayer<int> b = map.GetLayer<int>("ground");
    b.Should().BeSameAs(a);
  }

  [Fact]
  public void GetLayer_Missing_ReturnsNull()
  {
    TileMap map = new(5, 4, 10, 10);
    map.GetLayer<int>("nope").Should().BeNull();
  }

  [Fact]
  public void RemoveLayer_ReturnsTrueIfPresent()
  {
    TileMap map = new(5, 4, 10, 10);
    map.AddLayer<int>("ground");
    map.RemoveLayer("ground").Should().BeTrue();
    map.RemoveLayer("ground").Should().BeFalse();
  }
}
