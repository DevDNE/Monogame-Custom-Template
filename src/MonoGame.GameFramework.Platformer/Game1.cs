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
  private Platform _ground;

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
    _ground = new Platform(new Rectangle(0, 500, 1024, 76));
  }

  protected override void Update(GameTime gameTime)
  {
    _keyboardManager.Update();
    if (_keyboardManager.IsKeyDown(Keys.Escape)) Exit();
    if (_keyboardManager.WasKeyPressed(Keys.R)) _player.Respawn();

    _player.Update(gameTime, _ground);
    base.Update(gameTime);
  }

  protected override void Draw(GameTime gameTime)
  {
    GraphicsDevice.Clear(new Color(20, 24, 40));
    _spriteBatch.Begin();
    _ground.Draw(_spriteBatch, _pixel);
    _player.Draw(_spriteBatch, _pixel);
    _spriteBatch.End();
    base.Draw(gameTime);
  }
}
