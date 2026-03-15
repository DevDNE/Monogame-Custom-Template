using Microsoft.Extensions.DependencyInjection;
using dotenv.net;
using MonoGame.GameFramework.DependencyInjection;

namespace MonoGame.GameFramework.Demo;

public class Program
{
    public static void Main()
    {
        DotEnv.Load(new DotEnvOptions(envFilePaths: new[] { "..\\..\\..\\..\\.env" }));
        var settingsFilePath = System.Environment.GetEnvironmentVariable("SETTINGS_FILE_PATH");

        var serviceProvider = new ServiceCollection()
            .AddGameFrameworkManagers(settingsFilePath)
            .BuildServiceProvider();

        using var game = new Game1(serviceProvider);
        game.Run();
    }
}
