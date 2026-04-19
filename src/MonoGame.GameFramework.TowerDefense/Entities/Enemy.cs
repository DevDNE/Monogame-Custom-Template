using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.UI;

namespace MonoGame.GameFramework.TowerDefense.Entities;

public class Enemy
{
  public const int MaxHp = 20;
  public const float Speed = 80f;
  public const int Size = 28;

  public Vector2 Position;
  public int Hp;
  public int WaypointIndex; // next waypoint we're moving toward
  public bool Alive;
  public bool Leaked;

  public Rectangle Bounds => new((int)(Position.X - Size * 0.5f), (int)(Position.Y - Size * 0.5f), Size, Size);

  public void Spawn(Vector2 spawnPos)
  {
    Position = spawnPos;
    Hp = MaxHp;
    WaypointIndex = 1; // heading toward Waypoints[1]
    Alive = true;
    Leaked = false;
  }

  public void Update(float dt, MapPath.TileMapOrigin origin)
  {
    if (!Alive) return;
    if (WaypointIndex >= MapPath.Waypoints.Length) { Alive = false; Leaked = true; return; }
    (int c, int r) = MapPath.Waypoints[WaypointIndex];
    Vector2 target = MapPath.WorldPosition(origin, c, r);
    Vector2 toTarget = target - Position;
    float dist = toTarget.Length();
    float step = Speed * dt;
    if (dist <= step)
    {
      Position = target;
      WaypointIndex++;
    }
    else
    {
      toTarget /= dist;
      Position += toTarget * step;
    }
  }

  public void Damage(int amount)
  {
    Hp -= amount;
    if (Hp <= 0) { Hp = 0; Alive = false; }
  }

  public void Draw(SpriteBatch spriteBatch)
  {
    if (!Alive) return;
    Primitives.DrawRectangle(spriteBatch, Bounds, new Color(230, 80, 80));
    HpBar.Draw(spriteBatch, new Rectangle(Bounds.X, Bounds.Y - 6, Bounds.Width, 4), Hp, MaxHp, new Color(120, 220, 140));
  }
}
