using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameFramework.Platformer.Entities;

public class Player
{
  public const float Gravity = 1800f;
  public const float MaxFallSpeed = 900f;
  public const int Width = 32;
  public const int Height = 48;

  public Vector2 Position { get; set; }
  public Vector2 Velocity { get; set; }
  public bool IsGrounded { get; private set; }

  public Rectangle Bounds => new((int)Position.X, (int)Position.Y, Width, Height);

  private readonly Vector2 _spawnPosition;

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
  }

  public void Update(GameTime gameTime, Platform ground)
  {
    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

    float vy = MathHelper.Clamp(Velocity.Y + Gravity * dt, -MaxFallSpeed, MaxFallSpeed);
    Velocity = new Vector2(Velocity.X, vy);
    Position += Velocity * dt;

    IsGrounded = false;
    if (Velocity.Y >= 0 && Bounds.Intersects(ground.Bounds))
    {
      Position = new Vector2(Position.X, ground.Bounds.Top - Height);
      Velocity = new Vector2(Velocity.X, 0);
      IsGrounded = true;
    }
  }

  public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
  {
    spriteBatch.Draw(pixel, Bounds, new Color(220, 200, 100));
  }
}
