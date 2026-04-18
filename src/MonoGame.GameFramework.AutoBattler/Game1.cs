using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.GameFramework.AutoBattler.GameStates;
using MonoGame.GameFramework.Input;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.UI;

namespace MonoGame.GameFramework.AutoBattler;

public class Game1 : Game
{
  private const int ViewportWidth = 760;
  private const int ViewportHeight = 720;

  private readonly ServiceProvider _serviceProvider;
  private readonly GraphicsDeviceManager _graphics;
  private SpriteBatch _spriteBatch;
  private SpriteFont _font;
  private KeyboardManager _keyboardManager;
  private MouseManager _mouseManager;
  private UIManager _uiManager;
  private GameStateManager _gameStateManager;

  private readonly GameModel _model = new();

  private TitleState _titleState;
  private ShopState _shopState;
  private CombatState _combatState;
  private PostCombatState _postCombatState;

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
    Window.Title = "Auto Battler";
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
    Primitives.Initialize(GraphicsDevice);
    _font = Content.Load<SpriteFont>("fonts/Arial");

    _shopState = new ShopState(_serviceProvider, _font, _model, ViewportWidth, ViewportHeight,
      onStartCombat: () => _gameStateManager.ChangeState(_combatState));

    _combatState = new CombatState(_serviceProvider, _font, _model, ViewportWidth, ViewportHeight,
      onCombatEnded: winner =>
      {
        _postCombatState.Configure(winner);
        _gameStateManager.ChangeState(_postCombatState);
      });

    _postCombatState = new PostCombatState(_serviceProvider, _font, _model, ViewportWidth, ViewportHeight,
      onNextRound: () => _gameStateManager.ChangeState(_shopState),
      onRestart: () =>
      {
        _model.Reset();
        _gameStateManager.ChangeState(_titleState);
      });

    _titleState = new TitleState(_serviceProvider, _font, ViewportWidth, ViewportHeight,
      onPlay: () => { _model.Reset(); _gameStateManager.ChangeState(_shopState); },
      onQuit: Exit);

    _gameStateManager.PushState(_titleState);
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
    GraphicsDevice.Clear(new Color(18, 22, 34));
    _gameStateManager.Draw(_spriteBatch, gameTime);
    base.Draw(gameTime);
  }
}
