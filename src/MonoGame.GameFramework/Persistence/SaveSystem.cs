using System.IO;
using Newtonsoft.Json;

namespace MonoGame.GameFramework.Persistence;

public class SaveSystem
{
  public void Save<T>(string path, T data, int version = 1)
  {
    SaveFile<T> file = new() { Version = version, Data = data };
    string json = JsonConvert.SerializeObject(file, Formatting.Indented);
    string directory = Path.GetDirectoryName(path);
    if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
    File.WriteAllText(path, json);
  }

  public bool TryLoad<T>(string path, out SaveFile<T> file)
  {
    file = null;
    if (!File.Exists(path)) return false;
    string json = File.ReadAllText(path);
    file = JsonConvert.DeserializeObject<SaveFile<T>>(json);
    return file != null;
  }

  public bool Exists(string path) => File.Exists(path);

  public bool Delete(string path)
  {
    if (!File.Exists(path)) return false;
    File.Delete(path);
    return true;
  }
}
