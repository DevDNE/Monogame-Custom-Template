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

  private readonly ServiceProvider _serviceProvider;
  private readonly GraphicsDeviceManager _graphics;
  private SpriteBatch _spriteBatch;
  private KeyboardManager _keyboardManager;
  private Texture2D _pixel;
  private Player _player;
  private List<Platform> _platforms;
  private Camera2D _camera;

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

    _player = new Player(new Vector2(200, 60));
    _platforms = new List<Platform>
    {
      new(new Rectangle(0, 540, 2400, 36)),     // extended ground
      new(new Rectangle(130, 420, 180, 24)),
      new(new Rectangle(430, 340, 180, 24)),
      new(new Rectangle(740, 420, 180, 24)),
      new(new Rectangle(430, 180, 180, 24)),    // ceiling over middle
      new(new Rectangle(1100, 440, 180, 24)),   // right-side reach
      new(new Rectangle(1400, 360, 180, 24)),
      new(new Rectangle(1700, 280, 180, 24)),
      new(new Rectangle(2000, 380, 260, 24)),
    };

    _camera = new Camera2D(new Vector2(ViewportWidth, ViewportHeight))
    {
      Position = PlayerCenter(),
      Target = PlayerCenter(),
      FollowLerp = 0.12f,
    };
  }

  private Vector2 PlayerCenter() => _player.Position + new Vector2(Player.Width * 0.5f, Player.Height * 0.5f);

  protected override void Update(GameTime gameTime)
  {
    _keyboardManager.Update();
    if (_keyboardManager.IsKeyDown(Keys.Escape)) Exit();
    if (_keyboardManager.WasKeyPressed(Keys.R)) _player.Respawn();

    float inputX = 0f;
    if (_keyboardManager.IsKeyDown(Keys.A) || _keyboardManager.IsKeyDown(Keys.Left)) inputX -= 1f;
    if (_keyboardManager.IsKeyDown(Keys.D) || _keyboardManager.IsKeyDown(Keys.Right)) inputX += 1f;
    bool jumpPressed = _keyboardManager.WasKeyPressed(Keys.Space);
    bool jumpHeld = _keyboardManager.IsKeyDown(Keys.Space);

    _player.Update(gameTime, _platforms, inputX, jumpPressed, jumpHeld);

    _camera.Target = PlayerCenter();
    _camera.Update(gameTime);

    base.Update(gameTime);
  }

  protected override void Draw(GameTime gameTime)
  {
    GraphicsDevice.Clear(new Color(20, 24, 40));
    _spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);
    foreach (Platform p in _platforms) p.Draw(_spriteBatch, _pixel);
    _player.Draw(_spriteBatch, _pixel);
    _spriteBatch.End();
    base.Draw(gameTime);
  }
}
