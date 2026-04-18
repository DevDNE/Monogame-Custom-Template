using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.GameFramework.Input;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Platformer.GameStates;
using MonoGame.GameFramework.UI;

namespace MonoGame.GameFramework.Platformer;

public class Game1 : Game
{
  private const int ViewportWidth = 1024;
  private const int ViewportHeight = 576;

  private readonly ServiceProvider _serviceProvider;
  private readonly GraphicsDeviceManager _graphics;
  private SpriteBatch _spriteBatch;
  private Texture2D _pixel;
  private SpriteFont _font;
  private KeyboardManager _keyboardManager;
  private MouseManager _mouseManager;
  private UIManager _uiManager;
  private GameStateManager _gameStateManager;

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
    _mouseManager = _serviceProvider.GetService<MouseManager>();
    _uiManager = _serviceProvider.GetService<UIManager>();
    _gameStateManager = _serviceProvider.GetService<GameStateManager>();
    base.Initialize();
  }

  protected override void LoadContent()
  {
    _spriteBatch = new SpriteBatch(GraphicsDevice);
    _pixel = new Texture2D(GraphicsDevice, 1, 1);
    _pixel.SetData(new[] { Color.White });
    _font = Content.Load<SpriteFont>("fonts/Arial");

    PlayState playState = new(_serviceProvider, _pixel, _font, ViewportWidth, ViewportHeight);
    TitleState titleState = new(
      _serviceProvider, _pixel, _font, ViewportWidth, ViewportHeight,
      onPlay: () =>
      {
        _gameStateManager.PeekState().Leaving();
        _gameStateManager.ChangeState(playState);
        _gameStateManager.PeekState().Entered();
      },
      onQuit: Exit);

    _gameStateManager.PushState(titleState);
    _gameStateManager.PeekState().Entered();
  }

  protected override void Update(GameTime gameTime)
  {
    _keyboardManager.Update();
    _mouseManager.Update();
    if (_keyboardManager.IsKeyDown(Keys.Escape)) Exit();

    _uiManager.Update(gameTime);
    _gameStateManager.Update(gameTime);

    base.Update(gameTime);
  }

  protected override void Draw(GameTime gameTime)
  {
    GraphicsDevice.Clear(new Color(20, 24, 40));
    _gameStateManager.Draw(_spriteBatch, gameTime);
    base.Draw(gameTime);
  }
}
