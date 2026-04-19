using System;
using System.IO;
using Newtonsoft.Json;

namespace MonoGame.GameFramework.Persistence;
public class SettingsManager
{
  private readonly string settingsFilePath;
  public string WindowTitle { get; set; }
  public int WindowWidth { get; set; }
  public int WindowHeight { get; set; }
  public bool IsFullScreen { get; set; }
  public bool IsBorderless { get; set; }
  public SettingsManager(string settingsFilePath = null)
  {
    WindowTitle = AppDomain.CurrentDomain.FriendlyName;
    WindowWidth = 800;
    WindowHeight = 600;
    IsFullScreen = false;
    IsBorderless = false;
    this.settingsFilePath = settingsFilePath;
  }

  public void SaveSettings()
  {
    if (settingsFilePath == null) return;
    string json = JsonConvert.SerializeObject(this);
    File.WriteAllText(settingsFilePath, json);
  }

  public void LoadSettings()
  {
    if (settingsFilePath != null && File.Exists(settingsFilePath))
    {
      string json = File.ReadAllText(settingsFilePath);
      SettingsManager settings = JsonConvert.DeserializeObject<SettingsManager>(json);

      WindowTitle = settings.WindowTitle;
      WindowWidth = settings.WindowWidth;
      WindowHeight = settings.WindowHeight;
      IsFullScreen = settings.IsFullScreen;
      IsBorderless = settings.IsBorderless;
    }
  }
}
