using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameFramework.Platformer.Entities;

public class Player
{
  public const float MoveSpeed = 280f;
  public const float GroundAcceleration = 1800f;
  public const float AirAcceleration = 1200f;
  public const float JumpVelocity = -700f;
  public const float JumpCutVelocity = -200f;
  public const float Gravity = 1800f;
  public const float MaxFallSpeed = 900f;
  public const float CoyoteTime = 0.1f;
  public const float JumpBufferTime = 0.1f;
  public const int Width = 32;
  public const int Height = 48;

  public Vector2 Position { get; set; }
  public Vector2 Velocity { get; set; }
  public bool IsGrounded { get; private set; }

  public Rectangle Bounds => new((int)Position.X, (int)Position.Y, Width, Height);

  private readonly Vector2 _spawnPosition;
  private float _coyoteTimer;
  private float _jumpBufferTimer;

  public Player(Vector2 spawnPosition)
  {
    _spawnPosition = spawnPosition;
    Respawn();
  }

  public void Respawn()
  {
    Position = _spawnPosition;
    Velocity = Vector2.Zero;
    IsGrounded = false;
    _coyoteTimer = 0f;
    _jumpBufferTimer = 0f;
  }

  public void Update(GameTime gameTime, IReadOnlyList<Platform> platforms, float inputX, bool jumpPressed, bool jumpHeld)
  {
    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

    if (IsGrounded) _coyoteTimer = CoyoteTime;
    else _coyoteTimer = MathF.Max(0f, _coyoteTimer - dt);

    if (jumpPressed) _jumpBufferTimer = JumpBufferTime;
    else _jumpBufferTimer = MathF.Max(0f, _jumpBufferTimer - dt);

    float targetVx = inputX * MoveSpeed;
    float accel = IsGrounded ? GroundAcceleration : AirAcceleration;
    float newVx = ApproachTarget(Velocity.X, targetVx, accel * dt);

    float newVy = Velocity.Y + Gravity * dt;

    bool canJump = _jumpBufferTimer > 0f && _coyoteTimer > 0f;
    if (canJump)
    {
      newVy = JumpVelocity;
      _jumpBufferTimer = 0f;
      _coyoteTimer = 0f;
    }

    if (!jumpHeld && newVy < JumpCutVelocity)
    {
      newVy = JumpCutVelocity;
    }

    newVy = MathHelper.Clamp(newVy, -MaxFallSpeed, MaxFallSpeed);

    Velocity = new Vector2(newVx, newVy);

    MoveX(Velocity.X * dt, platforms);
    MoveY(Velocity.Y * dt, platforms);
  }

  private void MoveX(float delta, IReadOnlyList<Platform> platforms)
  {
    Position = new Vector2(Position.X + delta, Position.Y);
    foreach (Platform p in platforms)
    {
      if (!Bounds.Intersects(p.Bounds)) continue;
      if (delta > 0)
        Position = new Vector2(p.Bounds.Left - Width, Position.Y);
      else if (delta < 0)
        Position = new Vector2(p.Bounds.Right, Position.Y);
      Velocity = new Vector2(0, Velocity.Y);
    }
  }

  private void MoveY(float delta, IReadOnlyList<Platform> platforms)
  {
    Position = new Vector2(Position.X, Position.Y + delta);
    IsGrounded = false;
    foreach (Platform p in platforms)
    {
      if (!Bounds.Intersects(p.Bounds)) continue;
      if (delta > 0)
      {
        Position = new Vector2(Position.X, p.Bounds.Top - Height);
        IsGrounded = true;
      }
      else if (delta < 0)
      {
        Position = new Vector2(Position.X, p.Bounds.Bottom);
      }
      Velocity = new Vector2(Velocity.X, 0);
    }
  }

  private static float ApproachTarget(float current, float target, float step)
  {
    float diff = target - current;
    if (MathF.Abs(diff) <= step) return target;
    return current + MathF.Sign(diff) * step;
  }

  public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
  {
    spriteBatch.Draw(pixel, Bounds, new Color(220, 200, 100));
  }
}
