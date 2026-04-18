using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGame.GameFramework.Core;
using MonoGame.GameFramework.Events;
using MonoGame.GameFramework.Rendering;

namespace MonoGame.GameFramework.BattleGrid.Components.Entities;

public class EnemyPlayer : Entity
{
  public const int MaxHp = 100;
  public const float ShootCooldown = 2.0f;

  public int Hp { get; private set; } = MaxHp;
  public bool IsAlive => Hp > 0;

  private SpriteSheet character;
  private readonly DrawManager _drawManager;
  private readonly EventManager _eventManager;

  private Vector2 initialPosition = new Vector2(485, 250);
  private Rectangle hitbox;

  private readonly List<Projectile> _projectiles = new();
  private float _shootTimer = ShootCooldown;

  public EnemyPlayer(ServiceProvider serviceProvider)
  {
    _drawManager = serviceProvider.GetService<DrawManager>();
    _eventManager = serviceProvider.GetService<EventManager>();
    _eventManager.Subscribe("EnemyHit", OnProjectileHit);
    hitbox = new Rectangle((int)initialPosition.X, (int)initialPosition.Y, BattleConfig.HitboxWidth, BattleConfig.HitboxHeight);
  }

  public override void LoadContent(ContentManager content)
  {
    character = SpriteSheet.Static(
      Primitives.Pixel,
      new Rectangle((int)initialPosition.X, (int)initialPosition.Y, BattleConfig.DisplayWidth, BattleConfig.DisplayHeight),
      name: "Enemy");
    character.Tint = new Color(240, 100, 100);
    _drawManager.AddSprite(character);
  }

  public override void UnloadContent()
  {
    if (character != null)
    {
      _drawManager.RemoveSprite(character);
      character = null;
    }
    foreach (Projectile p in _projectiles) p.UnloadContent();
    _projectiles.Clear();
    _eventManager.Unsubscribe("EnemyHit", OnProjectileHit);
  }

  public override void Update(GameTime gameTime)
  {
    if (!IsAlive) return;

    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
    _shootTimer -= dt;
    if (_shootTimer <= 0f)
    {
      FireProjectile();
      _shootTimer = ShootCooldown;
    }

    for (int i = _projectiles.Count - 1; i >= 0; i--)
    {
      _projectiles[i].Update(gameTime);
      int x = _projectiles[i].GetHurtbox().X;
      if (x < 0 || x > BattleConfig.ProjectileOffscreenMaxX)
      {
        _projectiles[i].UnloadContent();
        _projectiles.RemoveAt(i);
      }
    }
  }

  private void FireProjectile()
  {
    Vector2 spawn = new(character.Position.X - 10, character.Position.Y + BattleConfig.DisplayHeight / 2f - 5);
    Projectile p = new(_drawManager, spawn, new Vector2(-8, 0), new Color(255, 140, 60));
    _drawManager.AddSprite(p.GetSprite());
    _projectiles.Add(p);
    _eventManager.TriggerEvent("EnemyFiredProjectile", this, new GameEventArgs("Enemy fired a projectile"));
  }

  public Rectangle GetHitbox() => hitbox;

  public List<Projectile> GetProjectiles() => _projectiles;

  public void RemoveProjectileOnCollision(Projectile projectile)
  {
    projectile.UnloadContent();
    _projectiles.Remove(projectile);
  }

  public void OnProjectileHit(object sender, GameEventArgs e)
  {
    Hp -= BattleConfig.ProjectileDamage;
    if (Hp < 0) Hp = 0;
  }
}
