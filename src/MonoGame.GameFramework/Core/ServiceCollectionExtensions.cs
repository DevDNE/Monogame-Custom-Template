using Microsoft.Extensions.DependencyInjection;
using MonoGame.GameFramework.Audio;
using MonoGame.GameFramework.Content;
using MonoGame.GameFramework.Debugging;
using MonoGame.GameFramework.Events;
using MonoGame.GameFramework.Input;
using MonoGame.GameFramework.Lifecycle;
using MonoGame.GameFramework.Persistence;
using MonoGame.GameFramework.Rendering;
using MonoGame.GameFramework.Text;
using MonoGame.GameFramework.Timing;
using MonoGame.GameFramework.UI;

namespace MonoGame.GameFramework.Core;
public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddGameFrameworkManagers(this IServiceCollection services, string settingsFilePath = null)
  {
    services.AddSingleton(new SettingsManager(settingsFilePath));
    services.AddSingleton<AssetCatalog>();
    services.AddSingleton<ILogger, ConsoleLogger>();
    services.AddSingleton<SaveSystem>();
    services.AddSingleton<TimerManager>();
    services.AddSingleton<DrawManager>();
    services.AddSingleton<EventManager>();
    services.AddSingleton<GamePadManager>();
    services.AddSingleton<GameStateManager>();
    services.AddSingleton<KeyboardManager>();
    services.AddSingleton<MouseManager>();
    services.AddSingleton<SceneManager>();
    services.AddSingleton<SoundManager>();
    services.AddSingleton<TextManager>();
    services.AddSingleton<UIManager>();
    services.AddSingleton<DebugOverlay>();
    return services;
  }
}
