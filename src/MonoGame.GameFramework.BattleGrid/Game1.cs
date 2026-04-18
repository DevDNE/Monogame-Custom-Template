using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.GameFramework.Audio;
using MonoGame.GameFramework.Input;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Persistence;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.Text;
using MonoGame.GameFramework.UI;
using MonoGame.GameFramework.BattleGrid.GameStates;

namespace MonoGame.GameFramework.BattleGrid;

public class Game1 : Game
{
    private readonly ServiceProvider _serviceProvider;
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;

    private SettingsManager _settingsManager;
    private TextManager _textManager;
    private SoundManager _soundManager;
    private KeyboardManager _keyboardManager;
    private MouseManager _mouseManager;
    private GamePadManager _gamePadManager;
    private UIManager _uiManager;
    private GameStateManager _gameStateManager;
    private SceneManager _sceneManager;

    public Game1(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _settingsManager = serviceProvider.GetService<SettingsManager>();
        _settingsManager.LoadSettings();
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = _settingsManager.WindowWidth,
            PreferredBackBufferHeight = _settingsManager.WindowHeight,
            IsFullScreen = _settingsManager.IsFullScreen
        };
        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Window.Title = _settingsManager.WindowTitle;
        _textManager = _serviceProvider.GetService<TextManager>();
        _soundManager = _serviceProvider.GetService<SoundManager>();
        _keyboardManager = _serviceProvider.GetService<KeyboardManager>();
        _mouseManager = _serviceProvider.GetService<MouseManager>();
        _gamePadManager = _serviceProvider.GetService<GamePadManager>();
        _uiManager = _serviceProvider.GetService<UIManager>();
        _gameStateManager = _serviceProvider.GetService<GameStateManager>();
        _sceneManager = _serviceProvider.GetService<SceneManager>();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Primitives.Initialize(GraphicsDevice);
        _font = Content.Load<SpriteFont>("fonts/Arial");
        _textManager.LoadContent(_font);
        _soundManager.LoadContent(Content);
        _sceneManager.LoadContent(Content);

        int vw = _settingsManager.WindowWidth;
        int vh = _settingsManager.WindowHeight;

        PlayState playState = new(_serviceProvider);
        TitleState titleState = new(
            _serviceProvider, _font, vw, vh,
            onPlay: () => _gameStateManager.ChangeState(playState),
            onQuit: Exit);

        _gameStateManager.PushState(titleState);
    }

    protected override void Update(GameTime gameTime)
    {
        _keyboardManager.Update();
        _mouseManager.Update();
        _gamePadManager.Update();
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

    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
        base.OnExiting(sender, args);
    }
}
