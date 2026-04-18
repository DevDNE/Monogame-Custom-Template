using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.GameFramework.Input;
using MonoGame.GameFramework.Platformer.Entities;

namespace MonoGame.GameFramework.Platformer;

public class Game1 : Game
{
  private readonly ServiceProvider _serviceProvider;
  private readonly GraphicsDeviceManager _graphics;
  private SpriteBatch _spriteBatch;
  private KeyboardManager _keyboardManager;
  private Texture2D _pixel;
  private Player _player;
  private List<Platform> _platforms;

  public Game1(ServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
    _graphics = new GraphicsDeviceManager(this)
    {
      PreferredBackBufferWidth = 1024,
      PreferredBackBufferHeight = 576,
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

    _player = new Player(new Vector2(496, 60));
    _platforms = new List<Platform>
    {
      new(new Rectangle(0, 540, 1024, 36)),     // ground
      new(new Rectangle(130, 420, 180, 24)),    // left floating
      new(new Rectangle(740, 420, 180, 24)),    // right floating
      new(new Rectangle(430, 340, 180, 24)),    // middle elevated
      new(new Rectangle(430, 180, 180, 24)),    // ceiling over middle
    };
  }

  protected override void Update(GameTime gameTime)
  {
    _keyboardManager.Update();
    if (_keyboardManager.IsKeyDown(Keys.Escape)) Exit();
    if (_keyboardManager.WasKeyPressed(Keys.R)) _player.Respawn();

    float inputX = 0f;
    if (_keyboardManager.IsKeyDown(Keys.A) || _keyboardManager.IsKeyDown(Keys.Left)) inputX -= 1f;
    if (_keyboardManager.IsKeyDown(Keys.D) || _keyboardManager.IsKeyDown(Keys.Right)) inputX += 1f;
    bool jumpPressed = _keyboardManager.WasKeyPressed(Keys.Space);

    _player.Update(gameTime, _platforms, inputX, jumpPressed);
    base.Update(gameTime);
  }

  protected override void Draw(GameTime gameTime)
  {
    GraphicsDevice.Clear(new Color(20, 24, 40));
    _spriteBatch.Begin();
    foreach (Platform p in _platforms) p.Draw(_spriteBatch, _pixel);
    _player.Draw(_spriteBatch, _pixel);
    _spriteBatch.End();
    base.Draw(gameTime);
  }
}
