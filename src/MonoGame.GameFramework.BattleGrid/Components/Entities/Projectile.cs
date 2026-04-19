using Microsoft.Xna.Framework;
using MonoGame.GameFramework.Rendering;

namespace MonoGame.GameFramework.BattleGrid.Components.Entities;

public class Projectile
{
  private const int ProjectileWidth = 24;
  private const int ProjectileHeight = 10;

  private SpriteSheet sprite;
  private Rectangle hurtbox;
  private Vector2 velocity;
  private readonly DrawManager drawManager;
  private readonly int _damage;

  public Projectile(DrawManager drawManager, Vector2 position, Vector2 velocity, Color tint, int damage = BattleConfig.ProjectileDamage)
  {
    this.drawManager = drawManager;
    this.velocity = velocity;
    _damage = damage;
    sprite = SpriteSheet.Static(
      Primitives.Pixel,
      new Rectangle((int)position.X, (int)position.Y, ProjectileWidth, ProjectileHeight),
      name: "Projectile");
    sprite.Tint = tint;
    hurtbox = new Rectangle((int)position.X, (int)position.Y, ProjectileWidth, ProjectileHeight);
  }

  public void UnloadContent()
  {
    if (sprite != null)
    {
      drawManager.RemoveSprite(sprite);
      sprite = null;
    }
  }

  public void Update(GameTime gameTime)
  {
    sprite.Position += velocity;
    sprite.DestinationFrame = new Rectangle((int)sprite.Position.X, (int)sprite.Position.Y, ProjectileWidth, ProjectileHeight);
    hurtbox = new Rectangle((int)sprite.Position.X, (int)sprite.Position.Y, ProjectileWidth, ProjectileHeight);
  }

  public SpriteSheet GetSprite() => sprite;
  public Rectangle GetHurtbox() => hurtbox;
  public int GetDamageNumber() => _damage;
}
