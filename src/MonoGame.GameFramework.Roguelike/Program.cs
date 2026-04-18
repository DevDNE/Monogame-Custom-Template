using Microsoft.Extensions.DependencyInjection;
using MonoGame.GameFramework.Core;

namespace MonoGame.GameFramework.Roguelike;

public class Program
{
  public static void Main()
  {
    ServiceProvider serviceProvider = new ServiceCollection()
      .AddGameFrameworkManagers()
      .BuildServiceProvider();

    using Game1 game = new(serviceProvider);
    game.Run();
  }
}
