namespace MonoGame.GameFramework.Persistence;

public class SaveFile<T>
{
  public int Version { get; set; } = 1;
  public T Data { get; set; }
}
