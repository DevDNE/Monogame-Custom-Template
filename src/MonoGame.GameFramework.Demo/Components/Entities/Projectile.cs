using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Core;
using MonoGame.GameFramework.Rendering;

namespace MonoGame.GameFramework.Demo.Components.Entities;
public class Projectile : Entity
{
  private SpriteSheet sprite;
  private Rectangle hurtbox;
  private Vector2 velocity;
  private readonly DrawManager drawManager;
  public Projectile(DrawManager _drawManager, Vector2 _position, Vector2 _velocity, SpriteSheet _sprite)
  {
    velocity = _velocity;
    drawManager = _drawManager;
    sprite = _sprite;
    sprite.Position = _position;
    hurtbox = new Rectangle((int)sprite.Position.X, (int)sprite.Position.Y, BattleConfig.HitboxWidth, BattleConfig.HitboxHeight);
  }

  public override void LoadContent(ContentManager content)
  {
    drawManager.AddSprite(sprite);
  }

  public override void UnloadContent()
  {
    if (sprite != null)
    {
      drawManager.RemoveSprite(sprite);
      sprite = null;
    }
  }

  public override void Update(GameTime gameTime)
  {
    sprite.Position += velocity;
    sprite.DestinationFrame = new Rectangle((int)sprite.Position.X, (int)sprite.Position.Y, 30, 60);
    hurtbox = new Rectangle((int)sprite.Position.X, (int)sprite.Position.Y, BattleConfig.HitboxWidth, BattleConfig.HitboxHeight);
  }

  public SpriteSheet GetSprite() {
    return sprite;
  }

  public void SetDestinationFrame(Rectangle desitnationFrame) {
    sprite.DestinationFrame = desitnationFrame;
  }

  public Rectangle GetHurtbox() {
    return hurtbox;
  }

  public int GetDamageNumber()
  {
    return BattleConfig.ProjectileDamage;
  }
}
