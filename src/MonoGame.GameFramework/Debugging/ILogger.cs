namespace MonoGame.GameFramework.Debugging;

public enum LogLevel
{
  Debug,
  Info,
  Warn,
  Error
}

public interface ILogger
{
  void Log(LogLevel level, string message);
  void Debug(string message);
  void Info(string message);
  void Warn(string message);
  void Error(string message);
}
