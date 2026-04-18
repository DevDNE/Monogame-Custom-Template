using FluentAssertions;
using Microsoft.Xna.Framework;
using MonoGame.GameFramework.Rendering;
using Xunit;

namespace MonoGame.GameFramework.Tests.Rendering;

public class Camera2DTests
{
  private static Camera2D MakeCamera(Vector2? position = null, float zoom = 1f)
  {
    Camera2D cam = new(new Vector2(800, 600))
    {
      Position = position ?? Vector2.Zero,
      Zoom = zoom,
    };
    return cam;
  }

  [Fact]
  public void WorldToScreen_AtCameraPosition_IsViewportCenter()
  {
    Camera2D cam = MakeCamera(position: new Vector2(50, 75));
    Vector2 screen = cam.WorldToScreen(cam.Position);
    screen.X.Should().BeApproximately(400f, 1e-3f);
    screen.Y.Should().BeApproximately(300f, 1e-3f);
  }

  [Fact]
  public void ScreenToWorld_ViewportCenter_IsCameraPosition()
  {
    Camera2D cam = MakeCamera(position: new Vector2(250, -125));
    Vector2 world = cam.ScreenToWorld(new Vector2(400, 300));
    world.X.Should().BeApproximately(250f, 1e-3f);
    world.Y.Should().BeApproximately(-125f, 1e-3f);
  }

  [Theory]
  [InlineData(1f)]
  [InlineData(2f)]
  [InlineData(0.5f)]
  public void ScreenToWorld_RoundtripsThroughWorldToScreen(float zoom)
  {
    Camera2D cam = MakeCamera(position: new Vector2(17, 29), zoom: zoom);
    Vector2 original = new(123, 456);
    Vector2 screen = cam.WorldToScreen(original);
    Vector2 back = cam.ScreenToWorld(screen);
    back.X.Should().BeApproximately(original.X, 1e-3f);
    back.Y.Should().BeApproximately(original.Y, 1e-3f);
  }

  [Fact]
  public void ChangingPosition_UpdatesViewMatrix()
  {
    Camera2D cam = MakeCamera();
    Matrix before = cam.GetViewMatrix();
    cam.Position = new Vector2(100, 0);
    Matrix after = cam.GetViewMatrix();
    after.Should().NotBe(before);
  }

  [Fact]
  public void GetViewMatrix_StableWhenNothingChanges()
  {
    Camera2D cam = MakeCamera(position: new Vector2(10, 20), zoom: 1.5f);
    Matrix a = cam.GetViewMatrix();
    Matrix b = cam.GetViewMatrix();
    b.Should().Be(a);
  }

  [Fact]
  public void Zoom_ScalesWorldToScreenDeltaFromCenter()
  {
    Camera2D cam = MakeCamera(position: Vector2.Zero, zoom: 2f);
    // A point 10 world-units right of camera should land 20 screen-pixels right of center.
    Vector2 screen = cam.WorldToScreen(new Vector2(10, 0));
    (screen.X - 400f).Should().BeApproximately(20f, 1e-3f);
  }
}
