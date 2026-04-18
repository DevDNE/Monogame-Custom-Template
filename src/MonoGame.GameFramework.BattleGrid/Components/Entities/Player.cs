using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using MonoGame.GameFramework.Core;
using MonoGame.GameFramework.Events;
using MonoGame.GameFramework.Input;
using MonoGame.GameFramework.Rendering;

namespace MonoGame.GameFramework.BattleGrid.Components.Entities;

public class Player : Entity
{
  private SpriteSheet character;
  private readonly DrawManager _drawManager;
  private readonly KeyboardManager _keyboardManager;
  private readonly EventManager _eventManager;

  private Vector2 initialPosition = new Vector2(200, 250);
  private List<Projectile> projectiles = new List<Projectile>();
  private Rectangle hitbox;

  public Player(ServiceProvider serviceProvider)
  {
    _drawManager = serviceProvider.GetService<DrawManager>();
    _keyboardManager = serviceProvider.GetService<KeyboardManager>();
    _eventManager = serviceProvider.GetService<EventManager>();
    hitbox = new Rectangle((int)initialPosition.X, (int)initialPosition.Y, BattleConfig.HitboxWidth, BattleConfig.HitboxHeight);
  }

  public override void LoadContent(ContentManager content)
  {
    character = SpriteSheet.Static(
      Primitives.Pixel,
      new Rectangle((int)initialPosition.X, (int)initialPosition.Y, BattleConfig.DisplayWidth, BattleConfig.DisplayHeight),
      name: "Player");
    character.Tint = new Color(80, 180, 255);
    _drawManager.AddSprite(character);
  }

  public override void UnloadContent()
  {
    if (character != null)
    {
      _drawManager.RemoveSprite(character);
      character = null;
    }
  }

  public override void Update(GameTime gameTime)
  {
    CheckMovingUp();
    CheckMovingLeft();
    CheckMovingDown();
    CheckMovingRight();
    CheckFiredProjectile(gameTime);
  }

  private void CheckMovingUp()
  {
    if (_keyboardManager.WasKeyReleased(Keys.W) && character.Position.Y > 200)
    {
      character.Position = new Vector2(character.Position.X, character.Position.Y - BattleConfig.TileSize);
      RefreshDestination();
      _eventManager.TriggerEvent("PlayerMoved", this, new GameEventArgs("Player moved up"));
    }
  }

  private void CheckMovingLeft()
  {
    if (_keyboardManager.WasKeyReleased(Keys.A) && character.Position.X > 150)
    {
      character.Position = new Vector2(character.Position.X - BattleConfig.TileSize, character.Position.Y);
      RefreshDestination();
      _eventManager.TriggerEvent("PlayerMoved", this, new GameEventArgs("Player moved left"));
    }
  }

  private void CheckMovingDown()
  {
    if (_keyboardManager.WasKeyReleased(Keys.S) && character.Position.Y < 300)
    {
      character.Position = new Vector2(character.Position.X, character.Position.Y + BattleConfig.TileSize);
      RefreshDestination();
      _eventManager.TriggerEvent("PlayerMoved", this, new GameEventArgs("Player moved down"));
    }
  }

  private void CheckMovingRight()
  {
    if (_keyboardManager.WasKeyReleased(Keys.D) && character.Position.X < 250)
    {
      character.Position = new Vector2(character.Position.X + BattleConfig.TileSize, character.Position.Y);
      RefreshDestination();
      _eventManager.TriggerEvent("PlayerMoved", this, new GameEventArgs("Player moved right"));
    }
  }

  private void RefreshDestination()
  {
    character.DestinationFrame = new Rectangle(
      (int)character.Position.X, (int)character.Position.Y,
      BattleConfig.DisplayWidth, BattleConfig.DisplayHeight);
    hitbox = new Rectangle(
      (int)character.Position.X, (int)character.Position.Y,
      BattleConfig.HitboxWidth, BattleConfig.HitboxHeight);
  }

  private void CheckFiredProjectile(GameTime gameTime)
  {
    if (_keyboardManager.WasKeyReleased(Keys.Space))
    {
      FireProjectile(new Projectile(_drawManager, character.Position, new Vector2(10, 0)));
    }
    RemoveProjectileOffscreen(gameTime);
  }

  private void RemoveProjectileOffscreen(GameTime gameTime)
  {
    for (int i = projectiles.Count - 1; i >= 0; i--)
    {
      projectiles[i].Update(gameTime);
      if (projectiles[i].GetHurtbox().X > BattleConfig.ProjectileOffscreenMaxX ||
          projectiles[i].GetHurtbox().X < 0)
      {
        _drawManager.RemoveSprite(projectiles[i].GetSprite());
        projectiles.RemoveAt(i);
      }
    }
  }

  public void RemoveProjectileOnCollision(Projectile projectile)
  {
    _drawManager.RemoveSprite(projectile.GetSprite());
    projectiles.Remove(projectile);
  }

  public void FireProjectile(Projectile projectile)
  {
    _drawManager.AddSprite(projectile.GetSprite());
    projectiles.Add(projectile);
    _eventManager.TriggerEvent("PlayerFiredProjectile", this, new GameEventArgs("Player fired a projectile"));
  }

  public List<Projectile> GetProjectiles() => projectiles;
  public Rectangle GetHitbox() => hitbox;
}
