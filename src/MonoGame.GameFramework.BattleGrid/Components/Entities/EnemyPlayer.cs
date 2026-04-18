using System;
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
  public const float ActionInterval = 1.5f;

  private enum Action { Move, Shoot }
  private enum Pattern { Single, Wide }

  public int Hp { get; private set; } = MaxHp;
  public bool IsAlive => Hp > 0;
  public int GridCol { get; private set; } = 1;
  public int GridRow { get; private set; } = 1;

  private SpriteSheet character;
  private readonly DrawManager _drawManager;
  private readonly EventManager _eventManager;
  private readonly Random _random = new();

  private Rectangle hitbox;
  private readonly List<Projectile> _projectiles = new();

  private Action _nextAction = Action.Shoot;
  private Pattern _nextPattern = Pattern.Single;
  private float _actionTimer = ActionInterval;

  public EnemyPlayer(ServiceProvider serviceProvider)
  {
    _drawManager = serviceProvider.GetService<DrawManager>();
    _eventManager = serviceProvider.GetService<EventManager>();
  }

  public override void LoadContent(ContentManager content)
  {
    Vector2 pos = Grid.EnemyCellTopLeft(GridCol, GridRow);
    character = SpriteSheet.Static(
      Primitives.Pixel,
      new Rectangle((int)pos.X, (int)pos.Y, BattleConfig.DisplayWidth, BattleConfig.DisplayHeight),
      name: "Enemy");
    character.Tint = new Color(240, 100, 100);
    hitbox = new Rectangle((int)pos.X, (int)pos.Y, BattleConfig.HitboxWidth, BattleConfig.HitboxHeight);
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
  }

  public override void Update(GameTime gameTime)
  {
    if (!IsAlive) return;

    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
    _actionTimer -= dt;
    if (_actionTimer <= 0f)
    {
      PerformNextAction();
      _actionTimer = ActionInterval;
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

  public void Damage(int amount)
  {
    Hp -= amount;
    if (Hp < 0) Hp = 0;
    _eventManager.TriggerEvent("EnemyHit", this, new GameEventArgs($"Enemy took {amount} damage"));
  }

  private void PerformNextAction()
  {
    if (_nextAction == Action.Move)
    {
      MoveToRandomCell();
      _nextAction = Action.Shoot;
    }
    else
    {
      FirePattern(_nextPattern);
      _nextPattern = _nextPattern == Pattern.Single ? Pattern.Wide : Pattern.Single;
      _nextAction = Action.Move;
    }
  }

  private void MoveToRandomCell()
  {
    int newCol, newRow;
    do
    {
      newCol = _random.Next(3);
      newRow = _random.Next(3);
    } while (newCol == GridCol && newRow == GridRow);

    GridCol = newCol;
    GridRow = newRow;
    Vector2 pos = Grid.EnemyCellTopLeft(GridCol, GridRow);
    character.Position = pos;
    character.DestinationFrame = new Rectangle((int)pos.X, (int)pos.Y, BattleConfig.DisplayWidth, BattleConfig.DisplayHeight);
    hitbox = new Rectangle((int)pos.X, (int)pos.Y, BattleConfig.HitboxWidth, BattleConfig.HitboxHeight);
  }

  private void FirePattern(Pattern p)
  {
    if (p == Pattern.Single) FireSingleShot();
    else FireWideShot();
  }

  private void FireSingleShot()
  {
    float y = character.Position.Y + BattleConfig.DisplayHeight * 0.5f - 5f;
    SpawnProjectile(new Vector2(character.Position.X - 14, y));
    _eventManager.TriggerEvent("EnemyFiredProjectile", this, new GameEventArgs("Enemy fired single shot"));
  }

  private void FireWideShot()
  {
    for (int row = 0; row < 3; row++)
    {
      float y = Grid.RowCenterY(row) - 5f;
      float x = BattleConfig.EnemyBoardX - 14f;
      SpawnProjectile(new Vector2(x, y));
    }
    _eventManager.TriggerEvent("EnemyFiredProjectile", this, new GameEventArgs("Enemy fired wide shot"));
  }

  private void SpawnProjectile(Vector2 pos)
  {
    Projectile p = new(_drawManager, pos, new Vector2(-8, 0), new Color(255, 140, 60));
    _drawManager.AddSprite(p.GetSprite());
    _projectiles.Add(p);
  }

  public Rectangle GetHitbox() => hitbox;
  public List<Projectile> GetProjectiles() => _projectiles;

  public void RemoveProjectileOnCollision(Projectile projectile)
  {
    projectile.UnloadContent();
    _projectiles.Remove(projectile);
  }
}
