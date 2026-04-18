using System;
using Microsoft.Xna.Framework;

namespace MonoGame.GameFramework.Rendering;

public class Camera2D
{
  private Vector2 _position = Vector2.Zero;
  private float _zoom = 1f;
  private float _rotation = 0f;
  private Vector2 _viewportSize;
  private Vector2 _shakeOffset;

  public Vector2 Position
  {
    get => _position;
    set { if (_position != value) { _position = value; _viewDirty = true; } }
  }

  public float Zoom
  {
    get => _zoom;
    set { if (_zoom != value) { _zoom = value; _viewDirty = true; } }
  }

  public float Rotation
  {
    get => _rotation;
    set { if (_rotation != value) { _rotation = value; _viewDirty = true; } }
  }

  public Vector2 ViewportSize
  {
    get => _viewportSize;
    set { if (_viewportSize != value) { _viewportSize = value; _viewDirty = true; } }
  }

  public float FollowLerp { get; set; } = 0.1f;
  public Vector2? Target { get; set; }

  private float _shakeTimeRemaining;
  private float _shakeIntensity;
  private readonly Random _random = new();

  private Matrix _viewMatrix;
  private bool _viewDirty = true;

  public Camera2D(Vector2 viewportSize)
  {
    ViewportSize = viewportSize;
  }

  public void Shake(float duration, float intensity)
  {
    _shakeTimeRemaining = duration;
    _shakeIntensity = intensity;
  }

  public void Update(GameTime gameTime)
  {
    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

    if (Target.HasValue)
    {
      Position = Vector2.Lerp(Position, Target.Value, FollowLerp);
    }

    if (_shakeTimeRemaining > 0f)
    {
      _shakeTimeRemaining -= dt;
      float x = ((float)_random.NextDouble() * 2f - 1f) * _shakeIntensity;
      float y = ((float)_random.NextDouble() * 2f - 1f) * _shakeIntensity;
      Vector2 newShake = _shakeTimeRemaining <= 0f ? Vector2.Zero : new Vector2(x, y);
      if (newShake != _shakeOffset) { _shakeOffset = newShake; _viewDirty = true; }
    }
    else if (_shakeOffset != Vector2.Zero)
    {
      _shakeOffset = Vector2.Zero;
      _viewDirty = true;
    }
  }

  public Matrix GetViewMatrix()
  {
    if (_viewDirty)
    {
      Vector2 center = _viewportSize * 0.5f;
      _viewMatrix = Matrix.CreateTranslation(new Vector3(-(_position + _shakeOffset), 0f))
                    * Matrix.CreateRotationZ(_rotation)
                    * Matrix.CreateScale(_zoom, _zoom, 1f)
                    * Matrix.CreateTranslation(new Vector3(center, 0f));
      _viewDirty = false;
    }
    return _viewMatrix;
  }

  public Vector2 ScreenToWorld(Vector2 screen)
  {
    Matrix invert = Matrix.Invert(GetViewMatrix());
    return Vector2.Transform(screen, invert);
  }

  public Vector2 WorldToScreen(Vector2 world)
  {
    return Vector2.Transform(world, GetViewMatrix());
  }
}
