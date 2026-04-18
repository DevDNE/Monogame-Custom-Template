using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameFramework.Platformer.Entities;

public class Enemy
{
  public const int Width = 28;
  public const int Height = 32;
  public const float Speed = 80f;

  public Vector2 Position { get; set; }
  public Rectangle Bounds => new((int)Position.X, (int)Position.Y, Width, Height);

  private readonly float _minX;
  private readonly float _maxX;
  private int _direction = 1;

  public Enemy(float x, float y, float patrolMinX, float patrolMaxX)
  {
    _minX = patrolMinX;
    _maxX = patrolMaxX - Width;
    Position = new Vector2(MathHelper.Clamp(x, _minX, _maxX), y);
  }

  public void Update(GameTime gameTime)
  {
    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
    float newX = Position.X + _direction * Speed * dt;
    if (newX <= _minX) { newX = _minX; _direction = 1; }
    else if (newX >= _maxX) { newX = _maxX; _direction = -1; }
    Position = new Vector2(newX, Position.Y);
  }

  public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
  {
    spriteBatch.Draw(pixel, Bounds, new Color(200, 60, 60));
  }
}
