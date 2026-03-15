using System;

namespace MonoGame.GameFramework.Events;

public class GameEventArgs : EventArgs
{
  public string Message { get; }

  public GameEventArgs(string message)
  {
    Message = message;
  }
}
