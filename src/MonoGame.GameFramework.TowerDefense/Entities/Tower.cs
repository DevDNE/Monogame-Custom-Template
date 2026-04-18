using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Rendering;

namespace MonoGame.GameFramework.TowerDefense.Entities;

public class Tower
{
  public const int Cost = 20;
  public const int Size = 30;
  public const float FireCooldown = 0.8f;
  public const float Range = 140f;

  public int Col;
  public int Row;
  public Vector2 Position;

  private float _cooldown;

  public Tower(int col, int row, Vector2 position)
  {
    Col = col;
    Row = row;
    Position = position;
  }

  public Rectangle Bounds => new((int)(Position.X - Size * 0.5f), (int)(Position.Y - Size * 0.5f), Size, Size);

  /// <summary>Advance cooldown and, if ready, return the enemy to target (or null).</summary>
  public Enemy TryFire(float dt, IEnumerable<Enemy> enemies)
  {
    if (_cooldown > 0f) _cooldown -= dt;
    if (_cooldown > 0f) return null;

    Enemy nearest = null;
    float nearestDistSq = Range * Range;
    foreach (Enemy e in enemies)
    {
      if (!e.Alive) continue;
      float dSq = Vector2.DistanceSquared(e.Position, Position);
      if (dSq < nearestDistSq) { nearestDistSq = dSq; nearest = e; }
    }
    if (nearest == null) return null;
    _cooldown = FireCooldown;
    return nearest;
  }

  public void Draw(SpriteBatch spriteBatch)
  {
    Primitives.DrawRectangle(spriteBatch, Bounds, new Color(80, 150, 240));
    // Small gun-barrel marker
    Rectangle barrel = new(Bounds.Center.X - 2, Bounds.Y - 4, 4, 8);
    Primitives.DrawRectangle(spriteBatch, barrel, new Color(200, 220, 240));
  }
}
