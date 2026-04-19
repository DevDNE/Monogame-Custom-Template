using Microsoft.Extensions.DependencyInjection;
using dotenv.net;
using MonoGame.GameFramework.Core;
using MonoGame.GameFramework.Testing;

namespace MonoGame.GameFramework.BattleGrid;

public class Program
{
    public static void Main(string[] args)
    {
        DotEnv.Load(new DotEnvOptions(envFilePaths: new[] { "..\\..\\..\\..\\.env" }));
        var settingsFilePath = System.Environment.GetEnvironmentVariable("SETTINGS_FILE_PATH");

        var serviceProvider = new ServiceCollection()
            .AddGameFrameworkManagers(settingsFilePath)
            .BuildServiceProvider();

        serviceProvider.GetService<SmokeHarness>().ExitAfterFrames = SmokeHarness.ParseExitAfter(args);

        using var game = new Game1(serviceProvider);
        game.Run();
    }
}
