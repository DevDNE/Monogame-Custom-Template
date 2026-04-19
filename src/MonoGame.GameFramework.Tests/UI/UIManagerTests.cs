using FluentAssertions;
using Microsoft.Xna.Framework;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.UI;
using Xunit;

namespace MonoGame.GameFramework.Tests.UI;

public class UIManagerTests
{
  private static SpriteSheet MakeSprite(Rectangle bounds) => new() { DestinationFrame = bounds };

  [Fact]
  public void GetElementAt_InsideBounds_ReturnsAddedSprite()
  {
    UIManager ui = new(mouseManager: null);
    SpriteSheet s = MakeSprite(new Rectangle(100, 100, 50, 50));
    ui.AddUIElement("menu", s);
    ui.GetElementAt(new Vector2(120, 120)).Should().BeSameAs(s);
  }

  [Fact]
  public void GetElementAt_OutsideBounds_ReturnsNull()
  {
    UIManager ui = new(mouseManager: null);
    SpriteSheet s = MakeSprite(new Rectangle(100, 100, 50, 50));
    ui.AddUIElement("menu", s);
    ui.GetElementAt(new Vector2(500, 500)).Should().BeNull();
  }

  [Fact]
  public void RemoveUIElement_RemovesFromHitTesting()
  {
    UIManager ui = new(mouseManager: null);
    SpriteSheet s = MakeSprite(new Rectangle(0, 0, 100, 100));
    ui.AddUIElement("menu", s);
    ui.RemoveUIElement("menu", s);
    ui.GetElementAt(new Vector2(50, 50)).Should().BeNull();
  }

  [Fact]
  public void RemoveUIElement_ClearsFocusIfElementWasFocused()
  {
    UIManager ui = new(mouseManager: null);
    SpriteSheet s = MakeSprite(new Rectangle(0, 0, 100, 100));
    ui.AddUIElement("menu", s);
    ui.SetFocus(s);
    ui.RemoveUIElement("menu", s);
    ui.FocusedElement.Should().BeNull();
  }

  [Fact]
  public void AddUIElement_DoesNotRequireDrawManager()
  {
    // UIManager constructor only takes MouseManager — no DrawManager coupling.
    UIManager ui = new(mouseManager: null);
    SpriteSheet s = MakeSprite(new Rectangle(0, 0, 10, 10));
    ui.Invoking(x => x.AddUIElement("g", s)).Should().NotThrow();
  }

  [Fact]
  public void ElementCount_SumsAcrossGroups()
  {
    UIManager ui = new(mouseManager: null);
    ui.ElementCount.Should().Be(0);
    ui.AddUIElement("a", MakeSprite(new Rectangle(0, 0, 10, 10)));
    ui.AddUIElement("a", MakeSprite(new Rectangle(0, 0, 10, 10)));
    ui.AddUIElement("b", MakeSprite(new Rectangle(0, 0, 10, 10)));
    ui.ElementCount.Should().Be(3);
  }
}
