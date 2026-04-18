using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameFramework.Core;
using MonoGame.GameFramework.Events;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.Text;

namespace MonoGame.GameFramework.Demo.Components.Entities;

public class EnemyPlayer : Entity
{
  public Tile CurrentTile { get; set; }
  private SpriteSheet character;
  private DrawManager _drawManager;
  private TextManager _textManager;
  private EventManager _eventManager;

  private int hp = 100;
  private TextHandle hpText;
  private Vector2 initialPosition = new Vector2(485, 250);
  private Rectangle hitbox;
  private int hitboxWidth = 38;
  private int hitboxHeight = 22;
  private int originalWidth = 35;
  private int originalHeight = 49;
  private int displayedWidth = 70;
  private int displayedHeight = 98;
  private float frameInterval = 0.1f;
  private int currentFrame = 0;
  public EnemyPlayer(ServiceProvider serviceProvider)
  {
    _drawManager = serviceProvider.GetService<DrawManager>();
    _textManager = serviceProvider.GetService<TextManager>();
    _eventManager = serviceProvider.GetService<EventManager>();
    _eventManager.Subscribe("EnemyHit", OnProjectileHit);
    hitbox = new Rectangle((int)initialPosition.X, (int)initialPosition.Y, hitboxWidth, hitboxHeight);
  }

  public override void LoadContent(ContentManager content)
  {
    character = SpriteSheet.Animated(
      content.Load<Texture2D>("gfx/aquaStyle"),
      new Rectangle[] {
        new(0, 0, originalWidth, originalHeight),
        new(48, 0, originalWidth, originalHeight),
        new(96, 0, originalWidth, originalHeight),
        new(144, 0, originalWidth, originalHeight)
      },
      new Rectangle((int)initialPosition.X, (int)initialPosition.Y, displayedWidth, displayedHeight),
      frameInterval, name: "PlayerCharacter", startFrame: currentFrame);
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

  public override void Update(GameTime gameTime){}

  public Rectangle GetHitbox()
  {
    return hitbox;
  }

  public void OnProjectileHit(object sender, GameEventArgs e)
  {
    hp -= 10;
    _textManager.SetText(hpText, hp > 0 ? hp.ToString() : "Defeated");
  }
}
