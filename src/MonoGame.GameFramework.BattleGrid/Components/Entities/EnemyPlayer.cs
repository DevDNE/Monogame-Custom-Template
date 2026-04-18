using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGame.GameFramework.Core;
using MonoGame.GameFramework.Events;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.Text;

namespace MonoGame.GameFramework.BattleGrid.Components.Entities;

public class EnemyPlayer : Entity
{
  private SpriteSheet character;
  private readonly DrawManager _drawManager;
  private readonly TextManager _textManager;
  private readonly EventManager _eventManager;

  private int hp = 100;
  private TextHandle hpText;
  private Vector2 initialPosition = new Vector2(485, 250);
  private Rectangle hitbox;

  public EnemyPlayer(ServiceProvider serviceProvider)
  {
    _drawManager = serviceProvider.GetService<DrawManager>();
    _textManager = serviceProvider.GetService<TextManager>();
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
    hpText = _textManager.AddText("enemy", hp.ToString(), new Vector2(500, 150), Color.Red);

    _drawManager.AddSprite(character);
  }

  public override void UnloadContent()
  {
    if (character != null)
    {
      _drawManager.RemoveSprite(character);
      character = null;
    }
    _eventManager.Unsubscribe("EnemyHit", OnProjectileHit);
  }

  public override void Update(GameTime gameTime) { }

  public Rectangle GetHitbox() => hitbox;

  public void OnProjectileHit(object sender, GameEventArgs e)
  {
    hp -= 10;
    _textManager.SetText(hpText, hp > 0 ? hp.ToString() : "Defeated");
  }
}
