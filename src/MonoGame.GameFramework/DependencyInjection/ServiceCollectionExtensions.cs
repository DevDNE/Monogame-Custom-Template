using Microsoft.Extensions.DependencyInjection;
using MonoGame.GameFramework.Managers;

namespace MonoGame.GameFramework.DependencyInjection;
public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddGameFrameworkManagers(this IServiceCollection services, string settingsFilePath = null)
  {
    services.AddSingleton(new SettingsManager(settingsFilePath));
    services.AddSingleton<DrawManager>();
    services.AddSingleton<EventManager>();
    services.AddSingleton<GamePadManager>();
    services.AddSingleton<GameStateManager>();
    services.AddSingleton<KeyboardManager>();
    services.AddSingleton<MouseManager>();
    services.AddSingleton<SceneManager>();
    services.AddSingleton<SoundManager>();
    services.AddSingleton<SteamworksManager>();
    services.AddSingleton<TextManager>();
    services.AddSingleton<UIManager>();
    return services;
  }
}
