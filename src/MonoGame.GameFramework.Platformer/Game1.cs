using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.GameFramework.Input;
using MonoGame.GameFramework.Platformer.Entities;
using MonoGame.GameFramework.Rendering;

namespace MonoGame.GameFramework.Platformer;

public class Game1 : Game
{
  private const int ViewportWidth = 1024;
  private const int ViewportHeight = 576;
  private const float DeathPlaneY = 800f;

  private readonly ServiceProvider _serviceProvider;
  private readonly GraphicsDeviceManager _graphics;
  private SpriteBatch _spriteBatch;
  private KeyboardManager _keyboardManager;
  private SpriteFont _font;
  private Texture2D _pixel;
  private Player _player;
  private List<Platform> _platforms;
  private List<Enemy> _enemies;
  private Goal _goal;
  private Camera2D _camera;
  private bool _won;

  public Game1(ServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
    _graphics = new GraphicsDeviceManager(this)
    {
      PreferredBackBufferWidth = ViewportWidth,
      PreferredBackBufferHeight = ViewportHeight,
    };
    _graphics.ApplyChanges();
    Content.RootDirectory = "Content";
    IsMouseVisible = true;
    Window.Title = "Platformer Sample";
  }

  protected override void Initialize()
  {
    _keyboardManager = _serviceProvider.GetService<KeyboardManager>();
    base.Initialize();
  }

  protected override void LoadContent()
  {
    _spriteBatch = new SpriteBatch(GraphicsDevice);
    _pixel = new Texture2D(GraphicsDevice, 1, 1);
    _pixel.SetData(new[] { Color.White });
    _font = Content.Load<SpriteFont>("fonts/Arial");

    _player = new Player(new Vector2(200, 60));
    _platforms = new List<Platform>
    {
      new(new Rectangle(0, 540, 1050, 36)),
      new(new Rectangle(1200, 540, 1200, 36)),
      new(new Rectangle(130, 420, 180, 24)),
      new(new Rectangle(430, 340, 180, 24)),
      new(new Rectangle(740, 420, 180, 24)),
      new(new Rectangle(430, 180, 180, 24)),
      new(new Rectangle(1050, 440, 180, 24)),
      new(new Rectangle(1400, 360, 180, 24)),
      new(new Rectangle(1700, 280, 180, 24)),
      new(new Rectangle(2000, 380, 260, 24)),
    };

    _enemies = new List<Enemy>
    {
      new(x: 1400, y: 360 - Enemy.Height, patrolMinX: 1400, patrolMaxX: 1400 + 180),
    };

    _goal = new Goal(new Vector2(2300, 540 - Goal.Height));

    _camera = new Camera2D(new Vector2(ViewportWidth, ViewportHeight))
    {
      Position = PlayerCenter(),
      Target = PlayerCenter(),
      FollowLerp = 0.12f,
    };
  }

  private Vector2 PlayerCenter() => _player.Position + new Vector2(Player.Width * 0.5f, Player.Height * 0.5f);

  private void Respawn()
  {
    _player.Respawn();
    _won = false;
    if (_camera != null) _camera.Position = PlayerCenter();
  }

  protected override void Update(GameTime gameTime)
  {
    _keyboardManager.Update();
    if (_keyboardManager.IsKeyDown(Keys.Escape)) Exit();
    if (_keyboardManager.WasKeyPressed(Keys.R)) Respawn();

    if (!_won)
    {
      float inputX = 0f;
      if (_keyboardManager.IsKeyDown(Keys.A) || _keyboardManager.IsKeyDown(Keys.Left)) inputX -= 1f;
      if (_keyboardManager.IsKeyDown(Keys.D) || _keyboardManager.IsKeyDown(Keys.Right)) inputX += 1f;
      bool jumpPressed = _keyboardManager.WasKeyPressed(Keys.Space);
      bool jumpHeld = _keyboardManager.IsKeyDown(Keys.Space);

      _player.Update(gameTime, _platforms, inputX, jumpPressed, jumpHeld);
      foreach (Enemy e in _enemies) e.Update(gameTime);

      if (_player.Position.Y > DeathPlaneY)
      {
        Respawn();
      }
      else if (_player.Bounds.Intersects(_goal.Bounds))
      {
        _won = true;
      }
      else
      {
        foreach (Enemy e in _enemies)
        {
          if (_player.Bounds.Intersects(e.Bounds))
          {
            Respawn();
            break;
          }
        }
      }
    }

    _camera.Target = PlayerCenter();
    _camera.Update(gameTime);

    base.Update(gameTime);
  }

  protected override void Draw(GameTime gameTime)
  {
    GraphicsDevice.Clear(new Color(20, 24, 40));

    _spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);
    foreach (Platform p in _platforms) p.Draw(_spriteBatch, _pixel);
    foreach (Enemy e in _enemies) e.Draw(_spriteBatch, _pixel);
    _goal.Draw(_spriteBatch, _pixel);
    _player.Draw(_spriteBatch, _pixel);
    _spriteBatch.End();

    if (_won)
    {
      _spriteBatch.Begin();
      const string line1 = "You Win!";
      const string line2 = "Press R to play again";
      Vector2 size1 = _font.MeasureString(line1);
      Vector2 size2 = _font.MeasureString(line2);
      Vector2 center = new(ViewportWidth * 0.5f, ViewportHeight * 0.5f);
      _spriteBatch.Draw(_pixel, new Rectangle(0, 0, ViewportWidth, ViewportHeight), new Color(0, 0, 0, 140));
      _spriteBatch.DrawString(_font, line1, new Vector2(center.X - size1.X * 0.5f, center.Y - size1.Y - 4), Color.White);
      _spriteBatch.DrawString(_font, line2, new Vector2(center.X - size2.X * 0.5f, center.Y + 4), new Color(200, 200, 200));
      _spriteBatch.End();
    }

    base.Draw(gameTime);
  }
}
