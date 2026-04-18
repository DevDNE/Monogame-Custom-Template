using System;

namespace MonoGame.GameFramework.Debugging;

public class ConsoleLogger : ILogger
{
  public LogLevel MinimumLevel { get; set; } = LogLevel.Info;

  public void Log(LogLevel level, string message)
  {
    if (level < MinimumLevel) return;
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] {message}");
  }

  public void Debug(string message) => Log(LogLevel.Debug, message);
  public void Info(string message) => Log(LogLevel.Info, message);
  public void Warn(string message) => Log(LogLevel.Warn, message);
  public void Error(string message) => Log(LogLevel.Error, message);
}
