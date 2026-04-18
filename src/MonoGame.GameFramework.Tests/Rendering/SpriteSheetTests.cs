using FluentAssertions;
using Microsoft.Xna.Framework;
using MonoGame.GameFramework.Rendering;
using Xunit;

namespace MonoGame.GameFramework.Tests.Rendering;

public class SpriteSheetTests
{
  [Fact]
  public void Tint_DefaultIsWhite()
  {
    SpriteSheet s = new();
    s.Tint.Should().Be(Color.White);
  }

  [Fact]
  public void Tint_SetterRoundTrips()
  {
    SpriteSheet s = new() { Tint = Color.Red };
    s.Tint.Should().Be(Color.Red);
    s.Tint = Color.Blue;
    s.Tint.Should().Be(Color.Blue);
  }
}
